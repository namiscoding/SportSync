using SportSync.Business.Dtos;
using SportSync.Business.Interfaces;
using SportSync.Data.Entities;
using SportSync.Data.Enums;
using SportSync.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Numerics;
using System.Text.RegularExpressions;

namespace SportSync.Business.Services
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _db; 
        private readonly ILogger<BookingService> _log;
        private readonly IVietQrService _vietQrService;

        public BookingService(ApplicationDbContext db, ILogger<BookingService> log, IVietQrService vietQrService) {
            _db = db;
            _log = log;
            _vietQrService = vietQrService;
        }

        public async Task<BookingResultDto> CreateBookingAsync(
     CreateBookingRequestDto r, string userId, CancellationToken ct = default)
        {
            if (r.EndTime <= r.StartTime)
                throw new InvalidOperationException("Giờ kết thúc phải sau giờ bắt đầu.");

            /* 1. Lấy sân + giá */
            var court = await _db.Courts
                                 .Include(c => c.CourtComplex)
                                 .Include(c => c.HourlyPriceRates)
                                 .SingleOrDefaultAsync(c => c.CourtId == r.CourtId, ct)
                         ?? throw new KeyNotFoundException("Không tìm thấy sân.");

            /* 2. Ghép Date + Time */
            DateTime start = r.PlayDate.ToDateTime(r.StartTime, DateTimeKind.Local);
            DateTime end = r.PlayDate.ToDateTime(r.EndTime, DateTimeKind.Local);

            /* 3. Kiểm tra trùng & block (giữ nguyên) */
            bool overlaps = await _db.Bookings
     .AnyAsync(b => b.CourtId == r.CourtId &&
                    b.BookedStartTime <= end &&   
                    start <= b.BookedEndTime,  
                    ct);

            if (overlaps)
                throw new InvalidOperationException(
                    "Khoảng thời gian đã có người đặt – bạn cần chọn bắt đầu sau " +
                    "khi lịch trước đó kết thúc ít nhất 1 phút.");

            bool blocked = await _db.BlockedCourtSlots
                .AnyAsync(s => s.CourtId == r.CourtId &&
                               DateOnly.FromDateTime(s.BlockDate) == r.PlayDate &&
                               s.StartTime < r.EndTime.ToTimeSpan() &&
                               r.StartTime.ToTimeSpan() < s.EndTime, ct);
            if (blocked) throw new InvalidOperationException("Khoảng thời gian đang bị chặn.");

            /* 4. Tính tiền — theo phút */
            var day = start.DayOfWeek;
            var rateSet = court.HourlyPriceRates
                                .Where(hr => hr.DayOfWeek == null || hr.DayOfWeek == day)
                                .ToList();

            decimal subtotal = CalculateCourtSubtotal(rateSet, start, end);

            /* 5. Lưu booking */
            var booking = new Booking
            {
                BookerUserId = userId,
                NotesFromBooker = r.NotesFromBooker,
                CourtId = court.CourtId,
                CourtComplexId = court.CourtComplexId,

                BookedStartTime = start,
                BookedEndTime = end,
                TotalPrice = subtotal,
                BookingStatus = BookingStatusType.PendingOwnerConfirmation,
                CreatedAt = DateTime.UtcNow
            };
            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync(ct);

            /* 6. Trả kết quả */
            var user = await _db.Users.FindAsync(new object?[] { userId }, ct);
            return new BookingResultDto
            {
                BookingId = booking.BookingId,

                ComplexName = court.CourtComplex.Name,
                CourtName = court.Name,
                CourtAddress = court.CourtComplex.Address,

                BookingDate = r.PlayDate,
                StartUtc = booking.BookedStartTime,
                EndUtc = booking.BookedEndTime,

                CourtSubtotal = subtotal,

                BookerUserId = userId,
                PhoneNumber = user?.PhoneNumber
            };
        }

        public async Task<BookingDetailDto?> GetBookingDetailAsync(
        long bookingId, string userId, CancellationToken ct = default)
        {
            var booking = await _db.Bookings
                                   .Include(b => b.Court)
                                       .ThenInclude(c => c.CourtComplex)
                                   .Include(b => b.Court)
                                       .ThenInclude(c => c.HourlyPriceRates)
                                   .AsNoTracking()
                                   .SingleOrDefaultAsync(b => b.BookingId == bookingId
                                                           && b.BookerUserId == userId, ct);
            if (booking == null) return null;

            // tái tính các đoạn giờ & tiền (cùng thuật toán đã dùng ở Create)
            var rates = booking.Court.HourlyPriceRates
                          .Where(r => r.DayOfWeek == null ||
                                      r.DayOfWeek == booking.BookedStartTime.DayOfWeek)
                          .ToList();

            var slots = new List<BookingSlotDto>();
            var cur = booking.BookedStartTime;
            while (cur < booking.BookedEndTime)
            {
                var hr = rates.Single(r =>
                            r.StartTime <= cur.TimeOfDay && cur.TimeOfDay < r.EndTime);

                var segEnd = cur.Date.Add(hr.EndTime);
                if (segEnd > booking.BookedEndTime) segEnd = booking.BookedEndTime;

                var minutes = (decimal)(segEnd - cur).TotalMinutes;
                var price = minutes / 60m * hr.PricePerHour;

                slots.Add(new BookingSlotDto
                {
                    TimeRange = $"{cur:HH\\:mm} - {segEnd:HH\\:mm}",
                    Price = price
                });
                cur = segEnd;
            }

            var user = await _db.Users.FindAsync(new object?[] { userId }, ct);
            var complex = booking.Court.CourtComplex;
            var addInfo = BuildAddInfo(complex, booking);
            return new BookingDetailDto
            {
                BookingId = booking.BookingId,

                ComplexName =   complex.Name,
                CourtName = booking.Court.Name,
                CourtAddress = complex.Address,

                BookingDate = DateOnly.FromDateTime(booking.BookedStartTime),
                StartUtc = booking.BookedStartTime,
                EndUtc = booking.BookedEndTime,

                TotalPrice = booking.TotalPrice,
                Slots = slots,

                PhoneNumber = user?.PhoneNumber,
                QrUrl = await _vietQrService.GenerateAsync(
                    complex, booking.TotalPrice,
                    addInfo, ct)
            };
        }
        private static decimal CalculateCourtSubtotal(
        IReadOnlyCollection<HourlyPriceRate> rates,
        DateTime start, DateTime end)
        {
            if (start >= end) throw new ArgumentException("Start ≥ End");
            decimal total = 0m;
            var cur = start;

            while (cur < end)
            {
                var hr = rates.SingleOrDefault(x =>
                             x.StartTime <= cur.TimeOfDay &&
                             cur.TimeOfDay < x.EndTime)
                         ?? throw new InvalidOperationException(
                              $"Khoảng {cur:HH\\:mm} chưa có bảng giá.");

                DateTime segEnd = start.Date.Add(hr.EndTime);
                if (segEnd > end) segEnd = end;

                decimal minutes = (decimal)(segEnd - cur).TotalMinutes;
                total += (minutes / 60m) * hr.PricePerHour;
                cur = segEnd;                 // sang phân đoạn kế
            }
            return total;
        }

        private static string BuildAddInfo(CourtComplex cx, Booking b)
        { 
            string Normalize(string txt) =>
                Regex.Replace(
                        txt.Normalize(NormalizationForm.FormD),
                        @"\p{IsCombiningDiacriticalMarks}+",
                        "")
                    .ToUpperInvariant()
                    .Replace(" ", "");


            string courtPart = Normalize(b.Court.Name);
            if (courtPart.Length > 15) courtPart = courtPart[..15];

            string timePart = $"{b.BookedStartTime:HHmm}-{b.BookedEndTime:HHmm}";

            string raw = courtPart + timePart;
            return raw.Length > 25 ? raw[..25] : raw;
        }
    }
}
