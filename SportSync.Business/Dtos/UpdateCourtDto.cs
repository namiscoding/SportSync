using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SportSync.Business.Dtos
{
    public class UpdateCourtDto
    {
        public int CourtId { get; set; } // ID của sân cần cập nhật
        public int CourtComplexId { get; set; } // Để kiểm tra ngữ cảnh và quay lại
        public string Name { get; set; }
        public int SportTypeId { get; set; }
        public string? Description { get; set; }
        public int DefaultSlotDurationMinutes { get; set; }
        public int AdvanceBookingDaysLimit { get; set; }
        public TimeOnly? OpeningTime { get; set; }
        public TimeOnly? ClosingTime { get; set; }
        public IFormFile? NewMainImageFile { get; set; } // Ảnh mới (nếu có)
        public List<int>? SelectedAmenityIds { get; set; }
    }
}
