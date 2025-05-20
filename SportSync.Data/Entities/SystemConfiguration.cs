using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Data.Entities
{
    public class SystemConfiguration
    {
        public string ConfigurationKey { get; set; } // PK, MaxLength(100)
        public string ConfigurationValue { get; set; } // NOT NULL, NVARCHAR(MAX)
        public string? Description { get; set; } // NULL, MaxLength(500)
        public DateTime LastUpdatedAt { get; set; } // NOT NULL, DEFAULT GETDATE()
        public string? UpdatedByAdminId { get; set; } // FK references AspNetUsers(Id), NULL

        // Navigation Property
        public virtual ApplicationUser? UpdatedByAdmin { get; set; }
    }
}
