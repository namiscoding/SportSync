using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Dtos
{
    public class BookingSlotInfo
    {
        [Required]
        public DateTime SlotDate { get; set; }
        [Required]
        public TimeOnly StartTime { get; set; }
    }
}
