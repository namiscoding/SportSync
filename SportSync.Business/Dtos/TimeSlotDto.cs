using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Dtos
{
    public sealed class TimeSlotDto
    {
        public int TimeSlotId { get; init; }
        public TimeOnly Start { get; init; }
        public TimeOnly End { get; init; }
        public decimal Price { get; init; }
        public decimal PriceLowest { get; init; }
    }
}
