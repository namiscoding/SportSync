using System;
using System.Collections.Generic;

namespace SportSync.Data.Entities
{
    public class OwnerSubscription
    {
        public long OwnerSubscriptionId { get; set; } // PK, BIGINT, Identity
        public string OwnerUserId { get; set; } // FK, NOT NULL, ON DELETE RESTRICT
        public int PlanId { get; set; } // FK, NOT NULL, ON DELETE RESTRICT
        public DateTime StartDate { get; set; } // NOT NULL
        public DateTime? EndDate { get; set; } // NULL
        public bool IsActive { get; set; } // NOT NULL, DEFAULT 1
        public int PaymentStatus { get; set; } // NOT NULL
        public DateTime? LastPaymentDate { get; set; } // NULL
        public DateTime? NextBillingDate { get; set; } // NULL
        public DateTime CreatedAt { get; set; } // NOT NULL, DEFAULT GETDATE()
        public DateTime? UpdatedAt { get; set; } // NULL

        // Navigation Properties
        public virtual ApplicationUser OwnerUser { get; set; }
        public virtual SubscriptionPlan Plan { get; set; }
    }
}