using SportSync.Business.Dtos;
using SportSync.Business.Interfaces;
using SportSync.Data.Enums;
using SportSync.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SportSync.Data.Entities;

namespace SportSync.Business.Services
{
    public sealed class CourtSearchService : ICourtSearchService
    {
        private readonly ApplicationDbContext _db;
        public CourtSearchService(ApplicationDbContext db) => _db = db;


        public async Task<IReadOnlyList<CourtComplexResultDto>> SearchAsync(
    CourtSearchRequest rq, CancellationToken ct = default)
        {
            int dow = (int)rq.Date.DayOfWeek;

            var complexesQ = _db.CourtComplexes
                .AsNoTracking()
                .Include(c => c.Courts)
                .Where(c => c.IsActiveByOwner);

            if (!string.IsNullOrWhiteSpace(rq.City))
            {
                var city = rq.City.Trim().ToLower();
                complexesQ = complexesQ.Where(c =>
                    c.City != null &&
                    EF.Functions.Like(c.City.ToLower(), $"%{city}%"));
            }

            if (!string.IsNullOrWhiteSpace(rq.District))
            {
                var district = rq.District.Trim().ToLower();
                complexesQ = complexesQ.Where(c =>
                    c.District != null &&
                    EF.Functions.Like(c.District.ToLower(), $"%{district}%"));
            }   

            var raw = await complexesQ
                .Select(cpx => new
                {
                    cpx.CourtComplexId,
                    cpx.Name,
                    cpx.Address,
                    cpx.MainImageCloudinaryUrl,
                    cpx.Longitude,
                    cpx.Latitude,
                    Courts = cpx.Courts
                        .Where(co => co.IsActiveByAdmin &&
                                     co.StatusByOwner == CourtStatusByOwner.Available &&
                                     (!rq.SportTypeId.HasValue || co.SportTypeId == rq.SportTypeId))
                        .Select(co => new
                        {
                            co.CourtId,
                            co.Name,
                            SportTypeName = co.SportType.Name,
                            ImageUrl = co.MainImageCloudinaryUrl,

                            Amenities = co.CourtAmenities
                                          .Select(a => a.Amenity.Name).Take(3),

                            Slots = co.TimeSlots
                                .Where(ts => ts.IsActiveByOwner &&
                                             (ts.DayOfWeek == null || (int)ts.DayOfWeek == dow) &&
                                             ts.StartTime >= (rq.FromTime ?? TimeOnly.MinValue) &&
                                             ts.EndTime <= (rq.ToTime ?? TimeOnly.MaxValue))
                                // Chưa block
                                .Where(ts => !_db.BlockedCourtSlots.Any(b =>
                                             b.CourtId == co.CourtId &&
                                             b.BlockDate == rq.Date &&
                                             b.StartTime < ts.EndTime &&
                                             b.EndTime > ts.StartTime))
                                // Chưa booking confirmed
                                .Where(ts => !_db.BookedSlots.Any(bs =>
                                             bs.TimeSlotId == ts.TimeSlotId &&
                                             bs.SlotDate == rq.Date &&
                                             bs.Booking.BookingStatus == BookingStatusType.Confirmed))
                                .OrderBy(ts => ts.Price)
                                .Take(2)
                                .Select(ts => new TimeSlotDto
                                {
                                    TimeSlotId = ts.TimeSlotId,
                                    Start = ts.StartTime,
                                    End = ts.EndTime,
                                    Price = ts.Price,
                                    PriceLowest = ts.Price
                                })
                        }).Take(2)
                })
                .ToListAsync(ct);


            var list = raw
                .Select(c => new CourtComplexResultDto
                {
                    ComplexId = c.CourtComplexId,
                    Name = c.Name,
                    Address = c.Address,
                    ThumbnailUrl = c.MainImageCloudinaryUrl,
                    DistanceKm = null, 
                    Latitude = c.Latitude,
                    Longitude = c.Longitude,

                    Courts = c.Courts
                              .Where(co => co.Slots.Any())      
                              .Select(co => new CourtWithSlotsDto
                              {
                                  CourtId = co.CourtId,
                                  Name = co.Name,
                                  SportTypeName = co.SportTypeName,
                                  ImageUrl = co.ImageUrl,
                                  Amenities = co.Amenities,
                                  AvailableSlots = co.Slots
                              })
                })
                .Where(c => c.Courts.Any()) 
                .OrderBy(c => c.Name)        
                .ToList();

            return list;
        }

