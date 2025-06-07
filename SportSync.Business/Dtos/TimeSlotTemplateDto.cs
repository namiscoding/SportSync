using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Dtos
{
    public class TimeSlotTemplateDto
    {
        public int CourtId { get; set; }
        public string CourtName { get; set; }
        public int CourtComplexId { get; set; }
        public string CourtComplexName { get; set; }
        public TimeOnly OpeningTime { get; set; }
        public TimeOnly ClosingTime { get; set; }
        public int SlotDurationMinutes { get; set; }
        public List<TimeSlotInfo> TimeSlots { get; set; } = new();
    }
}
