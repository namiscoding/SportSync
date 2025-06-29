using SportSync.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Dtos
{
    public sealed record CreateBookingRequestDto
    {
        public int CourtId { get; init; }

        /* Người đặt – sẽ được so khớp với user đăng nhập */
        public string BookerUserId { get; init; } = null!;

        /* Thời gian */
        public DateOnly PlayDate { get; init; }   // yyyy-MM-dd
        public TimeOnly StartTime { get; init; }   // HH:mm
        public TimeOnly EndTime { get; init; }
        public string? NotesFromBooker { get; init; }
    }
    public sealed record BookingResultDto
    {
        public long BookingId { get; init; }

        /* Thông tin sân/cụm */
        public string ComplexName { get; init; } = null!;
        public string CourtName { get; init; } = null!;
        public string? CourtAddress { get; init; }

        /* Thời gian */
        public DateOnly BookingDate { get; init; }
        public DateTime StartUtc { get; init; }
        public DateTime EndUtc { get; init; }

        /* Chi phí */
        public decimal CourtSubtotal { get; init; }

        /* Người đặt */
        public string BookerUserId { get; init; } = null!;
        public string? PhoneNumber { get; init; }
    }
    public sealed record BookingSlotDto
    {
        public string TimeRange { get; init; } = null!;   // "16:45 - 17:45"
        public decimal Price { get; init; }            // tiền của đoạn này
    }

    public sealed record BookingDetailDto          // dùng cho trang Success
    {
        public long BookingId { get; init; }

        // cụm & sân
        public string ComplexName { get; init; } = null!;
        public string CourtName { get; init; } = null!;
        public string? CourtAddress { get; init; }

        // thời gian
        public DateOnly BookingDate { get; init; }
        public DateTime StartUtc { get; init; }
        public DateTime EndUtc { get; init; }

        // chi phí
        public decimal TotalPrice { get; init; }
        public IReadOnlyList<BookingSlotDto> Slots { get; init; } = Array.Empty<BookingSlotDto>();

        // liên hệ
        public string? PhoneNumber { get; init; }
        public string QrUrl { get; set; } = default!;
    }

    }
