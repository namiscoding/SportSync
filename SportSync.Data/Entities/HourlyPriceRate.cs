using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Data.Entities
{
    public class HourlyPriceRate
    {
        public int HourlyPriceRateId { get; set; } 

        public int CourtId { get; set; } = default!; 
        public Court Court { get; set; } = default!;

        public DayOfWeek? DayOfWeek { get; set; }

        public TimeSpan StartTime { get; set; } 

        public TimeSpan EndTime { get; set; } 

        public decimal PricePerHour { get; set; }

        public DateTime CreatedAt { get; set; } 
        public DateTime? UpdatedAt { get; set; }
    }
}
