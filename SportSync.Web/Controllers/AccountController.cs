using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportSync.Data.Entities; // Đảm bảo namespace này đúng
using SportSync.Data.Enums;   // Đảm bảo namespace này đúng
using SportSync.Web.Models.ViewModels.Account; // Đảm bảo namespace này đúng
using System;
using System.Linq;
using System.Text.RegularExpressions; // Thêm cho Regex
using System.Threading.Tasks;

// ViewModel để nhận IdToken từ client (đã có ở file trước)
// public class FirebaseLoginRequest { ... }

namespace SportSync.Web.Controllers // Đảm bảo namespace này đúng
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        // private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger
            /*, RoleManager<IdentityRole> roleManager */)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            // _roleManager = roleManager;
        }

        // Helper function to normalize phone number (giữ nguyên)
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

        // --- KIỂM TRA SỐ ĐIỆN THOẠI TỒN TẠI ---
        [HttpGet]
        [AllowAnonymous]
        [Route("Account/CheckPhoneNumberExistence")]
        public async Task<IActionResult> CheckPhoneNumberExistence(string phoneNumber)
        {
            string normalizedPhoneNumber = NormalizePhoneNumberToE164(phoneNumber);
            if (string.IsNullOrEmpty(normalizedPhoneNumber))
            {
                return Ok(new { exists = false, message = "Số điện thoại không hợp lệ." }); // Hoặc BadRequest
            }

            var user = await _userManager.FindByNameAsync(normalizedPhoneNumber); // UserName là SĐT đã chuẩn hóa
            if (user != null)
            {
                // Kiểm tra thêm nếu user đã có mật khẩu hay chưa, tùy theo logic bạn muốn
                // Hiện tại, chỉ cần SĐT tồn tại là không cho đăng ký lại qua luồng này.
                bool hasPassword = !string.IsNullOrEmpty(user.PasswordHash);
                _logger.LogInformation("CheckPhoneNumberExistence: Phone number {PhoneNumber} exists. HasPassword: {HasPassword}", normalizedPhoneNumber, hasPassword);
                return Ok(new { exists = true, message = "Số điện thoại này đã được đăng ký." });
            }
            _logger.LogInformation("CheckPhoneNumberExistence: Phone number {PhoneNumber} does not exist.", normalizedPhoneNumber);
            return Ok(new { exists = false });
        }


        // --- ĐĂNG KÝ ---
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
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

            if (!ModelState.IsValid) // Kiểm tra ModelState sau khi đã chuẩn hóa SĐT và cập nhật model
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("CompleteRegistration: ModelState is invalid. Errors: {Errors}", string.Join("; ", errors));
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ.", errors = errors });
            }

            _logger.LogInformation("CompleteRegistration: Attempting for PhoneNumber {PhoneNumber}, FullName: {FullName}", model.PhoneNumber, model.FullName);

            FirebaseToken decodedToken = null;
            string firebaseVerifiedPhoneNumber = null;

            try
            {
                if (FirebaseApp.DefaultInstance == null)
                {
                    _logger.LogError("CompleteRegistration: Firebase Admin SDK NOT INITIALIZED.");
                    return StatusCode(500, new { success = false, message = "Lỗi cấu hình máy chủ." });
                }

                decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(model.FirebaseIdToken);
                _logger.LogInformation("CompleteRegistration: Firebase ID Token verified. UID: {FirebaseUid}", decodedToken.Uid);

                if (decodedToken.Claims.TryGetValue("phone_number", out var phoneNumberClaimValue) && phoneNumberClaimValue != null)
                {
                    firebaseVerifiedPhoneNumber = phoneNumberClaimValue.ToString();
                }

                if (string.IsNullOrEmpty(firebaseVerifiedPhoneNumber))
                {
                    _logger.LogWarning("CompleteRegistration: 'phone_number' claim not found in Firebase token. Using (normalized) PhoneNumber from request model: {RequestPhoneNumber} as fallback.", model.PhoneNumber);
                    firebaseVerifiedPhoneNumber = model.PhoneNumber;
                }

                if (firebaseVerifiedPhoneNumber != model.PhoneNumber)
                {
                    _logger.LogWarning("CompleteRegistration: Phone number mismatch! Token: {TokenPhoneNumber}, Model: {ModelPhoneNumber}. Denying registration.",
                                       firebaseVerifiedPhoneNumber, model.PhoneNumber);
                    return BadRequest(new { success = false, message = "Xác thực số điện thoại không thành công hoặc thông tin không khớp." });
                }
            }
            catch (FirebaseAuthException ex)
            {
                _logger.LogError(ex, "CompleteRegistration: Invalid Firebase ID Token for PhoneNumber {PhoneNumber}.", model.PhoneNumber);
                return Unauthorized(new { success = false, message = "Token xác thực không hợp lệ hoặc đã hết hạn." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CompleteRegistration: Error verifying Firebase ID Token for {PhoneNumber}.", model.PhoneNumber);
                return StatusCode(500, new { success = false, message = "Lỗi máy chủ khi xác thực token." });
            }

            var user = await _userManager.FindByNameAsync(firebaseVerifiedPhoneNumber);

            if (user != null)
            {
                if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    _logger.LogWarning("CompleteRegistration: User with phone {PhoneNumber} already exists AND has a password.", firebaseVerifiedPhoneNumber);
                    return BadRequest(new { success = false, message = "Số điện thoại này đã được đăng ký và có mật khẩu. Vui lòng đăng nhập hoặc sử dụng chức năng 'Quên mật khẩu'." });
                }
                else
                {
                    _logger.LogInformation("CompleteRegistration: User {PhoneNumber} exists but has no password. Proceeding to set password and update profile.", firebaseVerifiedPhoneNumber);
                    var addPasswordResult = await _userManager.AddPasswordAsync(user, model.Password);
                    if (!addPasswordResult.Succeeded)
                    {
                        _logger.LogError("CompleteRegistration: Failed to add password for existing user {UserName}. Errors: {Errors}",
                                         user.UserName, string.Join(", ", addPasswordResult.Errors.Select(e => e.Description)));
                        return StatusCode(500, new { success = false, message = "Không thể đặt mật khẩu cho tài khoản.", errors = addPasswordResult.Errors.Select(e => e.Description) });
                    }
                    _logger.LogInformation("CompleteRegistration: Password added successfully for existing user {UserName}.", user.UserName);

                    // Cập nhật UserProfile với FullName
                    if (user.UserProfile == null) user.UserProfile = new UserProfile { UserId = user.Id }; // Đảm bảo UserProfile tồn tại
                    user.UserProfile.FullName = model.FullName;
                    // Không cần gọi _userManager.UpdateAsync(user) riêng cho UserProfile nếu nó được EF Core theo dõi qua user.

                    if (!user.PhoneNumberConfirmed) user.PhoneNumberConfirmed = true;

                    var updateUserResult = await _userManager.UpdateAsync(user); // Lưu thay đổi cho UserProfile và PhoneNumberConfirmed
                    if (!updateUserResult.Succeeded)
                    {
                        _logger.LogError("CompleteRegistration: Failed to update user profile for {UserName}. Errors: {Errors}",
                                         user.UserName, string.Join(", ", updateUserResult.Errors.Select(e => e.Description)));
                        // Có thể không phải là lỗi nghiêm trọng nếu mật khẩu đã được đặt
                    }
                }
            }
            else
            {
                _logger.LogInformation("CompleteRegistration: User with phone {PhoneNumber} not found. Creating new user.", firebaseVerifiedPhoneNumber);
                user = new ApplicationUser
                {
                    UserName = firebaseVerifiedPhoneNumber,
                    PhoneNumber = firebaseVerifiedPhoneNumber,
                    PhoneNumberConfirmed = true,
                    Email = $"user_{Guid.NewGuid().ToString("N").Substring(0, 8)}@placeholder.sportbook.com",
                    EmailConfirmed = false,
                    UserProfile = new UserProfile // Khởi tạo UserProfile khi tạo ApplicationUser
                    {
                        // UserId sẽ được EF Core tự động gán khi user được tạo và lưu
                        FullName = model.FullName, // Gán FullName
                        RegisteredDate = DateTime.UtcNow,
                        AccountStatusByAdmin = AccountStatus.Active
                    }
                };

                var createUserResult = await _userManager.CreateAsync(user, model.Password);
                if (!createUserResult.Succeeded)
                {
                    _logger.LogError("CompleteRegistration: Failed to create user {UserName} with password. Errors: {Errors}",
                                     firebaseVerifiedPhoneNumber, string.Join(", ", createUserResult.Errors.Select(e => e.Description)));
                    return StatusCode(500, new { success = false, message = "Không thể tạo tài khoản.", errors = createUserResult.Errors.Select(e => e.Description) });
                }
                _logger.LogInformation("CompleteRegistration: User {UserName} created successfully with password and FullName {FullName}.", user.UserName, model.FullName);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation("CompleteRegistration: User {UserName} signed in.", user.UserName);

            return Ok(new { success = true, message = "Hoàn tất đăng ký và đăng nhập thành công!", redirectUrl = Url.Action("Index", "Home") });
        }

        // --- ĐĂNG NHẬP --- (Giữ nguyên)
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }
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
                _logger.LogInformation("Login POST: ModelState is valid for (normalized) PhoneNumber {NormalizedPhoneNumber}.", normalizedPhoneNumber);
                var user = await _userManager.FindByNameAsync(normalizedPhoneNumber);

                if (user != null)
                {
                    _logger.LogInformation("Login POST: User {UserName} found. Current PasswordHash: {PasswordHash}. PhoneNumberConfirmed: {PhoneNumberConfirmed}", user.UserName, user.PasswordHash, user.PhoneNumberConfirmed);
                    if (string.IsNullOrEmpty(user.PasswordHash))
                    {
                        _logger.LogWarning("Login POST: User {UserName} has no password set. Cannot login with password.", user.UserName);
                        ModelState.AddModelError(string.Empty, "Tài khoản của bạn chưa có mật khẩu. Vui lòng sử dụng chức năng 'Đăng ký' để hoàn tất và đặt mật khẩu.");
                        return View(model);
                    }
                    if (!user.PhoneNumberConfirmed)
                    {
                        _logger.LogWarning("Login POST: User {UserName}'s phone number is not confirmed.", user.UserName);
                        ModelState.AddModelError(string.Empty, "Số điện thoại chưa được xác thực. Vui lòng xác thực số điện thoại trước.");
                        return View(model);
                    }
                    var passwordCheckResult = await _userManager.CheckPasswordAsync(user, model.Password);
                    _logger.LogInformation("Login POST: Password check for user {UserName} result: {PasswordCheckResult}", user.UserName, passwordCheckResult);

                    if (passwordCheckResult)
                    {
                        _logger.LogInformation("Login POST: Password check succeeded for {UserName}. Attempting PasswordSignInAsync.", user.UserName);
                        var result = await _signInManager.PasswordSignInAsync(user: user, password: model.Password, isPersistent: model.RememberMe, lockoutOnFailure: true);
                        _logger.LogInformation("Login POST: PasswordSignInAsync result for {UserName}: Succeeded={Succeeded}, IsLockedOut={IsLockedOut}, IsNotAllowed={IsNotAllowed}, RequiresTwoFactor={RequiresTwoFactor}", user.UserName, result.Succeeded, result.IsLockedOut, result.IsNotAllowed, result.RequiresTwoFactor);

                        if (result.Succeeded)
                        {
                            _logger.LogInformation("User {NormalizedPhoneNumber} logged in successfully with password.", normalizedPhoneNumber);
                            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
                            return RedirectToAction("Index", "Home");
                        }
                        if (result.IsLockedOut) { _logger.LogWarning("User {NormalizedPhoneNumber} account locked out.", normalizedPhoneNumber); ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa. Vui lòng thử lại sau."); }
                        else if (result.IsNotAllowed) { _logger.LogWarning("User {NormalizedPhoneNumber} is not allowed to sign in.", normalizedPhoneNumber); ModelState.AddModelError(string.Empty, "Tài khoản chưa được kích hoạt hoặc không được phép đăng nhập."); }
                        else { _logger.LogWarning("Login POST: PasswordSignInAsync failed for {UserName}.", user.UserName); ModelState.AddModelError(string.Empty, "Thông tin đăng nhập không chính xác."); }
                    }
                    else { _logger.LogWarning("Login POST: CheckPasswordAsync failed for user {UserName}.", user.UserName); ModelState.AddModelError(string.Empty, "Thông tin đăng nhập không chính xác (sai mật khẩu)."); }
                }
                else { _logger.LogWarning("Login POST: User with (normalized) PhoneNumber {NormalizedPhoneNumber} not found.", normalizedPhoneNumber); ModelState.AddModelError(string.Empty, "Thông tin đăng nhập không chính xác (không tìm thấy SĐT)."); }
            }
            else { _logger.LogWarning("Login POST: ModelState is invalid. Raw PhoneNumber {RawPhoneNumber}. Errors: {Errors}", model.PhoneNumber, string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))); }
            return View(model);
        }


        // --- QUÊN MẬT KHẨU --- (Giữ nguyên)
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            if (_signInManager.IsSignedIn(User)) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string phoneNumber, string token)
        {
            string normalizedPhoneNumber = NormalizePhoneNumberToE164(phoneNumber);
            if (string.IsNullOrEmpty(normalizedPhoneNumber) || string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Yêu cầu đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.";
                return RedirectToAction(nameof(ForgotPassword));
            }
            var model = new ResetPasswordViewModel { PhoneNumber = normalizedPhoneNumber, FirebaseIdToken = token };
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

            if (!ModelState.IsValid) return View(model);
            _logger.LogInformation("ResetPassword: Attempting for PhoneNumber {PhoneNumber}", model.PhoneNumber);
            FirebaseToken decodedToken = null;
            string tokenVerifiedPhoneNumber = null;

            try
            {
                if (FirebaseApp.DefaultInstance == null) { _logger.LogError("ResetPassword: Firebase Admin SDK NOT INITIALIZED."); ModelState.AddModelError(string.Empty, "Lỗi cấu hình máy chủ."); return View(model); }
                decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(model.FirebaseIdToken);
                _logger.LogInformation("ResetPassword: Firebase ID Token verified. UID: {FirebaseUid}", decodedToken.Uid);
                if (decodedToken.Claims.TryGetValue("phone_number", out var phoneNumberClaimValue) && phoneNumberClaimValue != null) tokenVerifiedPhoneNumber = phoneNumberClaimValue.ToString();
                if (string.IsNullOrEmpty(tokenVerifiedPhoneNumber)) { _logger.LogWarning("ResetPassword: 'phone_number' claim not found. Using model phone: {ModelPhone}", model.PhoneNumber); tokenVerifiedPhoneNumber = model.PhoneNumber; }
                if (tokenVerifiedPhoneNumber != model.PhoneNumber) { _logger.LogWarning("ResetPassword: Phone mismatch! Token: {TokenPhone}, Model: {ModelPhone}.", tokenVerifiedPhoneNumber, model.PhoneNumber); ModelState.AddModelError(string.Empty, "Xác thực SĐT không khớp."); return View(model); }
            }
            catch (FirebaseAuthException ex) { _logger.LogError(ex, "ResetPassword: Invalid Firebase ID Token for {Phone}.", model.PhoneNumber); ModelState.AddModelError(string.Empty, "Token xác thực không hợp lệ/hết hạn."); return View(model); }
            catch (Exception ex) { _logger.LogError(ex, "ResetPassword: Error verifying Firebase ID Token for {Phone}.", model.PhoneNumber); ModelState.AddModelError(string.Empty, "Lỗi máy chủ khi xác thực token."); return View(model); }

            var user = await _userManager.FindByNameAsync(model.PhoneNumber);
            if (user == null) { _logger.LogWarning("ResetPassword: User {Phone} not found.", model.PhoneNumber); TempData["SuccessMessage"] = "Nếu SĐT tồn tại, mật khẩu sẽ được reset."; return RedirectToAction(nameof(Login)); }

            var identityResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            _logger.LogInformation("ResetPassword: Generated Identity reset token for {UserName}.", user.UserName);
            var resetPassResult = await _userManager.ResetPasswordAsync(user, identityResetToken, model.Password);

            if (!resetPassResult.Succeeded)
            {
                _logger.LogError("ResetPassword: Failed to reset password for {UserName}. Errors: {Errors}", user.UserName, string.Join(", ", resetPassResult.Errors.Select(e => e.Description)));
                foreach (var error in resetPassResult.Errors) ModelState.AddModelError(string.Empty, error.Description);
                model.FirebaseIdToken = "";
                return View(model);
            }
            _logger.LogInformation("ResetPassword: Password reset successfully for {UserName}.", user.UserName);
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
            // Chuyển hướng về trang Đăng nhập sau khi logout
            return RedirectToAction(nameof(Login));
        }
    }
}
