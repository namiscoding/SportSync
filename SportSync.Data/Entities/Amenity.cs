using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Data.Entities
{
    public class Amenity
    {
        public int AmenityId { get; set; } // Sẽ là PK, Identity bằng Fluent API
        public string Name { get; set; } // Sẽ là UNIQUE, NOT NULL, MaxLength(150) bằng Fluent API
        public string? Description { get; set; } // Sẽ là MaxLength(500), NULL bằng Fluent API
        public string? IconCssClass { get; set; } // Sẽ là MaxLength(100), NULL bằng Fluent API
        public bool IsActive { get; set; } // Sẽ là NOT NULL, Default(1) bằng Fluent API

        // Navigation property cho mối quan hệ nhiều-nhiều với Court thông qua bảng CourtAmenities
        public virtual ICollection<CourtAmenity> CourtAmenities { get; set; }

        public Amenity()
        {
            CourtAmenities = new HashSet<CourtAmenity>();
            // Tương tự SportType, IsActive = true; có thể bỏ nếu cấu hình default bằng Fluent API
            // IsActive = true;
        }
    }
}
