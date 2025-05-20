using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Data.Entities
{
    public class CourtComplexImage
    {
        public int CourtComplexImageId { get; set; } // PK, Identity
        public int CourtComplexId { get; set; } // FK references CourtComplexes(CourtComplexId), NOT NULL, ON DELETE CASCADE
        public string CloudinaryPublicId { get; set; } // NOT NULL, MaxLength(255)
        public string CloudinaryUrl { get; set; } // NOT NULL, NVARCHAR(MAX)
        public string? Caption { get; set; } // NULL, MaxLength(255)
        public bool IsPrimary { get; set; } // NOT NULL, DEFAULT 0

        // Navigation Property
        public virtual CourtComplex CourtComplex { get; set; }

        public CourtComplexImage()
        {
            // IsPrimary sẽ được cấu hình default bằng Fluent API
        }
    }
}
