using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Data.Entities
{
    public class TimeSlot
    {
        public int TimeSlotId { get; set; } // PK, Identity
        public int CourtId { get; set; } // FK references Courts(CourtId), NOT NULL, ON DELETE CASCADE
        public TimeOnly StartTime { get; set; } // NOT NULL
        public TimeOnly EndTime { get; set; } // NOT NULL
        public decimal Price { get; set; } // NOT NULL, DECIMAL(18,2)
        public DayOfWeek? DayOfWeek { get; set; } // NULL (System.DayOfWeek: Sunday=0,...Saturday=6)
                                                  // UNIQUE (CourtId, StartTime, DayOfWeek)
        public bool IsActiveByOwner { get; set; } // NOT NULL, DEFAULT 1
        public string? Notes { get; set; } // NULL, MaxLength(255)

        // Navigation Properties
        public virtual Court Court { get; set; }
        public virtual ICollection<BookedSlot> BookedSlots { get; set; }

        public TimeSlot()
        {
            BookedSlots = new HashSet<BookedSlot>();
        }
    }
}
