using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Dtos
{
    public class CourtComplexBasicInfoDto // DTO này có thể dùng để trả về thông tin cơ bản
    {
        public int CourtComplexId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string? MainImageCloudinaryUrl { get; set; }
        public string ApprovalStatus { get; set; } // Hiển thị dạng string
        public bool IsActiveByOwner { get; set; }
        public bool IsActiveByAdmin { get; set; }
    }
}
