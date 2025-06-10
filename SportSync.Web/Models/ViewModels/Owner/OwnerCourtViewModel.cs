using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SportSync.Web.Models.ViewModels.Owner
{
    public class OwnerCourtViewModel
    {
        public int CourtId { get; set; }

        [Required]
        public int CourtComplexId { get; set; }

        [Required(ErrorMessage = "Tên sân là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên sân không được vượt quá 100 ký tự.")]
        [Display(Name = "Tên Sân (ví dụ: Sân A, Sân 1)")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại hình thể thao.")]
        [Display(Name = "Loại Hình Thể Thao")]
        public int SportTypeId { get; set; }
        public IEnumerable<SelectListItem>? SportTypes { get; set; }

        [Display(Name = "Mô tả (Tùy chọn)")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Thời lượng mỗi slot là bắt buộc.")]
        [Range(30, 240, ErrorMessage = "Thời lượng slot phải từ 30 đến 240 phút.")]
        [Display(Name = "Thời lượng mỗi Slot (phút)")]
        public int DefaultSlotDurationMinutes { get; set; } = 60;

        [Display(Name = "Giới hạn đặt trước (ngày, tùy chọn)")]
        [Range(1, 365, ErrorMessage = "Giới hạn đặt trước phải từ 1 đến 365 ngày.")]
        public int AdvanceBookingDaysLimit { get; set; } = 7;

        [DataType(DataType.Time)]
        [Display(Name = "Giờ mở cửa sân (Tùy chọn - nếu khác khu phức hợp)")]
        public TimeOnly? OpeningTime { get; set; }

        [DataType(DataType.Time)]
        [Display(Name = "Giờ đóng cửa sân (Tùy chọn - nếu khác khu phức hợp)")]
        public TimeOnly? ClosingTime { get; set; }

        [Display(Name = "Ảnh đại diện sân (Tùy chọn)")]
        public IFormFile? MainImageFile { get; set; }
        public string? MainImageCloudinaryUrl { get; set; }

        [Display(Name = "Các tiện ích (Tùy chọn)")]
        public List<int>? SelectedAmenityIds { get; set; }
        public IEnumerable<SelectListItem>? AvailableAmenities { get; set; }

        public string? CourtComplexName { get; set; } // Thêm để hiển thị tên Khu phức hợp cha
        public OwnerCourtViewModel()
        {
            SelectedAmenityIds = new List<int>();
        }
    }
}
    
