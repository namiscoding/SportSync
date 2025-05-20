using System.ComponentModel.DataAnnotations;

namespace SportSync.Web.Models.ViewModels.CourtComplex
{
    public class CourtComplexViewModel
    {
        public int CourtComplexId { get; set; } // Cần cho Edit

        [Required(ErrorMessage = "Tên khu phức hợp sân là bắt buộc.")]
        [StringLength(255, ErrorMessage = "Tên không được vượt quá 255 ký tự.")]
        [Display(Name = "Tên Khu Phức Hợp Sân")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc.")]
        [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự.")]
        [Display(Name = "Địa chỉ chi tiết")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Thành phố là bắt buộc.")]
        [StringLength(100)]
        [Display(Name = "Thành phố/Tỉnh")]
        public string City { get; set; }

        [Required(ErrorMessage = "Quận/Huyện là bắt buộc.")]
        [StringLength(100)]
        [Display(Name = "Quận/Huyện")]
        public string District { get; set; }

        [StringLength(100)]
        [Display(Name = "Phường/Xã (Tùy chọn)")]
        public string? Ward { get; set; }

        [Display(Name = "Mô tả (Tùy chọn)")]
        public string? Description { get; set; }

        [Display(Name = "Ảnh đại diện chính (Tùy chọn)")]
        public IFormFile? MainImageFile { get; set; } // Để tải ảnh lên

        public string? MainImageCloudinaryUrl { get; set; } // Để hiển thị ảnh hiện tại khi Edit

        [RegularExpression(@"^\+?[0-9\s\-\(\)]+$", ErrorMessage = "Số điện thoại liên hệ không hợp lệ.")]
        [StringLength(20)]
        [Display(Name = "Số điện thoại liên hệ (Tùy chọn)")]
        public string? ContactPhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "Địa chỉ email liên hệ không hợp lệ.")]
        [StringLength(255)]
        [Display(Name = "Email liên hệ (Tùy chọn)")]
        public string? ContactEmail { get; set; }

        [DataType(DataType.Time)]
        [Display(Name = "Giờ mở cửa mặc định (Tùy chọn)")]
        public TimeOnly? DefaultOpeningTime { get; set; }

        [DataType(DataType.Time)]
        [Display(Name = "Giờ đóng cửa mặc định (Tùy chọn)")]
        public TimeOnly? DefaultClosingTime { get; set; }

        // Các trường này sẽ được tự động gán hoặc quản lý bởi Admin
        // public string OwnerUserId { get; set; }
        // public ApprovalStatus ApprovalStatus { get; set; }
        // public bool IsActiveByOwner { get; set; } = true; // Mặc định khi tạo mới
        // public bool IsActiveByAdmin { get; set; } = true; // Mặc định
    }
}
