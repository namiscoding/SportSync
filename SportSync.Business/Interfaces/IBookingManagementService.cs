using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportSync.Business.Dtos;
using SportSync.Data.Enums;

namespace SportSync.Business.Interfaces
{
    public interface IBookingManagementService
    {
        Task<WeeklyScheduleDto> GetWeeklyScheduleAsync(int courtId, DateTime dateInWeek, string ownerUserId);
        Task<(bool Success, string ErrorMessage)> CreateBulkManualBookingAsync(CreateBulkManualBookingDto dto, string ownerUserId);
        Task<IEnumerable<BookingListItemDto>> GetBookingsForOwnerAsync(string ownerUserId);
        Task<(bool Success, string ErrorMessage)> UpdateBookingStatusAsync(long bookingId, BookingStatusType newStatus, string ownerUserId);
        Task<BookingBillDto> GetBookingDetailsForBillAsync(long bookingId, string ownerUserId);
    }
}
