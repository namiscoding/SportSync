using SportSync.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Dtos
{
    public sealed record CreateBookingDto
    {
        public string BookerUserId { get; init; } = null!;  // bắt buộc
        public int CourtId { get; init; }
        public DateOnly Date { get; init; }

        public IReadOnlyList<int> SlotIds { get; init; } = Array.Empty<int>();

        /* tuỳ chọn */
        public string? NotesFromBooker { get; init; }
        public PaymentMethodType PaymentType { get; init; } = PaymentMethodType.BankTransfer;
    }
    public sealed class BookingInvoiceDto
    {
        public long BookingId { get; init; }

        public string ComplexName { get; init; } = null!;
        public string CourtName { get; init; } = null!;
        public string? CourtAddress { get; init; }   // tuỳ thích
        public DateOnly BookingDate { get; init; }

        public string? PhoneNumber { get; set; } 
        public IReadOnlyList<InvoiceSlotDto> Slots { get; init; } = [];

        public decimal TotalPrice { get; init; }
    }

    public sealed class InvoiceSlotDto
    {
        public string TimeRange { get; init; } = null!; // “07:00-08:00”
        public decimal Price { get; init; }
    }
}
