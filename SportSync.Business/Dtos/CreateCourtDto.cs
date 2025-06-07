using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SportSync.Business.Dtos
{
    public class CreateCourtDto
    {
        public int CourtComplexId { get; set; }
        public string Name { get; set; }
        public int SportTypeId { get; set; }
        public string? Description { get; set; }
        public int DefaultSlotDurationMinutes { get; set; }
        public int AdvanceBookingDaysLimit { get; set; }
        public TimeOnly? OpeningTime { get; set; }
        public TimeOnly? ClosingTime { get; set; }
        public IFormFile? MainImageFile { get; set; } // Nhận IFormFile từ Controller
        public List<int>? SelectedAmenityIds { get; set; }
    }
}
