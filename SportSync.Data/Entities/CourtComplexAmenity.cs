using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Data.Entities
{
    public class CourtComplexAmenity
    {
        public int CourtComplexId { get; set; } 
        public CourtComplex CourtComplex { get; set; } = default!;

        public int AmenityId { get; set; } 
        public Amenity Amenity { get; set; } = default!;
    }
}
