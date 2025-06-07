using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Dtos.OwnerDashboard
{
    public class PendingBookingDto
    {
        public long BookingId { get; set; }
        public string CustomerName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
