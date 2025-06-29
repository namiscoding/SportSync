using System;
using System.Collections.Generic;

namespace SportSync.Data.Entities
{
    public class SubscriptionPlan
    {
        public int PlanId { get; set; } // PK, Identity
        public string Name { get; set; } // UNIQUE, NOT NULL, MaxLength(100)
        public string? Description { get; set; } // NULL, MaxLength(500)
        public decimal MonthlyPrice { get; set; } // NOT NULL, DECIMAL(18,2)
        public string? Features { get; set; } // NULL, NVARCHAR(MAX)
        public bool IsActive { get; set; } // NOT NULL, DEFAULT 1
        public DateTime CreatedAt { get; set; } // NOT NULL, DEFAULT GETDATE()
        public DateTime? UpdatedAt { get; set; } // NULL

        // Navigation Property
        public virtual ICollection<OwnerSubscription> OwnerSubscriptions { get; set; }

        public SubscriptionPlan()
        {
            OwnerSubscriptions = new HashSet<OwnerSubscription>();
        }
    }
}