using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SportSync.Data.Entities;
using SportSync.Web.Models.ViewModels.Profile;

namespace SportSync.Web.Controllers
{
    [Authorize] // Yêu cầu người dùng phải đăng nhập để truy cập controller này
    public class ProfileController : Controller // Đổi tên lớp thành ProfileController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<ProfileController> _logger; // Đổi ILogger<ManageController> thành ILogger<ProfileController>

        public ProfileController( // Đổi tên constructor
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ProfileController> logger) // Đổi ILogger<ManageController> thành ILogger<ProfileController>
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // GET: /Profile hoặc /Profile/Index (Trang quản lý thông tin cá nhân)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("ProfileController.Index (GET): User not found for User Principal {UserPrincipalName}.", User.Identity.Name);
                return Challenge();
            }

            var model = new ManageProfileViewModel // ViewModel vẫn có thể giữ tên ManageProfileViewModel hoặc đổi thành ProfileViewModel
            {
                PhoneNumber = user.PhoneNumber,
                FullName = user.UserProfile?.FullName
                // Email = user.Email 
            };

            if (TempData.ContainsKey("StatusMessage"))
            {
                model.StatusMessage = TempData["StatusMessage"].ToString();
            }

            // Đổi tên View từ "ManageProfile" thành "Index" để theo convention,
            // hoặc bạn có thể giữ tên View là "ManageProfile.cshtml" và trả về View("ManageProfile", model)
            // Để đơn giản, giả sử View cho action Index sẽ là Index.cshtml trong thư mục Views/Profile
            return View(model);
        }

        // POST: /Profile hoặc /Profile/Index (Cập nhật thông tin cá nhân)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ManageProfileViewModel model) // ViewModel vẫn có thể giữ tên ManageProfileViewModel
        {
            if (!ModelState.IsValid)
            {
                var userForPhone = await _userManager.GetUserAsync(User);
                model.PhoneNumber = userForPhone?.PhoneNumber; // Cần load lại SĐT vì nó là readonly trên form
                return View(model); // Trả về View Index.cshtml (trong Views/Profile)
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("ProfileController.Index (POST): User not found for User Principal {UserPrincipalName}.", User.Identity.Name);
                return Challenge();
            }

            bool profileChanged = false;

            if (user.UserProfile == null)
            {
                user.UserProfile = new UserProfile { UserId = user.Id };
                _logger.LogInformation("ProfileController.Index (POST): UserProfile was null for user {UserName}, initializing.", user.UserName);
            }

            if (user.UserProfile.FullName != model.FullName)
            {
                user.UserProfile.FullName = model.FullName;
                profileChanged = true;
                _logger.LogInformation("ProfileController.Index (POST): FullName updated for user {UserName} to {FullName}.", user.UserName, model.FullName);
            }

            // (Tùy chọn) Cập nhật Email
            // ...

            if (profileChanged)
            {
                var updateResult = await _userManager.UpdateAsync(user);
                if (updateResult.Succeeded)
                {
                    TempData["StatusMessage"] = "Thông tin cá nhân của bạn đã được cập nhật.";
                    _logger.LogInformation("ProfileController.Index (POST): Profile updated successfully for user {UserName}.", user.UserName);
                }
                else
                {
                    _logger.LogError("ProfileController.Index (POST): Error updating profile for user {UserName}. Errors: {Errors}", user.UserName, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    model.PhoneNumber = user.PhoneNumber;
                    return View(model); // Trả về View Index.cshtml (trong Views/Profile)
                }
            }
            else
            {
                TempData["StatusMessage"] = "Không có thay đổi nào được thực hiện.";
            }

            return RedirectToAction(nameof(Index));
        }


        // GET: /Profile/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            var model = new ChangePasswordViewModel();
            if (TempData.ContainsKey("StatusMessage"))
            {
                model.StatusMessage = TempData["StatusMessage"].ToString();
            }
            return View(model); // Trả về View ChangePassword.cshtml (trong Views/Profile)
        }

        // POST: /Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); // Trả về View ChangePassword.cshtml (trong Views/Profile)
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("ProfileController.ChangePassword (POST): User not found for User Principal {UserPrincipalName}.", User.Identity.Name);
                return Challenge();
            }

            if (!await _userManager.HasPasswordAsync(user))
            {
                _logger.LogInformation("ProfileController.ChangePassword (POST): User {UserName} does not have a password.", user.UserName);
                ModelState.AddModelError(string.Empty, "Tài khoản của bạn chưa có mật khẩu. Không thể sử dụng chức năng này.");
                return View(model); // Trả về View ChangePassword.cshtml (trong Views/Profile)
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                _logger.LogWarning("ProfileController.ChangePassword (POST): Failed to change password for user {UserName}. Errors: {Errors}", user.UserName, string.Join(", ", changePasswordResult.Errors.Select(e => e.Description)));
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model); // Trả về View ChangePassword.cshtml (trong Views/Profile)
            }

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("User {UserName} changed their password successfully.", user.UserName);
            TempData["StatusMessage"] = "Mật khẩu của bạn đã được thay đổi thành công.";

            return RedirectToAction(nameof(ChangePassword));
        }
    }
}
