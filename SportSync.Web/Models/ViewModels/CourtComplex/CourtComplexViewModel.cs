using System.ComponentModel.DataAnnotations;

namespace SportSync.Web.Models.ViewModels.CourtComplex
{
    public class CourtComplexViewModel
    {
        public int CourtComplexId { get; set; }

        [Required(ErrorMessage = "Tên khu phức hợp sân là bắt buộc.")]
        [StringLength(255, ErrorMessage = "Tên không được vượt quá 255 ký tự.")]
        [Display(Name = "Tên Khu Phức Hợp Sân")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc.")]
        [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự.")]
        [Display(Name = "Địa chỉ chi tiết (Số nhà, tên đường)")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Tỉnh/Thành phố.")]
        [Display(Name = "Tỉnh/Thành phố")]
        public string City { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Quận/Huyện.")]
        [Display(Name = "Quận/Huyện")]
        public string District { get; set; }

        // Đã sửa thành bắt buộc trong lần cập nhật View trước
        [Required(ErrorMessage = "Vui lòng chọn Phường/Xã.")]
        [Display(Name = "Phường/Xã")]
        public string? Ward { get; set; }

        [Display(Name = "Mô tả (Tùy chọn)")]
        public string? Description { get; set; }

        [Display(Name = "Ảnh đại diện chính (Tùy chọn)")]
        public IFormFile? MainImageFile { get; set; }

        public string? MainImageCloudinaryUrl { get; set; }

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

        // **THÊM CÁC TRƯỜNG TỌA ĐỘ**
        [Display(Name = "Vĩ độ (Latitude)")]
        // Bạn có thể thêm Range validation nếu muốn, ví dụ:
        // [Range(-90.0, 90.0, ErrorMessage = "Vĩ độ không hợp lệ.")]
        public decimal? Latitude { get; set; }

        [Display(Name = "Kinh độ (Longitude)")]
        // [Range(-180.0, 180.0, ErrorMessage = "Kinh độ không hợp lệ.")]
        public decimal? Longitude { get; set; }
    }
}
