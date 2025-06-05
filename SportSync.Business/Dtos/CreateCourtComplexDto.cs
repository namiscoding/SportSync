using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Dtos
{
    public class CreateCourtComplexDto
    {
        // Các thuộc tính tương tự như CourtComplexViewModel, trừ IFormFile
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
        public decimal? Latitude { get; set; } 
        public decimal? Longitude { get; set; } 
        // Thay thế IFormFile bằng thông tin cần thiết cho việc upload
        public ImageInputDto? MainImage { get; set; }
    }
}
