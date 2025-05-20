using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportSync.Data.Enums;

namespace SportSync.Data.Entities
{
    public class Court
    {
        public int CourtId { get; set; } // PK, Identity
        public int CourtComplexId { get; set; } // FK references CourtComplexes(CourtComplexId), NOT NULL, ON DELETE CASCADE
        public int SportTypeId { get; set; } // FK references SportTypes(SportTypeId), NOT NULL
        public string Name { get; set; } // NOT NULL, MaxLength(100)
        public string? Description { get; set; } // NULL, NVARCHAR(MAX)
        public int DefaultSlotDurationMinutes { get; set; } // NOT NULL
        public int AdvanceBookingDaysLimit { get; set; } // NOT NULL, DEFAULT 7
        public TimeOnly? OpeningTime { get; set; } // NULL, TIME
        public TimeOnly? ClosingTime { get; set; } // NULL, TIME
        public CourtStatusByOwner StatusByOwner { get; set; } // NOT NULL, DEFAULT 0 (Available)
        public bool IsActiveByAdmin { get; set; } // NOT NULL, DEFAULT 1
        public string? MainImageCloudinaryPublicId { get; set; } // NULL, MaxLength(255)
        public string? MainImageCloudinaryUrl { get; set; } // NULL, NVARCHAR(MAX)
        public DateTime CreatedAt { get; set; } // NOT NULL, DEFAULT GETDATE()
        public DateTime? UpdatedAt { get; set; } // NULL

        // Navigation Properties
        public virtual CourtComplex CourtComplex { get; set; }
        public virtual SportType SportType { get; set; }
        public virtual ICollection<CourtImage> CourtImages { get; set; }
        public virtual ICollection<CourtAmenity> CourtAmenities { get; set; } // Liên kết nhiều-nhiều với Amenity
        public virtual ICollection<TimeSlot> TimeSlots { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; } // Bookings thuộc về Court nào
        public virtual ICollection<BlockedCourtSlot> BlockedCourtSlots { get; set; }

        public Court()
        {
            CourtImages = new HashSet<CourtImage>();
            CourtAmenities = new HashSet<CourtAmenity>();
            TimeSlots = new HashSet<TimeSlot>();
            Bookings = new HashSet<Booking>();
            BlockedCourtSlots = new HashSet<BlockedCourtSlot>();
            // Các giá trị mặc định sẽ được cấu hình bằng Fluent API.
        }
    }
}
