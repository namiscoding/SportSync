using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportSync.Data.Enums;

namespace SportSync.Business.Dtos
{
    public class CourtOutputDto
    {
        public int CourtId { get; set; }
        public string Name { get; set; }
        public string SportTypeName { get; set; }
        public string? Description { get; set; }
        public int DefaultSlotDurationMinutes { get; set; }
        public string? MainImageCloudinaryUrl { get; set; }
        public List<string> AmenityNames { get; set; }
        public int CourtComplexId { get; set; } // Để tạo link quay lại
        public CourtStatusByOwner StatusByOwner { get; set; }
    }
}
