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

        public Task<(bool, string?, long?)> CreateAsync(CreateBookingDto dto, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<BookingInvoiceDto?> GetInvoiceAsync(long bookingId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
