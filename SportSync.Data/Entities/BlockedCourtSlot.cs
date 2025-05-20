using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Data.Entities
{
    public class BlockedCourtSlot
    {
        public int BlockedSlotId { get; set; } // PK, Identity
        public int CourtId { get; set; } // FK references Courts(CourtId), NOT NULL, ON DELETE CASCADE
        public DateOnly BlockDate { get; set; } // NOT NULL
        public TimeOnly StartTime { get; set; } // NOT NULL
        public TimeOnly EndTime { get; set; } // NOT NULL
        public string? Reason { get; set; } // NULL, MaxLength(500)
        public string CreatedByUserId { get; set; } // FK references AspNetUsers(Id), NOT NULL
        public DateTime CreatedAt { get; set; } // NOT NULL, DEFAULT GETDATE()
                                                // UNIQUE (CourtId, BlockDate, StartTime)

        // Navigation Properties
        public virtual Court Court { get; set; }
        public virtual ApplicationUser CreatedByUser { get; set; }
    }
}
