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
        Task<(bool, string?, long?)> CreateAsync(CreateBookingDto dto,
                                        CancellationToken ct = default);

        Task<BookingInvoiceDto?> GetInvoiceAsync(long bookingId, CancellationToken ct = default);
    }
}
