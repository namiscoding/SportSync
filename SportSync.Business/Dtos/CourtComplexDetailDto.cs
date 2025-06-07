using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Dtos
{

    public sealed record AmenityDto(string Name);

 



    public sealed class CourtComplexDetailDto
    {
        public int ComplexId { get; init; }
        public string Name { get; init; } = null!;
        public string Address { get; init; } = null!;
        public string? Description { get; init; }
        public string? ThumbnailUrl { get; init; }
        public decimal? Latitude { get; init; }
        public decimal? Longitude { get; init; }
        public string? ContactPhone { get; init; }
        public string? ContactEmail { get; init; }

        public IEnumerable<CourtDetailDto> Courts { get; init; } = [];
    }
}
