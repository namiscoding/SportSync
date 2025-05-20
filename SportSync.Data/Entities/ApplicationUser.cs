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
        public virtual ICollection<CourtComplex> OwnedCourtComplexes { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<BlockedCourtSlot> CreatedBlockedSlots { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<CourtComplex> ApprovedCourtComplexesByAdmin { get; set; } // Đổi tên để rõ hơn
        public virtual ICollection<SystemConfiguration> UpdatedSystemConfigurationsByAdmin { get; set; } // Đổi tên để rõ hơn


        public ApplicationUser()
        {
            OwnedCourtComplexes = new HashSet<CourtComplex>();
            Bookings = new HashSet<Booking>();
            CreatedBlockedSlots = new HashSet<BlockedCourtSlot>();
            Notifications = new HashSet<Notification>();
            ApprovedCourtComplexesByAdmin = new HashSet<CourtComplex>();
            UpdatedSystemConfigurationsByAdmin = new HashSet<SystemConfiguration>();
        }
    }
}
