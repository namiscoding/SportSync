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
        private readonly ISmsSender _smsSender;

        private const string OtpSessionKeyPrefix = "RegOtp_";
        private const string OtpExpirySessionKeyPrefix = "RegOtpExpiry_";
        private const string OtpVerifiedFlagKeyPrefix = "RegOtpVerified_";

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger,
            ISmsSender smsSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _smsSender = smsSender;
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

        [HttpPost]
        [AllowAnonymous]
        [Route("Account/SendRegistrationOtp")]
        public async Task<IActionResult> SendRegistrationOtp([FromBody] ForgotPasswordViewModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.PhoneNumber))
            {
                return BadRequest(new { success = false, message = "Vui lòng cung cấp số điện thoại." });
            }

            string normalizedPhoneNumber = NormalizePhoneNumberToE164(model.PhoneNumber);
            if (string.IsNullOrEmpty(normalizedPhoneNumber))
            {
                return BadRequest(new { success = false, message = "Số điện thoại không hợp lệ." });
            }

            var existingUser = await _userManager.FindByNameAsync(normalizedPhoneNumber);
            if (existingUser != null && !string.IsNullOrEmpty(existingUser.PasswordHash))
            {
                _logger.LogWarning("SendRegistrationOtp: Attempt to register with existing phone number {PhoneNumber} that already has a password.", normalizedPhoneNumber);
                return Ok(new { success = false, message = "Số điện thoại này đã được đăng ký. Vui lòng đăng nhập hoặc sử dụng chức năng 'Quên mật khẩu'." });
            }

            var otpCode = new Random().Next(100000, 999999).ToString();
            var otpExpiry = DateTime.UtcNow.AddMinutes(5);

            HttpContext.Session.SetString(OtpSessionKeyPrefix + normalizedPhoneNumber, otpCode);
            HttpContext.Session.SetString(OtpExpirySessionKeyPrefix + normalizedPhoneNumber, otpExpiry.ToString("o"));
            HttpContext.Session.Remove(OtpVerifiedFlagKeyPrefix + normalizedPhoneNumber);

            _logger.LogInformation("SendRegistrationOtp: Generated OTP {OtpCode} for {PhoneNumber}, expires at {OtpExpiryUtc}", otpCode, normalizedPhoneNumber, otpExpiry);

            try
            {
                await _smsSender.SendSmsAsync(normalizedPhoneNumber, $"Mã OTP đăng ký SportSync của bạn là: {otpCode}");
                _logger.LogInformation("SendRegistrationOtp: OTP sent successfully to {PhoneNumber}.", normalizedPhoneNumber);
                return Ok(new { success = true, message = "Mã OTP đã được gửi đến số điện thoại của bạn." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendRegistrationOtp: Error sending OTP to {PhoneNumber}.", normalizedPhoneNumber);
                return StatusCode(500, new { success = false, message = "Lỗi gửi OTP. Vui lòng thử lại." });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("Account/VerifyRegistrationOtp")]
        public IActionResult VerifyRegistrationOtp([FromBody] OtpVerificationRequest model)
        {
            if (model == null || string.IsNullOrEmpty(model.PhoneNumber) || string.IsNullOrEmpty(model.OtpCode))
            {
                return BadRequest(new { success = false, message = "Số điện thoại và mã OTP là bắt buộc." });
            }

            string normalizedPhoneNumber = NormalizePhoneNumberToE164(model.PhoneNumber);
            if (string.IsNullOrEmpty(normalizedPhoneNumber))
            {
                return BadRequest(new { success = false, message = "Số điện thoại không hợp lệ." });
            }

            var sessionOtp = HttpContext.Session.GetString(OtpSessionKeyPrefix + normalizedPhoneNumber);
            var sessionOtpExpiryString = HttpContext.Session.GetString(OtpExpirySessionKeyPrefix + normalizedPhoneNumber);

            if (string.IsNullOrEmpty(sessionOtp) || string.IsNullOrEmpty(sessionOtpExpiryString))
            {
                _logger.LogWarning("VerifyRegistrationOtp: No OTP found in session for {PhoneNumber}.", normalizedPhoneNumber);
                return Ok(new { success = false, message = "Mã OTP không tồn tại hoặc đã hết hạn. Vui lòng yêu cầu OTP mới." });
            }

            if (!DateTime.TryParse(sessionOtpExpiryString, out DateTime otpExpiry) || otpExpiry < DateTime.UtcNow)
            {
                _logger.LogWarning("VerifyRegistrationOtp: OTP expired for {PhoneNumber}. Stored Expiry: {StoredExpiryUtc}", normalizedPhoneNumber, sessionOtpExpiryString);
                HttpContext.Session.Remove(OtpSessionKeyPrefix + normalizedPhoneNumber);
                HttpContext.Session.Remove(OtpExpirySessionKeyPrefix + normalizedPhoneNumber);
                return Ok(new { success = false, message = "Mã OTP đã hết hạn. Vui lòng yêu cầu OTP mới." });
            }

            if (sessionOtp != model.OtpCode)
            {
                _logger.LogWarning("VerifyRegistrationOtp: Invalid OTP for {PhoneNumber}. Entered: {EnteredOtp}, Expected: {ExpectedOtp}", normalizedPhoneNumber, model.OtpCode, sessionOtp);
                return Ok(new { success = false, message = "Mã OTP không chính xác." });
            }

            HttpContext.Session.SetString(OtpVerifiedFlagKeyPrefix + normalizedPhoneNumber, "true");
            _logger.LogInformation("VerifyRegistrationOtp: OTP verified successfully for {PhoneNumber}. Verification flag set.", normalizedPhoneNumber);
            return Ok(new { success = true, message = "Xác thực OTP thành công." });
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("Account/CompleteRegistration")]
        public async Task<IActionResult> CompleteRegistration([FromBody] CompleteRegistrationViewModel model)
        {
            string normalizedModelPhoneNumber = NormalizePhoneNumberToE164(model.PhoneNumber);
            if (string.IsNullOrEmpty(normalizedModelPhoneNumber))
            {
                return BadRequest(new { success = false, message = "Số điện thoại không hợp lệ." });
            }
            model.PhoneNumber = normalizedModelPhoneNumber;

            // **THAY ĐỔI Ở ĐÂY: Chỉ kiểm tra ModelState, không kiểm tra OTP trực tiếp nữa**
            if (!ModelState.IsValid) // ModelState vẫn kiểm tra [Required] cho OtpCode, FullName, Password...
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("CompleteRegistration: ModelState is invalid. Errors: {Errors}", string.Join("; ", errors));
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ.", errors = errors });
            }

            _logger.LogInformation("CompleteRegistration: Attempting for PhoneNumber {PhoneNumber}, FullName: {FullName}", model.PhoneNumber, model.FullName);

            // **THAY ĐỔI Ở ĐÂY: Kiểm tra cờ đã xác thực OTP từ session**
            var otpVerifiedFlag = HttpContext.Session.GetString(OtpVerifiedFlagKeyPrefix + model.PhoneNumber);
            if (otpVerifiedFlag != "true")
            {
                _logger.LogWarning("CompleteRegistration: OTP verification flag not found or invalid for {PhoneNumber}. Registration denied. User might have skipped OTP verification step.", model.PhoneNumber);
                // Trả về lỗi chung hoặc lỗi cụ thể cho trường OTP nếu muốn
                // ModelState.AddModelError("OtpCode", "Vui lòng xác thực OTP trước.");
                return BadRequest(new { success = false, message = "Vui lòng xác thực OTP ở bước trước." });
            }

            // **THAY ĐỔI Ở ĐÂY: Bỏ qua việc kiểm tra lại sessionOtp và model.OtpCode ở đây**
            // Chúng ta tin tưởng vào cờ OtpVerifiedFlagKeyPrefix.
            // Nếu muốn an toàn hơn, bạn có thể giữ lại việc kiểm tra model.OtpCode với sessionOtp,
            // nhưng điều đó có nghĩa là OTP vẫn phải được lưu trong session cho đến bước này.
            // Hiện tại, chúng ta sẽ chỉ dựa vào cờ.

            // Xóa cờ khỏi session sau khi đã kiểm tra
            // OTP và Expiry có thể đã được xóa hoặc sẽ hết hạn tự nhiên.
            // Để chắc chắn, có thể xóa cả OTP và Expiry nếu bạn muốn.
            HttpContext.Session.Remove(OtpVerifiedFlagKeyPrefix + model.PhoneNumber);
            HttpContext.Session.Remove(OtpSessionKeyPrefix + model.PhoneNumber); // Xóa luôn OTP sau khi dùng cờ
            HttpContext.Session.Remove(OtpExpirySessionKeyPrefix + model.PhoneNumber);
            _logger.LogInformation("CompleteRegistration: OTP verification flag and OTP data cleared from session for {PhoneNumber}.", model.PhoneNumber);

            var user = await _userManager.FindByNameAsync(model.PhoneNumber);

            if (user != null)
            {
                if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    _logger.LogWarning("CompleteRegistration: User with phone {PhoneNumber} already exists AND has a password.", model.PhoneNumber);
                    return BadRequest(new { success = false, message = "Số điện thoại này đã được đăng ký và có mật khẩu." });
                }
                else
                {
                    _logger.LogInformation("CompleteRegistration: User {PhoneNumber} exists but has no password. Setting password and profile.", model.PhoneNumber);
                    var addPasswordResult = await _userManager.AddPasswordAsync(user, model.Password);
                    if (!addPasswordResult.Succeeded)
                    {
                        _logger.LogError("CompleteRegistration: Failed to add password for existing user {UserName}. Errors: {Errors}", user.UserName, string.Join(", ", addPasswordResult.Errors.Select(e => e.Description)));
                        return StatusCode(500, new { success = false, message = "Không thể đặt mật khẩu.", errors = addPasswordResult.Errors.Select(e => e.Description) });
                    }
                    _logger.LogInformation("CompleteRegistration: Password added for existing user {UserName}.", user.UserName);

                    if (user.UserProfile == null) user.UserProfile = new UserProfile { UserId = user.Id };
                    user.UserProfile.FullName = model.FullName;
                    if (!user.PhoneNumberConfirmed) user.PhoneNumberConfirmed = true;

                    var updateUserResult = await _userManager.UpdateAsync(user);
                    if (!updateUserResult.Succeeded) _logger.LogError("CompleteRegistration: Failed to update user profile for {UserName}.", user.UserName);
                }
            }
            else
            {
                _logger.LogInformation("CompleteRegistration: User with phone {PhoneNumber} not found. Creating new user.", model.PhoneNumber);
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
                    _logger.LogError("CompleteRegistration: Failed to create user {UserName} with password. Errors: {Errors}", model.PhoneNumber, string.Join(", ", createUserResult.Errors.Select(e => e.Description)));
                    return StatusCode(500, new { success = false, message = "Không thể tạo tài khoản.", errors = createUserResult.Errors.Select(e => e.Description) });
                }
                _logger.LogInformation("CompleteRegistration: User {UserName} created successfully with password and FullName {FullName}.", user.UserName, model.FullName);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation("CompleteRegistration: User {UserName} signed in.", user.UserName);

            return Ok(new { success = true, message = "Đăng ký tài khoản thành công!", redirectUrl = Url.Action("Index", "Home") });
        }

        // --- ĐĂNG NHẬP ---
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User)) return RedirectToLocal(returnUrl); // Chuyển hướng nếu đã đăng nhập
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

                        // **THAY ĐỔI CHUYỂN HƯỚNG SAU ĐĂNG NHẬP**
                        if (await _userManager.IsInRoleAsync(user, "StandardCourtOwner") ||
                            await _userManager.IsInRoleAsync(user, "ProCourtOwner"))
                        {
                            _logger.LogInformation("User {NormalizedPhoneNumber} is a court owner or admin. Redirecting to CourtOwnerDashboard.", normalizedPhoneNumber);
                            return RedirectToAction("Index", "CourtOwnerDashboard");
                        }

                        return RedirectToLocal(returnUrl); // Chuyển hướng đến returnUrl hoặc trang chủ
                    }
                    // ... (xử lý lỗi IsLockedOut, IsNotAllowed, sai mật khẩu giữ nguyên)
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


        // --- QUÊN MẬT KHẨU --- (Sẽ cần cập nhật tương tự cho OTP server)
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            if (_signInManager.IsSignedIn(User)) return RedirectToAction("Index", "Home");
            return View(); // View này sẽ cần JavaScript để gọi SendPasswordResetOtp
        }

        // Action mới để gửi OTP cho Quên Mật Khẩu
        [HttpPost]
        [AllowAnonymous]
        [Route("Account/SendPasswordResetOtp")]
        public async Task<IActionResult> SendPasswordResetOtp([FromBody] ForgotPasswordViewModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.PhoneNumber))
                return BadRequest(new { success = false, message = "Vui lòng cung cấp số điện thoại." });

            string normalizedPhoneNumber = NormalizePhoneNumberToE164(model.PhoneNumber);
            if (string.IsNullOrEmpty(normalizedPhoneNumber))
                return BadRequest(new { success = false, message = "Số điện thoại không hợp lệ." });

            var user = await _userManager.FindByNameAsync(normalizedPhoneNumber);
            if (user == null || string.IsNullOrEmpty(user.PasswordHash)) // Chỉ cho reset nếu user tồn tại và có mật khẩu
            {
                _logger.LogWarning("SendPasswordResetOtp: User not found or no password set for {PhoneNumber}.", normalizedPhoneNumber);
                // Trả về success = true để không tiết lộ SĐT có tồn tại hay không, nhưng không gửi OTP
                return Ok(new { success = true, message = "Nếu số điện thoại của bạn đã đăng ký, mã OTP sẽ được gửi." });
            }

            var otpCode = new Random().Next(100000, 999999).ToString();
            var otpExpiry = DateTime.UtcNow.AddMinutes(5);
            string sessionKeyOtp = OtpSessionKeyPrefix + normalizedPhoneNumber + "_Reset";
            string sessionKeyExpiry = OtpExpirySessionKeyPrefix + normalizedPhoneNumber + "_Reset";

            HttpContext.Session.SetString(sessionKeyOtp, otpCode);
            HttpContext.Session.SetString(sessionKeyExpiry, otpExpiry.ToString("o"));
            HttpContext.Session.Remove(OtpVerifiedFlagKeyPrefix + normalizedPhoneNumber + "_Reset"); // Xóa cờ cũ

            _logger.LogInformation("SendPasswordResetOtp: Generated OTP {OtpCode} for {PhoneNumber}", otpCode, normalizedPhoneNumber);
            try
            {
                await _smsSender.SendSmsAsync(normalizedPhoneNumber, $"Mã OTP đặt lại mật khẩu SportSync của bạn là: {otpCode}");
                return Ok(new { success = true, message = "Mã OTP đã được gửi đến số điện thoại của bạn." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendPasswordResetOtp: Error sending OTP to {PhoneNumber}.", normalizedPhoneNumber);
                return StatusCode(500, new { success = false, message = "Lỗi gửi OTP. Vui lòng thử lại." });
            }
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string phoneNumber) // Chỉ cần SĐT để hiển thị form
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                TempData["ErrorMessage"] = "Yêu cầu đặt lại mật khẩu không hợp lệ.";
                return RedirectToAction(nameof(ForgotPassword));
            }
            var model = new ResetPasswordViewModel { PhoneNumber = NormalizePhoneNumberToE164(phoneNumber) ?? phoneNumber };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            string normalizedModelPhoneNumber = NormalizePhoneNumberToE164(model.PhoneNumber);
            if (string.IsNullOrEmpty(normalizedModelPhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "Số điện thoại không hợp lệ.");
                return View(model);
            }
            model.PhoneNumber = normalizedModelPhoneNumber;

            if (!ModelState.IsValid) return View(model); // OtpCode, Password, ConfirmPassword sẽ được validate ở đây
            _logger.LogInformation("ResetPassword POST: Attempting for PhoneNumber {PhoneNumber}", model.PhoneNumber);

            // Xác thực OTP từ Session
            string sessionKeyOtp = OtpSessionKeyPrefix + model.PhoneNumber + "_Reset";
            string sessionKeyExpiry = OtpExpirySessionKeyPrefix + model.PhoneNumber + "_Reset";
            var sessionOtp = HttpContext.Session.GetString(sessionKeyOtp);
            var sessionOtpExpiryString = HttpContext.Session.GetString(sessionKeyExpiry);

            if (string.IsNullOrEmpty(sessionOtp) || string.IsNullOrEmpty(sessionOtpExpiryString) ||
                !DateTime.TryParse(sessionOtpExpiryString, out DateTime otpExpiry) || otpExpiry < DateTime.UtcNow ||
                sessionOtp != model.OtpCode)
            {
                _logger.LogWarning("ResetPassword POST: Invalid or expired OTP for {PhoneNumber}.", model.PhoneNumber);
                ModelState.AddModelError("OtpCode", "Mã OTP không hợp lệ hoặc đã hết hạn.");
                return View(model);
            }
            HttpContext.Session.Remove(sessionKeyOtp);
            HttpContext.Session.Remove(sessionKeyExpiry);
            _logger.LogInformation("ResetPassword POST: OTP verified for {PhoneNumber}.", model.PhoneNumber);

            var user = await _userManager.FindByNameAsync(model.PhoneNumber);
            if (user == null)
            {
                _logger.LogWarning("ResetPassword POST: User {Phone} not found (should not happen if OTP was sent).", model.PhoneNumber);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi. Vui lòng thử lại từ đầu.";
                return RedirectToAction(nameof(ForgotPassword));
            }

            var identityResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetPassResult = await _userManager.ResetPasswordAsync(user, identityResetToken, model.Password);

            if (!resetPassResult.Succeeded)
            {
                _logger.LogError("ResetPassword POST: Failed to reset password for {UserName}. Errors: {Errors}", user.UserName, string.Join(", ", resetPassResult.Errors.Select(e => e.Description)));
                foreach (var error in resetPassResult.Errors) ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }
            _logger.LogInformation("ResetPassword POST: Password reset successfully for {UserName}.", user.UserName);
            TempData["SuccessMessage"] = "Mật khẩu đã được đặt lại. Vui lòng đăng nhập.";
            return RedirectToAction(nameof(Login));
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

    // ViewModel cho yêu cầu xác thực OTP (dùng chung cho Register và ResetPassword ở client)
    public class OtpVerificationRequest
    {
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string OtpCode { get; set; }
    }
}
