using SportSync.Business.Dtos;
using SportSync.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Interfaces
{
    public interface IBookingService
    {
        Task<BookingResultDto> CreateBookingAsync(CreateBookingRequestDto request, string bookerUserId, CancellationToken ct = default);

        Task<BookingDetailDto?> GetBookingDetailAsync(long bookingId, string userId, CancellationToken ct = default);
    }
}