        public async Task<IReadOnlyList<CourtComplexResultDto>> SearchNearbyAsync(
        double userLat, double userLng,
        double radiusKm = 10,
        CourtSearchRequest rq = null,
        CancellationToken ct = default)
        {
            int dow = rq?.Date.DayOfWeek != null ? (int)rq.Date.DayOfWeek : (int)DateTime.Today.DayOfWeek;
            DateTime queryDate = rq?.Date.ToDateTime(TimeOnly.MinValue).Date ?? DateTime.Today.Date;
            DateOnly queryDateOnly = DateOnly.FromDateTime(queryDate);
            var fromTime = rq?.FromTime ?? TimeOnly.MinValue;
            var toTime = rq?.ToTime ?? TimeOnly.MaxValue;

            var complexesQ = _db.CourtComplexes
                .AsNoTracking()
                .Include(c => c.Courts)
                .Where(c => c.IsActiveByOwner);


            if (!string.IsNullOrWhiteSpace(rq?.City))
                complexesQ = complexesQ.Where(c => c.City == rq.City);

            if (!string.IsNullOrWhiteSpace(rq?.District))
                complexesQ = complexesQ.Where(c => c.District == rq.District);

            var raw = await complexesQ
                .Select(cpx => new
                {
                    cpx.CourtComplexId,
                    cpx.Name,
                    cpx.Address,
                    cpx.Latitude,
                    cpx.Longitude,
                    cpx.MainImageCloudinaryUrl,

                    Courts = cpx.Courts
                        .Where(co => co.IsActiveByAdmin &&
                                     co.StatusByOwner == CourtStatusByOwner.Available &&
                                     (rq == null || !rq.SportTypeId.HasValue || co.SportTypeId == rq.SportTypeId))
                        .Select(co => new
                        {
                            co.CourtId,
                            co.Name,
                            SportTypeName = co.SportType.Name,
                            ImageUrl = co.MainImageCloudinaryUrl,

                            Amenities = co.CourtAmenities.Select(a => a.Amenity.Name).Take(3).ToList(),

                            Slots = co.TimeSlots
                                .Where(ts => ts.IsActiveByOwner &&
                                             (ts.DayOfWeek == null || (int)ts.DayOfWeek == dow) &&
                                             ts.StartTime >= fromTime &&
                                             ts.EndTime <= toTime)
                                .Where(ts => !_db.BlockedCourtSlots.Any(b =>
                                    b.CourtId == co.CourtId &&
                                    b.BlockDate == queryDateOnly &&
                                    b.StartTime < ts.EndTime &&
                                    b.EndTime > ts.StartTime))
                                .Where(ts => !_db.BookedSlots.Any(bs =>
                                    bs.TimeSlotId == ts.TimeSlotId &&
                                    bs.SlotDate == queryDateOnly &&
                                    bs.Booking.BookingStatus == BookingStatusType.Confirmed))
                                .OrderBy(ts => ts.Price).Take(2)
                                .Select(ts => new TimeSlotDto
                                {
                                    TimeSlotId = ts.TimeSlotId,
                                    Start = ts.StartTime,
                                    End = ts.EndTime,
                                    Price = ts.Price,
                                    PriceLowest = ts.Price
                                })
                                .ToList()
                        }).Take(2)
                        .ToList()
                })
                .ToListAsync(ct);

            var list = raw
                .Select(c =>
                {
                    if (c.Latitude == null || c.Longitude == null) return null;

                    double dist = DistanceKm(userLat, userLng, (double)c.Latitude.Value, (double)c.Longitude.Value);

                    if (dist > radiusKm)
                        return null;

                    return new { c, dist };
                })
                .Where(x => x != null)
                .Select(x => new CourtComplexResultDto
                {
                    ComplexId = x.c.CourtComplexId,
                    Name = x.c.Name,
                    Address = x.c.Address,
                    ThumbnailUrl = x.c.MainImageCloudinaryUrl,
                    DistanceKm = x.dist,
                    Latitude = x.c.Latitude,     
                    Longitude = x.c.Longitude,      

                    Courts = x.c.Courts
                        .Where(co => co.Slots.Any())
                        .Select(co => new CourtWithSlotsDto
                        {
                            CourtId = co.CourtId,
                            Name = co.Name,
                            SportTypeName = co.SportTypeName,
                            ImageUrl = co.ImageUrl,
                            Amenities = co.Amenities ?? Enumerable.Empty<string>(),
                            AvailableSlots = co.Slots ?? Enumerable.Empty<TimeSlotDto>()
                        })
                        .ToList()
                })
                .Where(c => c.Courts.Any())
                .OrderBy(c => c.DistanceKm)
                .ThenBy(c => c.Courts.Min(co => co.MinPrice))
                .ToList();

            return list;
        }
        public async Task<CourtDetailDto?> GetDetailAsync(int courtId,
                                                          DateOnly date,
                                                          CancellationToken ct = default)
        {

            var court = await _db.Courts
                .AsNoTracking()
                .Include(c => c.SportType)
                .Include(c => c.CourtAmenities)
                    .ThenInclude(ca => ca.Amenity)
                .Include(c => c.TimeSlots)
                .FirstOrDefaultAsync(c => c.CourtId == courtId
                                       && c.IsActiveByAdmin
                                       && c.StatusByOwner == CourtStatusByOwner.Available,
                                     ct);

            if (court is null) return null;

            int dow = (int)date.DayOfWeek;           

            var activeSlots = court.TimeSlots
                .Where(s => s.IsActiveByOwner &&
                            (s.DayOfWeek == null                      
                             || (int)s.DayOfWeek!.Value == dow))       
                .ToList();

            var available = new List<TimeSlotDto>();
            foreach (var s in activeSlots)
            {
                bool booked = await _db.BookedSlots
                    .AnyAsync(bs => bs.TimeSlotId == s.TimeSlotId
                                 && bs.SlotDate == date, ct);

                bool blocked = await _db.BlockedCourtSlots
                    .AnyAsync(bc => bc.CourtId == courtId
                                 && bc.BlockDate == date
                                 && bc.StartTime == s.StartTime, ct);

                if (!booked && !blocked)
                {
                    available.Add(new TimeSlotDto
                    {
                        TimeSlotId = s.TimeSlotId,
                        Start = s.StartTime,
                        End = s.EndTime,
                        Price = s.Price
                    });
                }
            }

            var amenities = (court.CourtAmenities ?? Enumerable.Empty<CourtAmenity>())
                .Where(ca => ca.Amenity != null)
                .Select(ca => new AmenityDto(ca.Amenity!.Name));

            return new CourtDetailDto
            {
                CourtId = court.CourtId,
                ComplexId = court.CourtComplexId,
                Name = court.Name,
                SportTypeName = court.SportType.Name,
                ImageUrl = court.MainImageCloudinaryUrl,
                Amenities = amenities,                       
                AvailableSlots = available.OrderBy(s => s.Start)  
                                              .ToList()
            };
        }


        private static double DistanceKm(
    double lat1, double lon1,
    double lat2, double lon2)
        {
            const double R = 6371.0;
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLon = (lon2 - lon1) * Math.PI / 180;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180) *
                    Math.Cos(lat2 * Math.PI / 180) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            return 2 * R * Math.Asin(Math.Sqrt(a));
        }

    }
}
