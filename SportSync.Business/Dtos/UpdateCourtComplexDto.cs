using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SportSync.Business.Dtos
{
    public class UpdateCourtComplexDto
    {
        public int Id { get; set; } // ID của khu phức hợp cần cập nhật
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string? Ward { get; set; }
        public string? Description { get; set; }
        public string? ContactPhoneNumber { get; set; }
        public string? ContactEmail { get; set; }
        public TimeOnly? DefaultOpeningTime { get; set; }
        public TimeOnly? DefaultClosingTime { get; set; }
        public IFormFile? NewMainImageFile { get; set; } // Ảnh mới (nếu có)
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
}
