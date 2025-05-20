using System.ComponentModel.DataAnnotations;

namespace SportSync.Web.Models.ViewModels.Account
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string PhoneNumber { get; set; } // Để xác định người dùng

        [Required(ErrorMessage = "Cần có token xác thực từ Firebase.")]
        public string FirebaseIdToken { get; set; } // ID Token từ Firebase sau khi OTP thành công

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
        [StringLength(100, ErrorMessage = "{0} phải có ít nhất {2} và tối đa {1} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        [Compare("Password", ErrorMessage = "Mật khẩu mới và xác nhận mật khẩu mới không khớp.")]
        public string ConfirmPassword { get; set; }
    }
}
