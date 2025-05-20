using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Data.Entities
{
    public class BookedSlot
    {
        public long BookedSlotId { get; set; } // PK (BIGINT), Identity
        public long BookingId { get; set; } // FK references Bookings(BookingId), NOT NULL, ON DELETE CASCADE
        public int TimeSlotId { get; set; } // FK references TimeSlots(TimeSlotId), NOT NULL (Cân nhắc ON DELETE behavior)
        public DateOnly SlotDate { get; set; } // NOT NULL (Ngày cụ thể của slot này)
        public TimeOnly ActualStartTime { get; set; } // NOT NULL
        public TimeOnly ActualEndTime { get; set; } // NOT NULL
        public decimal PriceAtBookingTime { get; set; } // NOT NULL, DECIMAL(18,2)
                                                        // UNIQUE (TimeSlotId, SlotDate, ActualStartTime)

        // Navigation Properties
        public virtual Booking Booking { get; set; }
        public virtual TimeSlot TimeSlot { get; set; }
    }
}
