using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportSync.Data.Enums;

namespace SportSync.Data.Entities
{
    public class Notification
    {
        public long NotificationId { get; set; } // PK (BIGINT), Identity
        public string RecipientUserId { get; set; } // FK references AspNetUsers(Id), NOT NULL
        public string Title { get; set; } // NOT NULL, MaxLength(255)
        public string Message { get; set; } // NOT NULL, NVARCHAR(MAX)
        public NotificationContentType NotificationType { get; set; } // NOT NULL (Sử dụng enum vừa tạo)
        public string? ReferenceId { get; set; } // NULL, MaxLength(100) (ví dụ: BookingId, CourtComplexId)
        public string? ReferenceType { get; set; } // NULL, MaxLength(50) (ví dụ: "Booking", "CourtComplex")
        public bool IsRead { get; set; } // NOT NULL, DEFAULT 0
        public DateTime? ReadAt { get; set; } // NULL
        public DeliveryMethodType DeliveryMethod { get; set; } // NOT NULL, DEFAULT 0 (InApp)
        public DateTime CreatedAt { get; set; } // NOT NULL, DEFAULT GETDATE()

        // Navigation Property
        public virtual ApplicationUser RecipientUser { get; set; }
    }
}
