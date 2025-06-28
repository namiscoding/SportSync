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

    public class CourtWithSlotsDto
    {
        public int CourtId { get; set; }
        public string Name { get; set; }
        public string SportTypeName { get; set; }
        public string StatusByOwner { get; set; }  // Trạng thái của sân
        public bool IsActiveByAdmin { get; set; }  // Trạng thái bởi admin

        public List<HourlyPriceRateDto> HourlyPriceRates { get; set; } = new List<HourlyPriceRateDto>();  // Khung giờ
    }

    public class HourlyPriceRateDto
    {
        public int HourlyPriceRateId { get; set; }
        public TimeOnly Start { get; set; }
        public TimeOnly End { get; set; }
        public decimal PricePerHour { get; set; }
    }

    public sealed class CourtComplexResultDto
    {
        public int ComplexId { get; init; }
        public string Name { get; init; } = null!;
        public string Address { get; init; } = null!;
        public string? ThumbnailUrl { get; init; }
        public double? DistanceKm { get; init; }
        public string? GoogleMapsLink { get; init; }
        public string SportTypeName { get; init; } = null!;
        public IEnumerable<CourtWithSlotsDto> Courts { get; init; } = Enumerable.Empty<CourtWithSlotsDto>();
    }

}
