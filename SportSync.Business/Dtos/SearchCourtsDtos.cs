using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Dtos
{
    public sealed class CourtSearchRequest
    {
        public int? SportTypeId { get; init; }

   
        public string? City { get; init; }
        public string? District { get; init; }

      
        public double? UserLat { get; init; }  
        public double? UserLng { get; init; }  
        public double? RadiusKm { get; init; }   

       
        public required DateOnly Date { get; init; }

      
        public TimeOnly? FromTime { get; init; }
        public TimeOnly? ToTime { get; init; }
    }

    public class HourlyPriceRateDto
    {
        public int HourlyPriceRateId { get; set; }
        public TimeOnly Start { get; set; }
        public TimeOnly End { get; set; }
        public decimal PricePerHour { get; set; }
    }

    public class CourtWithSlotsDto
    {
        public int CourtId { get; set; }
        public string Name { get; set; } = null!;
        public string SportTypeName { get; set; }
        public IEnumerable<HourlyPriceRateDto> HourlyPriceRates { get; set; }
                          = Enumerable.Empty<HourlyPriceRateDto>();
    }

    public sealed class CourtComplexResultDto
    {
        public int ComplexId { get; init; }
        public string Name { get; init; } = null!;
        public string Address { get; init; } = null!;
        public string SportTypeName { get; init; } = null!;
        public string? ThumbnailUrl { get; init; }
        public IEnumerable<AmenityDto> Amenities { get; init; }
                            = Enumerable.Empty<AmenityDto>();
        public IEnumerable<CourtWithSlotsDto> Courts { get; init; }
                            = Enumerable.Empty<CourtWithSlotsDto>();
    }

}
