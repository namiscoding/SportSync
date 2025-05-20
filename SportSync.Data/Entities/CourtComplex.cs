using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportSync.Data.Enums;

namespace SportSync.Data.Entities
{
    public class CourtComplex
    {
        public int CourtComplexId { get; set; } // PK, Identity
        public string OwnerUserId { get; set; } // FK references AspNetUsers(Id), NOT NULL
        public string Name { get; set; } // NOT NULL, MaxLength(255)
        public string Address { get; set; } // NOT NULL, MaxLength(500)
        public string City { get; set; } // NOT NULL, MaxLength(100)
        public string District { get; set; } // NOT NULL, MaxLength(100)
        public string? Ward { get; set; } // NULL, MaxLength(100)
        public decimal? Latitude { get; set; } // NULL, DECIMAL(9,6)
        public decimal? Longitude { get; set; } // NULL, DECIMAL(9,6)
        public string? Description { get; set; } // NULL, NVARCHAR(MAX)
        public string? MainImageCloudinaryPublicId { get; set; } // NULL, MaxLength(255)
        public string? MainImageCloudinaryUrl { get; set; } // NULL, NVARCHAR(MAX)
        public string? ContactPhoneNumber { get; set; } // NULL, MaxLength(20)
        public string? ContactEmail { get; set; } // NULL, MaxLength(255)
        public TimeOnly? DefaultOpeningTime { get; set; } // NULL, TIME
        public TimeOnly? DefaultClosingTime { get; set; } // NULL, TIME
        public ApprovalStatus ApprovalStatus { get; set; } // NOT NULL, DEFAULT 0 (PendingApproval)
        public bool IsActiveByOwner { get; set; } // NOT NULL, DEFAULT 1
        public bool IsActiveByAdmin { get; set; } // NOT NULL, DEFAULT 1
        public DateTime CreatedAt { get; set; } // NOT NULL, DEFAULT GETDATE()
        public DateTime? UpdatedAt { get; set; } // NULL
        public string? ApprovedByAdminId { get; set; } // FK references AspNetUsers(Id), NULL
        public DateTime? ApprovedAt { get; set; } // NULL
        public string? RejectionReason { get; set; } // NULL, MaxLength(500)

        // Navigation Properties
        public virtual ApplicationUser OwnerUser { get; set; }
        public virtual ApplicationUser? ApprovedByAdmin { get; set; }
        public virtual ICollection<Court> Courts { get; set; }
        public virtual ICollection<CourtComplexImage> CourtComplexImages { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; } // Bookings thuộc về CourtComplex nào

        public CourtComplex()
        {
            Courts = new HashSet<Court>();
            CourtComplexImages = new HashSet<CourtComplexImage>();
            Bookings = new HashSet<Booking>();
            // Các giá trị mặc định như ApprovalStatus, IsActiveByOwner, IsActiveByAdmin, CreatedAt
            // sẽ được cấu hình bằng Fluent API.
        }
    }
}
