using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportSync.Data.Enums;

namespace SportSync.Data.Entities
{
    public class OwnerSubscription
    {
        public long OwnerSubscriptionId { get; set; } // PK, IDENTITY(1,1) handled in DbContext

        public string OwnerUserId { get; set; } = default!; // FK to ApplicationUser
        public ApplicationUser OwnerUser { get; set; } = default!;

        public int PlanId { get; set; } // FK to SubscriptionPlan
        public SubscriptionPlan Plan { get; set; } = default!;

        public DateTime StartDate { get; set; } // Required

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } // Default value handled in DbContext

        public PaymentStatusType PaymentStatus { get; set; } // Enum

        public DateTime? LastPaymentDate { get; set; }
        public DateTime? NextBillingDate { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
