using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SportSync.Data;
using SportSync.Data.Entities;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using SportSync.Web.Models.ViewModels.Profile;

namespace SportSync.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<ProfileController> _logger;
        private readonly ApplicationDbContext _context;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ProfileController> logger,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                _logger.LogWarning("ProfileController.Index (GET): UserId not found for current User Principal.");
                return Challenge();
            }

            _logger.LogInformation("ProfileController.Index (GET): Fetching profile for UserId {UserId}", userId);
            var userWithProfile = await _context.Users
                                          .Include(u => u.UserProfile)
                                          .FirstOrDefaultAsync(u => u.Id == userId);

            if (userWithProfile == null)
            {
                _logger.LogWarning("ProfileController.Index (GET): User with ID {UserId} not found in database.", userId);
                return NotFound($"Không thể tìm thấy người dùng với ID '{userId}'.");
            }
            _logger.LogInformation("ProfileController.Index (GET): User found. UserProfile is null: {IsUserProfileNull}", userWithProfile.UserProfile == null);
            if (userWithProfile.UserProfile != null)
            {
                _logger.LogInformation("ProfileController.Index (GET): Existing FullName in DB: '{ExistingFullName}'", userWithProfile.UserProfile.FullName);
            }

            var model = new ManageProfileViewModel // ViewModel này giờ không còn StatusMessage
            {
                PhoneNumber = userWithProfile.PhoneNumber,
                FullName = userWithProfile.UserProfile?.FullName
            };

            // **THAY ĐỔI Ở ĐÂY: Sử dụng ViewData để truyền StatusMessage**
            if (TempData.ContainsKey("StatusMessage"))
            {
                ViewData["StatusMessage"] = TempData["StatusMessage"].ToString();
                _logger.LogInformation("ProfileController.Index (GET): StatusMessage from TempData set to ViewData: '{StatusMessage}'", ViewData["StatusMessage"]);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ManageProfileViewModel model) // ViewModel này giờ không còn StatusMessage
        {
            _logger.LogInformation("ProfileController.Index (POST): Received FullName from model: '{ModelFullName}'", model.FullName);

            // **ModelState.IsValid sẽ không còn bị ảnh hưởng bởi StatusMessage nữa**
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ProfileController.Index (POST): ModelState is invalid. Errors: {Errors}",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                var currentUserForPhone = await _userManager.GetUserAsync(User);
                // Nạp lại PhoneNumber vào model vì nó không được POST lên (nếu là readonly và không có input)
                // Tuy nhiên, nếu PhoneNumber có trong ViewModel và được POST, dòng này có thể không cần thiết.
                // ViewModel hiện tại có PhoneNumber, nhưng trong View nó là readonly.
                // Để an toàn, chúng ta vẫn gán lại.
                if (model != null && currentUserForPhone != null)
                {
                    model.PhoneNumber = currentUserForPhone.PhoneNumber;
                }
                return View(model);
            }

            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                _logger.LogWarning("ProfileController.Index (POST): Current UserId not found.");
                return Challenge();
            }

            _logger.LogInformation("ProfileController.Index (POST): CurrentUserId: {CurrentUserId}", currentUserId);
            var userToUpdate = await _context.Users
                                       .Include(u => u.UserProfile)
                                       .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (userToUpdate == null)
            {
                _logger.LogWarning("ProfileController.Index (POST): User with ID {UserId} not found for update.", currentUserId);
                return NotFound("Không tìm thấy người dùng để cập nhật.");
            }
            _logger.LogInformation("ProfileController.Index (POST): UserToUpdate found. UserProfile is null: {IsUserProfileNull}", userToUpdate.UserProfile == null);
            if (userToUpdate.UserProfile != null)
            {
                _logger.LogInformation("ProfileController.Index (POST): Current FullName in DB before update: '{CurrentDbFullName}'", userToUpdate.UserProfile.FullName);
            }

            bool profileDataChanged = false;

            if (userToUpdate.UserProfile == null)
            {
                _logger.LogInformation("ProfileController.Index (POST): UserProfile is null for user {UserName}. Initializing.", userToUpdate.UserName);
                userToUpdate.UserProfile = new UserProfile { UserId = userToUpdate.Id };
                _context.UserProfiles.Add(userToUpdate.UserProfile);

                if (!string.IsNullOrEmpty(model.FullName))
                {
                    userToUpdate.UserProfile.FullName = model.FullName;
                    profileDataChanged = true;
                    _logger.LogInformation("ProfileController.Index (POST): New UserProfile created and FullName set to '{NewFullName}'. profileDataChanged = true.", model.FullName);
                }
                else
                {
                    _logger.LogInformation("ProfileController.Index (POST): New UserProfile created, but model.FullName is empty. profileDataChanged = false.");
                }
            }
            else
            {
                string currentDbFullName = userToUpdate.UserProfile.FullName;
                string modelFullName = model.FullName;

                if (!string.Equals(currentDbFullName, modelFullName))
                {
                    _logger.LogInformation("ProfileController.Index (POST): FullName changing from '{CurrentDbFullName}' to '{ModelFullName}'.", currentDbFullName, modelFullName);
                    userToUpdate.UserProfile.FullName = modelFullName;
                    profileDataChanged = true;
                }
                else
                {
                    _logger.LogInformation("ProfileController.Index (POST): FullName from model ('{ModelFullName}') is the same as in DB ('{CurrentDbFullName}'). profileDataChanged = false.", modelFullName, currentDbFullName);
                }
            }

            _logger.LogInformation("ProfileController.Index (POST): Value of profileDataChanged: {ProfileDataChanged}", profileDataChanged);

            if (profileDataChanged)
            {
                _logger.LogInformation("ProfileController.Index (POST): Attempting to save changes for user {UserName}.", userToUpdate.UserName);
                try
                {
                    int changes = await _context.SaveChangesAsync();
                    _logger.LogInformation("ProfileController.Index (POST): SaveChangesAsync completed. Number of state entries written: {ChangesCount}", changes);

                    if (changes > 0)
                    { // Hoặc kiểm tra profileDataChanged và không có lỗi DB
                        TempData["StatusMessage"] = "Thông tin cá nhân của bạn đã được cập nhật.";
                        _logger.LogInformation("ProfileController.Index (POST): UserProfile changes saved successfully for user {UserName}. TempData set.", userToUpdate.UserName);

                        var existingFullNameClaim = User.Claims.FirstOrDefault(c => c.Type == "FullName");
                        if (existingFullNameClaim != null)
                        {
                            var removeClaimResult = await _userManager.RemoveClaimAsync(userToUpdate, existingFullNameClaim);
                            if (removeClaimResult.Succeeded) _logger.LogInformation("Removed old FullName claim for {UserName}", userToUpdate.UserName);
                            else _logger.LogWarning("Failed to remove old FullName claim for {UserName}. Errors: {Errors}", userToUpdate.UserName, string.Join(", ", removeClaimResult.Errors.Select(e => e.Description)));
                        }
                        if (!string.IsNullOrEmpty(model.FullName))
                        {
                            var addClaimResult = await _userManager.AddClaimAsync(userToUpdate, new Claim("FullName", model.FullName));
                            if (addClaimResult.Succeeded) _logger.LogInformation("Added new FullName claim '{FullName}' for {UserName}", model.FullName, userToUpdate.UserName);
                            else _logger.LogWarning("Failed to add new FullName claim for {UserName}. Errors: {Errors}", userToUpdate.UserName, string.Join(", ", addClaimResult.Errors.Select(e => e.Description)));
                        }

                        await _signInManager.RefreshSignInAsync(userToUpdate);
                        _logger.LogInformation("ProfileController.Index (POST): SignIn cookie refreshed for user {UserName}.", userToUpdate.UserName);
                    }
                    else if (changes == 0 && profileDataChanged)
                    {
                        // Trường hợp này có thể xảy ra nếu EF Core không phát hiện thay đổi dù bạn nghĩ là có
                        TempData["StatusMessage"] = "Thông tin không thay đổi hoặc không thể lưu.";
                        _logger.LogWarning("ProfileController.Index (POST): SaveChangesAsync reported 0 changes for user {UserName}, though profileDataChanged was true. This might indicate an issue with EF Core change tracking or no actual change was made that EF Core detected.", userToUpdate.UserName);
                    }
                    else
                    { // changes == 0 && !profileDataChanged (không có gì để lưu)
                        TempData["StatusMessage"] = "Không có thông tin nào được thay đổi.";
                        _logger.LogInformation("ProfileController.Index (POST): No actual data changes to save for user {UserName}. TempData set.", userToUpdate.UserName);
                    }
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "ProfileController.Index (POST): DbUpdateException while saving profile for {UserName}.", userToUpdate.UserName);
                    ModelState.AddModelError(string.Empty, "Lỗi khi lưu thông tin. Vui lòng thử lại.");
                    model.PhoneNumber = userToUpdate.PhoneNumber;
                    return View(model);
                }
            }
            else
            { // profileDataChanged is false
                TempData["StatusMessage"] = "Không có thay đổi nào được thực hiện.";
                _logger.LogInformation("ProfileController.Index (POST): No changes detected for profile data. TempData set.", userToUpdate.UserName);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Profile/ChangePassword (Giữ nguyên)
        [HttpGet]
        public IActionResult ChangePassword()
        {
            var model = new ChangePasswordViewModel();
            // **THAY ĐỔI Ở ĐÂY: Sử dụng ViewData để truyền StatusMessage**
            if (TempData.ContainsKey("StatusMessage"))
            {
                ViewData["StatusMessage"] = TempData["StatusMessage"].ToString();
            }
            return View(model);
        }

        // POST: /Profile/ChangePassword (Giữ nguyên)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
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
                return View(model);
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                _logger.LogWarning("ProfileController.ChangePassword (POST): Failed to change password for user {UserName}. Errors: {Errors}", user.UserName, string.Join(", ", changePasswordResult.Errors.Select(e => e.Description)));
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("User {UserName} changed their password successfully.", user.UserName);
            TempData["StatusMessage"] = "Mật khẩu của bạn đã được thay đổi thành công.";

            return RedirectToAction(nameof(ChangePassword));
        }
    }
}
