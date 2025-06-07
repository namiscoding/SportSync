using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Dtos
{
    public class WeeklyScheduleDto
    {
        public int CourtId { get; set; }
        public string CourtName { get; set; }
        public string CourtComplexName { get; set; }
        public int CourtComplexId { get; set; }

        // Ngày đầu tiên của tuần đang được hiển thị (luôn là Thứ Hai)
        public DateTime StartDateOfWeek { get; set; }

        // Danh sách tất cả các slot trong tuần (bao gồm cả trống, đã đặt, đã đóng)
        public List<ScheduleSlotDto> Slots { get; set; } = new();
    }
}
