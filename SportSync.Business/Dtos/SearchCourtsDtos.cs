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

    public sealed class CourtWithSlotsDto
    {
        public int CourtId { get; init; }
        public string Name { get; init; } = null!;
        public string SportTypeName { get; init; } = null!;
        public string? ImageUrl { get; init; }


        public IEnumerable<string> Amenities { get; init; } = Enumerable.Empty<string>();

 
        public decimal MinPrice => AvailableSlots.Any()
                                    ? AvailableSlots.Min(s => s.Price)
                                    : 0;

        public IEnumerable<TimeSlotDto> AvailableSlots { get; init; } = [];
    }

    public sealed class CourtComplexResultDto
    {
        public int ComplexId { get; init; }
        public string Name { get; init; } = null!;
        public string Address { get; init; } = null!;
        public string? ThumbnailUrl { get; init; }


        public double? DistanceKm { get; init; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; } 

    
        public IEnumerable<CourtWithSlotsDto> Courts { get; init; } = [];
    }

}
