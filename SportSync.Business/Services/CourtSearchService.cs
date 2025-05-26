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

namespace SportSync.Business.Services
{
    public sealed class CourtSearchService : ICourtSearchService
    {
        private readonly ApplicationDbContext _db;
        public CourtSearchService(ApplicationDbContext db) => _db = db;

        // ---------------------------------------------------  Haversine (km)


        public async Task<IReadOnlyList<CourtComplexResultDto>> SearchAsync(
    CourtSearchRequest rq, CancellationToken ct = default)
        {
            int dow = (int)rq.Date.DayOfWeek;

            // Lọc CourtComplex theo trạng thái, City, District
            var complexesQ = _db.CourtComplexes
                .AsNoTracking()
                .Include(c => c.Courts)
                .Where(c => c.ApprovalStatus == ApprovalStatus.Approved &&
                            c.IsActiveByOwner && c.IsActiveByAdmin);

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

            // Lấy dữ liệu court + slots
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
                                          .Select(a => a.Amenity.Name),

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
                                .Select(ts => new TimeSlotDto
                                {
                                    TimeSlotId = ts.TimeSlotId,
                                    Start = ts.StartTime,
                                    End = ts.EndTime,
                                    Price = ts.Price
                                })
                        })
                })
                .ToListAsync(ct);

            // Bỏ lọc khoảng cách và bán kính (UserLat, UserLng không dùng)
            var list = raw
                .Select(c => new CourtComplexResultDto
                {
                    ComplexId = c.CourtComplexId,
                    Name = c.Name,
                    Address = c.Address,
                    ThumbnailUrl = c.MainImageCloudinaryUrl,
                    DistanceKm = null, // không tính khoảng cách
                    Latitude = c.Latitude,
                    Longitude = c.Longitude,

                    Courts = c.Courts
                              .Where(co => co.Slots.Any())        // có ít nhất 1 slot
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
                .Where(c => c.Courts.Any())  // loại bỏ complex không có sân phù hợp
                .OrderBy(c => c.Name)        // sắp xếp theo tên complex hoặc có thể theo tiêu chí khác
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

            // Chuyển rq.Date sang DateTime.Date để so sánh đúng kiểu với DB
            DateTime queryDate = rq?.Date.ToDateTime(TimeOnly.MinValue).Date ?? DateTime.Today.Date;
            DateOnly queryDateOnly = DateOnly.FromDateTime(queryDate);
            var fromTime = rq?.FromTime ?? TimeOnly.MinValue;
            var toTime = rq?.ToTime ?? TimeOnly.MaxValue;

            var complexesQ = _db.CourtComplexes
                .AsNoTracking()
                .Include(c => c.Courts)
                .Where(c => c.ApprovalStatus == ApprovalStatus.Approved &&
                            c.IsActiveByOwner && c.IsActiveByAdmin);

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

                            Amenities = co.CourtAmenities.Select(a => a.Amenity.Name).ToList(),

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
                                .Select(ts => new TimeSlotDto
                                {
                                    TimeSlotId = ts.TimeSlotId,
                                    Start = ts.StartTime,
                                    End = ts.EndTime,
                                    Price = ts.Price
                                })
                                .ToList()
                        })
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
                    Latitude = x.c.Latitude,        // Trả về Latitude
                    Longitude = x.c.Longitude,      // Trả về Longitude

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
