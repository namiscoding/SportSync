using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Dtos
{

    public sealed record AmenityDto(string Name);
    public class CourtComplexDetailDto
    {
        public int ComplexId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string? Description { get; set; }  // Mô tả khu phức hợp
        public string ContactPhoneNumber { get; set; }  // Số điện thoại liên hệ
        public string ContactEmail { get; set; }  // Email liên hệ
        public string SportTypeName { get; set; }

        public string? MainImageUrl { get; set; }
        public string? GoogleMapsLink { get; set; }
        public List<AmenityDto> Amenities { get; set; } = new List<AmenityDto>();  // Tiện nghi
        public List<CourtWithSlotsDto> Courts { get; set; } = new List<CourtWithSlotsDto>();  // Danh sách sân
    }
}
