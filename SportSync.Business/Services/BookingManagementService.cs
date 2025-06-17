using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SportSync.Business.Dtos;
using SportSync.Business.Interfaces;
using SportSync.Data;
using SportSync.Data.Entities;
using SportSync.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace SportSync.Business.Services
{
    public class BookingManagementService : IBookingManagementService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookingManagementService> _logger;

        public BookingManagementService(ApplicationDbContext context, ILogger<BookingManagementService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Task<(bool Success, string ErrorMessage)> CreateBulkManualBookingAsync(CreateBulkManualBookingDto dto, string ownerUserId)
        {
            throw new NotImplementedException();
        }

        public Task<BookingBillDto> GetBookingDetailsForBillAsync(long bookingId, string ownerUserId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<BookingListItemDto>> GetBookingsForOwnerAsync(string ownerUserId)
        {
            throw new NotImplementedException();
        }

        public Task<WeeklyScheduleDto> GetWeeklyScheduleAsync(int courtId, DateTime dateInWeek, string ownerUserId)
        {
            throw new NotImplementedException();
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateBookingStatusAsync(long bookingId, BookingStatusType newStatus, string ownerUserId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Court).ThenInclude(c => c.CourtComplex)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null || booking.Court.CourtComplex.OwnerUserId != ownerUserId)
            {
                return (false, "Không tìm thấy đơn đặt hoặc bạn không có quyền thay đổi.");
            }

            // TODO: Thêm logic kiểm tra xem việc chuyển trạng thái có hợp lệ không
            // Ví dụ: chỉ có thể duyệt đơn đang ở trạng thái "Chờ xác nhận"
            // if (booking.BookingStatus != BookingStatusType.PendingConfirmation)
            // {
            //     return (false, "Không thể duyệt một đơn không ở trạng thái 'Chờ xác nhận'.");
            // }

            _logger.LogInformation("Updating booking {BookingId} status from {OldStatus} to {NewStatus} by user {OwnerUserId}",
                bookingId, booking.BookingStatus, newStatus, ownerUserId);

            booking.BookingStatus = newStatus;
            booking.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating status for booking {BookingId}", bookingId);
                return (false, "Lỗi khi cập nhật cơ sở dữ liệu.");
            }
        }
    }
}
