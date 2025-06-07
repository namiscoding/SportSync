using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Dtos
{
    public class TimeSlotOutputDto
    {
        public int TimeSlotId { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public decimal Price { get; set; }
        public string DayOfWeekText { get; set; } // Hiển thị "Mọi ngày", "Thứ Hai", "Chủ Nhật", v.v.
        public string? Notes { get; set; }
        public bool IsActiveByOwner { get; set; }
        public int CourtId { get; set; } // Để dùng cho các hành động trên trang
    }
}
