using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SportSync.Web.Models.ViewModels.Profile
{
    public class ManageProfileViewModel
    {
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } // Read-only, lấy từ thông tin người dùng

        [Required(ErrorMessage = "Họ và Tên là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Họ và Tên không được vượt quá 100 ký tự.")]
        [Display(Name = "Họ và Tên")]
        public string FullName { get; set; }

        // (Tùy chọn) Nếu bạn muốn cho phép người dùng thêm/cập nhật email
        // [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        // [Display(Name = "Địa chỉ Email")]
        // public string Email { get; set; }

        [TempData] // Để hiển thị thông báo sau khi cập nhật
        public string StatusMessage { get; set; }
    }
}
