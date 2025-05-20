using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Data.Entities
{
    public class CourtAmenity
    {
        public int CourtId { get; set; } // Composite PK, FK references Courts(CourtId), ON DELETE CASCADE
        public int AmenityId { get; set; } // Composite PK, FK references Amenities(AmenityId), ON DELETE CASCADE

        // Navigation Properties
        public virtual Court Court { get; set; }
        public virtual Amenity Amenity { get; set; }
    }
}
