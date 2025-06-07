using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Dtos
{
    public class TimeSlotInfo
    {
        public string SlotId { get; set; } // ID duy nhất ở client, ví dụ: "slot-1-08:00" (Thứ Hai, 8h)
        public int DayOfWeek { get; set; } // 0=Chủ Nhật, 1=Thứ Hai, ..., 6=Thứ Bảy
        public TimeOnly StartTime { get; set; }
        public decimal Price { get; set; }
        public bool IsClosed { get; set; }
    }
}
