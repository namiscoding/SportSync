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

namespace SportSync.Business.Services
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _db;
        public BookingService(ApplicationDbContext db) => _db = db;

        public async Task<(bool, string?, long?)> CreateAsync(CreateBookingDto dto,
                                                       CancellationToken ct = default)
        {

            var court = await _db.Courts
                                 .Include(c => c.CourtComplex)
                                 .FirstOrDefaultAsync(c => c.CourtId == dto.CourtId, ct);
            if (court == null) return (false, "Sân không tồn tại", null);

            var slots = await _db.TimeSlots
                                 .Where(ts => dto.SlotIds.Contains(ts.TimeSlotId))
                                 .ToListAsync(ct);
            if (slots.Count != dto.SlotIds.Count)
                return (false, "Có slot không hợp lệ", null);

            //Kiểm tra trùng 
            var clashing = await _db.BookedSlots
                .AnyAsync(bs => dto.SlotIds.Contains(bs.TimeSlotId) &&
                                bs.SlotDate == dto.Date, ct);
            if (clashing) return (false, "Một số slot đã được đặt trước", null);


            var total = slots.Sum(s => s.Price);


            var booking = new Booking
            {
                BookerUserId = dto.BookerUserId,
                CourtComplexId = court.CourtComplexId,
                CourtId = court.CourtId,
                BookingDate = dto.Date,
                TotalPrice = total,
                BookingStatus = BookingStatusType.PendingOwnerConfirmation,
                PaymentType = dto.PaymentType,
                PaymentStatus = PaymentStatusType.Unpaid,
                NotesFromBooker = dto.NotesFromBooker
            };
            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync(ct);

            foreach (var s in slots)
            {
                _db.BookedSlots.Add(new BookedSlot
                {
                    BookingId = booking.BookingId,
                    TimeSlotId = s.TimeSlotId,
                    SlotDate = dto.Date,
                    ActualStartTime = s.StartTime,
                    ActualEndTime = s.EndTime,
                    PriceAtBookingTime = s.Price
                });
            }
            await _db.SaveChangesAsync(ct);

            return (true, null, booking.BookingId);
        }

        public async Task<BookingInvoiceDto?> GetInvoiceAsync(long id,
                                                              CancellationToken ct = default)
        {
            var data = await _db.Bookings
                .Where(b => b.BookingId == id)
                .Select(b => new BookingInvoiceDto
                {
                    BookingId = b.BookingId,
                    CourtName = b.Court.Name,
                    PhoneNumber = b.CourtComplex.OwnerUser.PhoneNumber,
                    ComplexName = b.CourtComplex.Name,
                    BookingDate = b.BookingDate,
                    Slots = b.BookedSlots.Select(s => new InvoiceSlotDto
                    {
                        TimeRange = $"{s.ActualStartTime:hh\\:mm}-{s.ActualEndTime:hh\\:mm}",
                        Price = s.PriceAtBookingTime
                    }).ToList(),
                    TotalPrice = b.TotalPrice
                }).FirstOrDefaultAsync(ct);
            return data;
        }
    }
}
