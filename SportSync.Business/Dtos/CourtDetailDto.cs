using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Dtos
{
    public sealed class CourtDetailDto
    {
        public int CourtId { get; init; }
        public int ComplexId { get; init; }
        public string Name { get; init; } = null!;
        public string SportTypeName { get; init; } = null!;
        public string? ImageUrl { get; init; }
        public IEnumerable<AmenityDto> Amenities { get; init; } = [];
        public IEnumerable<TimeSlotDto> AvailableSlots { get; init; } = [];
    }

}
