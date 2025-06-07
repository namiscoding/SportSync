using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SportSync.Business.Interfaces;
using SportSync.Data.Entities;
using SportSync.Data.Enums;
using SportSync.Web.Models.ViewModels.Account;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SportSync.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        // ISmsSender không còn cần thiết cho các luồng đã sửa
        // private readonly ISmsSender _smsSender;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger
            /*, ISmsSender smsSender */ ) // Xóa ISmsSender nếu không dùng ở bất cứ đâu khác
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            // _smsSender = smsSender;
        }

        private string NormalizePhoneNumberToE164(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return null;
            string cleanedNumber = Regex.Replace(phoneNumber, @"\D", "");
            if (cleanedNumber.StartsWith("0"))
            {
                if (cleanedNumber.Length >= 10 && cleanedNumber.Length <= 11) return "+84" + cleanedNumber.Substring(1);
            }
            else if (cleanedNumber.StartsWith("84"))
            {
                if (cleanedNumber.Length >= 11 && cleanedNumber.Length <= 12) return "+" + cleanedNumber;
            }
            else if (phoneNumber.StartsWith("+84"))
            {
                if (cleanedNumber.Length >= 11 && cleanedNumber.Length <= 12) return "+" + cleanedNumber;
            }
            _logger.LogWarning("NormalizePhoneNumberToE164: Could not normalize phone number '{OriginalPhoneNumber}'.", phoneNumber);
            return null;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("Account/CheckPhoneNumberExistence")]
        public async Task<IActionResult> CheckPhoneNumberExistence(string phoneNumber)
        {
            string normalizedPhoneNumber = NormalizePhoneNumberToE164(phoneNumber);
            if (string.IsNullOrEmpty(normalizedPhoneNumber))
            {
                return Ok(new { exists = false, message = "Số điện thoại không hợp lệ." });
            }
            var user = await _userManager.FindByNameAsync(normalizedPhoneNumber);
            if (user != null)
            {
                bool hasPassword = !string.IsNullOrEmpty(user.PasswordHash);
                _logger.LogInformation("CheckPhoneNumberExistence: Phone number {PhoneNumber} exists. HasPassword: {HasPassword}", normalizedPhoneNumber, hasPassword);
                if (hasPassword) return Ok(new { exists = true, message = "Số điện thoại này đã được đăng ký và có mật khẩu." });
                return Ok(new { exists = true, message = "Số điện thoại này đã được sử dụng." });
            }
            _logger.LogInformation("CheckPhoneNumberExistence: Phone number {PhoneNumber} does not exist.", normalizedPhoneNumber);
            return Ok(new { exists = false });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (_signInManager.IsSignedIn(User)) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Xóa các Action SendRegistrationOtp và VerifyRegistrationOtp nếu không dùng nữa.
        // [HttpPost]
        // [AllowAnonymous]
        // [Route("Account/SendRegistrationOtp")]
        // public async Task<IActionResult> SendRegistrationOtp([FromBody] ForgotPasswordViewModel model) { ... }

        // [HttpPost]
        // [AllowAnonymous]
        // [Route("Account/VerifyRegistrationOtp")]
        // public IActionResult VerifyRegistrationOtp([FromBody] OtpVerificationRequest model) { ... }


        [HttpPost]
        [AllowAnonymous]
        [Route("Account/Register")]
        public async Task<IActionResult> Register([FromBody] CompleteRegistrationViewModel model)
        {
            string normalizedModelPhoneNumber = NormalizePhoneNumberToE164(model.PhoneNumber);
            if (string.IsNullOrEmpty(normalizedModelPhoneNumber))
            {
                _logger.LogWarning("Register: Invalid phone number format provided: {PhoneNumber}", model.PhoneNumber);
                return BadRequest(new { success = false, message = "Số điện thoại không hợp lệ." });
            }
            model.PhoneNumber = normalizedModelPhoneNumber;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Register: Model state is invalid for phone {PhoneNumber}. Errors: {Errors}", model.PhoneNumber, string.Join("; ", errors));
                return BadRequest(new { success = false, message = "Dữ liệu đăng ký không hợp lệ.", errors = errors });
            }

            _logger.LogInformation("Register: Attempting to register new user with PhoneNumber {PhoneNumber}, FullName: {FullName}", model.PhoneNumber, model.FullName);

            var user = await _userManager.FindByNameAsync(model.PhoneNumber);

            if (user != null)
            {
                if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    _logger.LogWarning("Register: User with phone {PhoneNumber} already exists and has a password.", model.PhoneNumber);
                    return BadRequest(new { success = false, message = "Số điện thoại này đã được đăng ký. Vui lòng đăng nhập." });
                }
                else
                {
                    _logger.LogInformation("Register: User {PhoneNumber} exists but has no password. Setting password and profile.", model.PhoneNumber);

                    var addPasswordResult = await _userManager.AddPasswordAsync(user, model.Password);
                    if (!addPasswordResult.Succeeded)
                    {
                        _logger.LogError("Register: Failed to add password for existing user {UserName}. Errors: {Errors}", user.UserName, string.Join(", ", addPasswordResult.Errors.Select(e => e.Description)));
                        return StatusCode(500, new { success = false, message = "Không thể đặt mật khẩu cho tài khoản hiện có.", errors = addPasswordResult.Errors.Select(e => e.Description) });
                    }
                    _logger.LogInformation("Register: Password added for existing user {UserName}.", user.UserName);

                    if (user.UserProfile == null) user.UserProfile = new UserProfile { UserId = user.Id };
                    user.UserProfile.FullName = model.FullName;
                    if (!user.PhoneNumberConfirmed) user.PhoneNumberConfirmed = true;

                    var updateUserResult = await _userManager.UpdateAsync(user);
                    if (!updateUserResult.Succeeded)
                    {
                        _logger.LogError("Register: Failed to update user profile for {UserName}. Errors: {Errors}", user.UserName, string.Join(", ", updateUserResult.Errors.Select(e => e.Description)));
                        return StatusCode(500, new { success = false, message = "Không thể cập nhật thông tin người dùng.", errors = updateUserResult.Errors.Select(e => e.Description) });
                    }
                    _logger.LogInformation("Register: User profile updated for existing user {UserName}.", user.UserName);
                }
            }
            else
            {
                _logger.LogInformation("Register: User with phone {PhoneNumber} not found. Creating new user.", model.PhoneNumber);

                user = new ApplicationUser
                {
                    UserName = model.PhoneNumber,
                    PhoneNumber = model.PhoneNumber,
                    PhoneNumberConfirmed = true,
                    Email = $"user_{Guid.NewGuid().ToString("N").Substring(0, 8)}@placeholder.sportbook.com",
                    EmailConfirmed = false,
                    UserProfile = new UserProfile
                    {
                        FullName = model.FullName,
                        RegisteredDate = DateTime.UtcNow,
                        AccountStatusByAdmin = AccountStatus.Active
                    }
                };

                var createUserResult = await _userManager.CreateAsync(user, model.Password);
                if (!createUserResult.Succeeded)
                {
                    _logger.LogError("Register: Failed to create user {UserName} with password. Errors: {Errors}", model.PhoneNumber, string.Join(", ", createUserResult.Errors.Select(e => e.Description)));
                    return StatusCode(500, new { success = false, message = "Không thể tạo tài khoản mới. Vui lòng thử lại.", errors = createUserResult.Errors.Select(e => e.Description) });
                }
                _logger.LogInformation("Register: New user {UserName} created successfully with password and FullName {FullName}.", user.UserName, model.FullName);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation("Register: User {UserName} signed in after registration.", user.UserName);

            return Ok(new { success = true, message = "Đăng ký tài khoản thành công!", redirectUrl = Url.Action("Index", "Home") });
        }

        // Action cũ CompleteRegistration (đã đổi tên thành Register ở trên)
        // [HttpPost]
        // [AllowAnonymous]
        // [Route("Account/CompleteRegistration")]
        // public async Task<IActionResult> CompleteRegistration([FromBody] CompleteRegistrationViewModel model) { ... }


        // --- ĐĂNG NHẬP ---
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User)) return RedirectToLocal(returnUrl);
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginWithPasswordViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginWithPasswordViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            _logger.LogInformation("Login POST: Attempting login for raw PhoneNumber {RawPhoneNumber}. RememberMe: {RememberMe}", model.PhoneNumber, model.RememberMe);
            string normalizedPhoneNumber = NormalizePhoneNumberToE164(model.PhoneNumber);

            if (string.IsNullOrEmpty(normalizedPhoneNumber))
            {
                _logger.LogWarning("Login POST: Invalid phone number format entered: {RawPhoneNumber}", model.PhoneNumber);
                ModelState.AddModelError("PhoneNumber", "Số điện thoại không hợp lệ.");
                return View(model);
            }
            _logger.LogInformation("Login POST: Normalized PhoneNumber to {NormalizedPhoneNumber}", normalizedPhoneNumber);

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(normalizedPhoneNumber);
                if (user != null)
                {
                    if (string.IsNullOrEmpty(user.PasswordHash))
                    {
                        ModelState.AddModelError(string.Empty, "Tài khoản của bạn chưa có mật khẩu. Vui lòng hoàn tất đăng ký.");
                        return View(model);
                    }
                    if (!user.PhoneNumberConfirmed)
                    {
                        ModelState.AddModelError(string.Empty, "Số điện thoại chưa được xác thực.");
                        return View(model);
                    }

                    var result = await _signInManager.PasswordSignInAsync(user: user, password: model.Password, isPersistent: model.RememberMe, lockoutOnFailure: true);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User {NormalizedPhoneNumber} logged in successfully with password.", normalizedPhoneNumber);

                        if (await _userManager.IsInRoleAsync(user, "StandardCourtOwner") ||
                            await _userManager.IsInRoleAsync(user, "ProCourtOwner"))
                        {
                            _logger.LogInformation("User {NormalizedPhoneNumber} is a court owner or admin. Redirecting to CourtOwnerDashboard.", normalizedPhoneNumber);
                            return RedirectToAction("Index", "CourtOwnerDashboard");
                        }

                        return RedirectToLocal(returnUrl);
                    }
                    if (result.IsLockedOut) { _logger.LogWarning("User {NormalizedPhoneNumber} account locked out.", normalizedPhoneNumber); ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa. Vui lòng thử lại sau."); }
                    else if (result.IsNotAllowed) { _logger.LogWarning("User {NormalizedPhoneNumber} is not allowed to sign in.", normalizedPhoneNumber); ModelState.AddModelError(string.Empty, "Tài khoản chưa được kích hoạt hoặc không được phép đăng nhập."); }
                    else { _logger.LogWarning("Login POST: PasswordSignInAsync failed for {UserName}.", user.UserName); ModelState.AddModelError(string.Empty, "Thông tin đăng nhập không chính xác."); }
                }
                else { _logger.LogWarning("Login POST: User with (normalized) PhoneNumber {NormalizedPhoneNumber} not found.", normalizedPhoneNumber); ModelState.AddModelError(string.Empty, "Thông tin đăng nhập không chính xác (không tìm thấy SĐT)."); }
            }
            return View(model);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            if (_signInManager.IsSignedIn(User)) return RedirectToAction("Index", "Home");
            return View(); // View này sẽ trực tiếp hiển thị form nhập SĐT
        }

        // Action mới để kiểm tra số điện thoại cho mục đích đặt lại mật khẩu
        [HttpPost]
        [AllowAnonymous]
        [Route("Account/CheckPhoneNumberForPasswordReset")]
        public async Task<IActionResult> CheckPhoneNumberForPasswordReset([FromBody] ForgotPasswordViewModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.PhoneNumber))
            {
                return BadRequest(new { success = false, message = "Vui lòng cung cấp số điện thoại." });
            }

            string normalizedPhoneNumber = NormalizePhoneNumberToE164(model.PhoneNumber);
            if (string.IsNullOrEmpty(normalizedPhoneNumber))
            {
                _logger.LogWarning("CheckPhoneNumberForPasswordReset: Invalid phone number format provided: {PhoneNumber}", model.PhoneNumber);
                return BadRequest(new { success = false, message = "Số điện thoại không hợp lệ." });
            }

            var user = await _userManager.FindByNameAsync(normalizedPhoneNumber);

            // **QUAN TRỌNG:** Để tránh tiết lộ thông tin tài khoản, luôn trả về thành công nếu không tìm thấy.
            // Điều này khiến kẻ tấn công không biết SĐT có tồn tại hay không.
            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
            {
                _logger.LogWarning("CheckPhoneNumberForPasswordReset: User {PhoneNumber} not found or has no password set. Returning success for security.", normalizedPhoneNumber);
                // Giả vờ thành công để không tiết lộ SĐT tồn tại hay không
                return Ok(new { success = true, message = "Số điện thoại hợp lệ. Vui lòng đặt lại mật khẩu mới." });
            }

            // Nếu người dùng tồn tại và có mật khẩu, cho phép chuyển sang bước đặt lại
            _logger.LogInformation("CheckPhoneNumberForPasswordReset: Phone number {PhoneNumber} found for password reset.", normalizedPhoneNumber);
            return Ok(new { success = true, message = "Số điện thoại hợp lệ. Vui lòng đặt lại mật khẩu mới." });
        }


        // Action để xử lý đặt lại mật khẩu (không OTP)
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Route("Account/ResetPasswordNoOtp")] // Route mới cho action này
        public async Task<IActionResult> ResetPasswordNoOtp([FromBody] ResetPasswordViewModel model) // Sử dụng ResetPasswordViewModel
        {
            string normalizedModelPhoneNumber = NormalizePhoneNumberToE164(model.PhoneNumber);
            if (string.IsNullOrEmpty(normalizedModelPhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "Số điện thoại không hợp lệ.");
                _logger.LogWarning("ResetPasswordNoOtp POST: Invalid phone number format provided: {PhoneNumber}", model.PhoneNumber);
                return BadRequest(new { success = false, message = "Số điện thoại không hợp lệ." });
            }
            model.PhoneNumber = normalizedModelPhoneNumber;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ResetPasswordNoOtp POST: ModelState is invalid for phone {PhoneNumber}. Errors: {Errors}", model.PhoneNumber, string.Join("; ", errors));
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại mật khẩu.", errors = errors });
            }

            _logger.LogInformation("ResetPasswordNoOtp POST: Attempting password reset for PhoneNumber {PhoneNumber}", model.PhoneNumber);

            var user = await _userManager.FindByNameAsync(model.PhoneNumber);
            if (user == null || string.IsNullOrEmpty(user.PasswordHash)) // Nếu không tìm thấy user hoặc user không có mật khẩu (chưa hoàn tất đăng ký)
            {
                _logger.LogWarning("ResetPasswordNoOtp POST: User {Phone} not found or no password set. Returning success to prevent enumeration.", model.PhoneNumber);
                // Vẫn trả về thành công để tránh tiết lộ thông tin.
                return Ok(new { success = true, message = "Nếu tài khoản của bạn tồn tại, mật khẩu đã được đặt lại thành công." });
            }

            // --- VỚI LÝ DO BẢO MẬT, LUÔN CÂN NHẮC THÊM CÁC CƠ CHẾ XÁC THỰC BỔ SUNG TẠI ĐÂY ---
            // Ví dụ: Kiểm tra xem request có đến từ cùng phiên mà SĐT đã được kiểm tra ở bước 1 không (session flag)
            // Hoặc gửi email/SMS có mã hoặc link đặt lại duy nhất (phương pháp an toàn nhất)
            // ---

            var identityResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            _logger.LogInformation("ResetPasswordNoOtp POST: Generated password reset token for user {UserName}.", user.UserName);

            var resetPassResult = await _userManager.ResetPasswordAsync(user, identityResetToken, model.Password);

            if (resetPassResult.Succeeded)
            {
                _logger.LogInformation("ResetPasswordNoOtp POST: Password reset successfully for {UserName}.", user.UserName);
                await _userManager.UpdateSecurityStampAsync(user); // Đăng xuất người dùng khỏi các phiên cũ
                return Ok(new { success = true, message = "Mật khẩu của bạn đã được đặt lại thành công!", redirectUrl = Url.Action("Login", "Account") });
            }
            else
            {
                var errors = resetPassResult.Errors.Select(e => e.Description).ToList();
                _logger.LogError("ResetPasswordNoOtp POST: Failed to reset password for {UserName}. Errors: {Errors}", user.UserName, string.Join(", ", errors));
                return BadRequest(new { success = false, message = "Không thể đặt lại mật khẩu. Vui lòng kiểm tra lại.", errors = errors });
            }
        }


        // --- ĐĂNG XUẤT ---
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(Login));
        }
    }

    // ViewModel cho yêu cầu xác thực OTP (Nếu không dùng nữa, có thể xóa)
    public class OtpVerificationRequest
    {
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string OtpCode { get; set; }
    }
}