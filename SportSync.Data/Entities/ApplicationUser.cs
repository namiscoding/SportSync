using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace SportSync.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public virtual UserProfile UserProfile { get; set; }

        // Navigation properties cho các mối quan hệ 1-nhiều
        public CourtComplex? OwnedCourtComplex { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<BlockedCourtSlot> CreatedBlockedSlots { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public ICollection<OwnerSubscription>? OwnerSubscriptions { get; set; }
        public virtual ICollection<SystemConfiguration> UpdatedSystemConfigurationsByAdmin { get; set; } // Đổi tên để rõ hơn


        public ApplicationUser()
        {
            Bookings = new HashSet<Booking>();
            CreatedBlockedSlots = new HashSet<BlockedCourtSlot>();
            Notifications = new HashSet<Notification>();
            OwnerSubscriptions = new HashSet<OwnerSubscription>();
            UpdatedSystemConfigurationsByAdmin = new HashSet<SystemConfiguration>();
        }
    }
}
