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
        public long BookingId { get; set; } // PK, IDENTITY(1,1) handled in DbContext

        public string BookerUserId { get; set; } = default!; // FK to ApplicationUser
        public ApplicationUser BookerUser { get; set; } = default!;

        public int CourtComplexId { get; set; } = default!; // FK to CourtComplex
        public CourtComplex CourtComplex { get; set; } = default!;

        public int CourtId { get; set; } = default!; // FK to Court
        public Court Court { get; set; } = default!;

        public DateTime BookedStartTime { get; set; } // Required
        public DateTime BookedEndTime { get; set; } // Required

        public decimal TotalPrice { get; set; } // Required, ColumnType handled in DbContext

        public BookingStatusType BookingStatus { get; set; } // Default value handled in DbContext

        public string? ManualBookingCustomerName { get; set; }

        public string? ManualBookingCustomerPhone { get; set; }

        public string? NotesFromBooker { get; set; }
        public string? NotesFromOwner { get; set; }

        public DateTime CreatedAt { get; set; } // Default value handled in DbContext
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public ICollection<BookingProduct>? BookingProducts { get; set; }

        public Booking()
        {
            BookingProducts = new HashSet<BookingProduct>();
        }
    }
}
