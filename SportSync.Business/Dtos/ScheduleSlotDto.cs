using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Dtos
{
    public class ScheduleSlotDto
    {
        public DateTime Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } // "Available", "Booked", "Closed"

        // Thông tin nếu slot đã được đặt
        public long? BookingId { get; set; }
        public string CustomerName { get; set; } // Tên người đặt hoặc "Khách vãng lai"
        public string CustomerPhone { get; set; }
    }
}
