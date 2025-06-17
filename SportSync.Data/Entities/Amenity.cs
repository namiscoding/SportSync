using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Data.Entities
{
    public class Amenity
    {
       public int AmenityId { get; set; } // PK, IDENTITY(1,1) handled in DbContext

        public string Name { get; set; } = default!; // Required, StringLength, Unique handled in DbContext

        public string? Description { get; set; }

        public string? IconCssClass { get; set; }

        public bool IsActive { get; set; } // Default value handled in DbContext

        // Navigation property (Nhiều-nhiều với CourtComplexes qua bảng nối)
        public ICollection<CourtComplexAmenity>? CourtComplexAmenities { get; set; }

        public Amenity()
        {
            CourtComplexAmenities = new HashSet<CourtComplexAmenity>();
        }
    }
}
