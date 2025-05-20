using System.ComponentModel.DataAnnotations;

namespace SportSync.Web.Models.ViewModels.Account
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số điện thoại đã đăng ký")]
        public string PhoneNumber { get; set; }
    }
}
