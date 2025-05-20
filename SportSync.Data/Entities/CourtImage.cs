using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Data.Entities
{
    public class CourtImage
    {
        public int CourtImageId { get; set; } // PK, Identity
        public int CourtId { get; set; } // FK references Courts(CourtId), NOT NULL, ON DELETE CASCADE
        public string CloudinaryPublicId { get; set; } // NOT NULL, MaxLength(255)
        public string CloudinaryUrl { get; set; } // NOT NULL, NVARCHAR(MAX)
        public string? Caption { get; set; } // NULL, MaxLength(255)
        public bool IsPrimary { get; set; } // NOT NULL, DEFAULT 0

        // Navigation Property
        public virtual Court Court { get; set; }

        public CourtImage()
        {
        }
    }
}
