using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportSync.Data.Enums;

namespace SportSync.Data.Entities
{
    public class Booking
    {
        public long BookingId { get; set; } // PK (BIGINT), Identity
        public string BookerUserId { get; set; } // FK references AspNetUsers(Id), NOT NULL
        public int CourtComplexId { get; set; } // FK references CourtComplexes(CourtComplexId), NOT NULL
        public int CourtId { get; set; } // FK references Courts(CourtId), NOT NULL
        public DateOnly BookingDate { get; set; } // NOT NULL (Ngày thực tế diễn ra việc đặt sân)
                                                  // Khác với CreatedAt là ngày tạo booking record
        public decimal TotalPrice { get; set; } // NOT NULL, DECIMAL(18,2)
        public BookingStatusType BookingStatus { get; set; } // NOT NULL, DEFAULT 1 (PendingOwnerConfirmation)
        public PaymentMethodType PaymentType { get; set; } // NOT NULL, DEFAULT 0 (PayAtCourt)
        public PaymentStatusType PaymentStatus { get; set; } // NOT NULL, DEFAULT 0 (Unpaid)
        public BookingSourceType BookingSource { get; set; } // NOT NULL, DEFAULT 0 (Website)
        public string? ManualBookingCustomerName { get; set; } // NULL, MaxLength(255)
        public string? ManualBookingCustomerPhone { get; set; } // NULL, MaxLength(20)
        public string? NotesFromBooker { get; set; } // NULL, NVARCHAR(MAX)
        public string? NotesFromOwner { get; set; } // NULL, NVARCHAR(MAX)
        public DateTime CreatedAt { get; set; } // NOT NULL, DEFAULT GETDATE()
        public DateTime? UpdatedAt { get; set; } // NULL

        // Navigation Properties
        public virtual ApplicationUser BookerUser { get; set; }
        public virtual CourtComplex CourtComplex { get; set; }
        public virtual Court Court { get; set; }
        public virtual ICollection<BookedSlot> BookedSlots { get; set; }

        public Booking()
        {
            BookedSlots = new HashSet<BookedSlot>();
        }
    }
}
