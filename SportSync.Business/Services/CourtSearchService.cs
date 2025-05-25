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

        // ---------------------------------------------------  MAIN
        public async Task<IReadOnlyList<CourtComplexResultDto>> SearchAsync(
            CourtSearchRequest rq, CancellationToken ct = default)
        {
            int dow = (int)rq.Date.DayOfWeek;

            // ------------ base filter CourtComplex
            var complexesQ = _db.CourtComplexes
                .AsNoTracking()
                .Include(c => c.Courts) // cần Courts để join tiếp
                .Where(c => c.ApprovalStatus == ApprovalStatus.Approved &&
                            c.IsActiveByOwner && c.IsActiveByAdmin);

            if (!string.IsNullOrWhiteSpace(rq.City))
                complexesQ = complexesQ.Where(c => c.City == rq.City);

            if (!string.IsNullOrWhiteSpace(rq.District))
                complexesQ = complexesQ.Where(c => c.District == rq.District);

            // ------------ materialize tối thiểu field cần (để tính distance)
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
                                     (!rq.SportTypeId.HasValue ||
                                       co.SportTypeId == rq.SportTypeId))
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
                                // --- chưa block
                                .Where(ts => !_db.BlockedCourtSlots.Any(b =>
                                             b.CourtId == co.CourtId &&
                                             b.BlockDate == rq.Date &&
                                             b.StartTime < ts.EndTime &&
                                             b.EndTime > ts.StartTime))
                                // --- chưa booking confirmed
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

            // ------------ distance + bán kính & mapping
            var list = raw
                .Select(c =>
                {
                    double? dist = (rq.UserLat is null || c.Latitude is null)
                        ? null
                        : DistanceKm(rq.UserLat.Value, rq.UserLng!.Value,
                                     (double)c.Latitude!, (double)c.Longitude!);

                    return new { c, dist };
                })
               .Where(x => rq.UserLat == null || rq.UserLng == null
                        ? true
                        : (x.dist ?? double.MaxValue) <= rq.RadiusKm)
                .Select(x => new CourtComplexResultDto
                {
                    ComplexId = x.c.CourtComplexId,
                    Name = x.c.Name,
                    Address = x.c.Address,
                    ThumbnailUrl = x.c.MainImageCloudinaryUrl,
                    DistanceKm = x.dist,

                    Courts = x.c.Courts
                              .Where(co => co.Slots.Any())        // ít nhất 1 slot
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
                .OrderBy(c => c.DistanceKm ?? double.MaxValue)      // gần nhất lên đầu
                .ThenBy(c => c.Courts.Min(co => co.MinPrice))
                .ToList();

            return list;
        }
    }
}
