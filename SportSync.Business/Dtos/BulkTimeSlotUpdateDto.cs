using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Dtos
{
    public class BulkTimeSlotUpdateDto
    {
        public int CourtId { get; set; }
        public Dictionary<string, TimeSlotUpdateInfo> Changes { get; set; }
    }
}
