using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Data.Entities
{
    public class SubscriptionPlan
    {
        public int PlanId { get; set; } 

        public string Name { get; set; } = default!; 

        public string? Description { get; set; }

        public decimal MonthlyPrice { get; set; } 

        public string? Features { get; set; }

        public bool IsActive { get; set; } 

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public ICollection<OwnerSubscription>? OwnerSubscriptions { get; set; }
    }
}
