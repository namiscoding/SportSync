using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Dtos
{
    public class TimeSlotUpdateInfo
    {
        public decimal? NewPrice { get; set; }
        public string NewStatus { get; set; } // "Available" hoặc "Closed"
        public int DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
    }
}
