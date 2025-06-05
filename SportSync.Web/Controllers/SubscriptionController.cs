using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SportSync.Data.Entities; // Namespace của ApplicationUser
using System.Threading.Tasks;

namespace SportSync.Web.Controllers // Hoặc SportBookingWebsite.Web.Controllers
{
    [Authorize] // Yêu cầu đăng nhập để truy cập trang đăng ký gói
    public class SubscriptionController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager; // Cần để gán vai trò
        private readonly ILogger<SubscriptionController> _logger;

        public SubscriptionController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<SubscriptionController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // GET: /Subscription/Index
        // Hiển thị trang chọn gói dịch vụ
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge(); // Không tìm thấy người dùng
            }

            // Kiểm tra xem người dùng đã là chủ sân chưa, nếu rồi thì có thể chuyển hướng
            if (await _userManager.IsInRoleAsync(user, "StandardCourtOwner") ||
                await _userManager.IsInRoleAsync(user, "ProCourtOwner")) 
            {
                _logger.LogInformation("User {UserId} is already a court owner. Redirecting to CourtOwnerDashboard.", user.Id);
                // Nếu đã là chủ sân, có thể chuyển đến dashboard của họ
                return RedirectToAction("Index", "CourtOwnerDashboard");
            }

            return View(); // Trả về View hiển thị các gói
        }

        // POST: /Subscription/Subscribe
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Subscribe(string packageType)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            if (string.IsNullOrEmpty(packageType))
            {
                TempData["ErrorMessage"] = "Vui lòng chọn một gói dịch vụ.";
                return RedirectToAction(nameof(Index));
            }

            string roleToAssign = null;
            string successMessage = null;

            if (packageType.Equals("Standard", System.StringComparison.OrdinalIgnoreCase))
            {
                roleToAssign = "StandardCourtOwner";
                successMessage = "Bạn đã đăng ký thành công gói Standard! Bây giờ bạn có thể bắt đầu quản lý sân của mình.";
                _logger.LogInformation("User {UserId} subscribing to Standard package.", user.Id);
            }
            else if (packageType.Equals("Pro", System.StringComparison.OrdinalIgnoreCase))
            {
                roleToAssign = "ProCourtOwner";
                successMessage = "Bạn đã đăng ký thành công gói Pro!";
                _logger.LogInformation("User {UserId} subscribing to Pro package.");
                // Tạm thời gói Pro cũng sẽ thành công ngay
            }
            else
            {
                TempData["ErrorMessage"] = "Gói dịch vụ không hợp lệ.";
                _logger.LogWarning("User {UserId} attempted to subscribe to invalid package type: {PackageType}", user.Id, packageType);
                return RedirectToAction(nameof(Index));
            }

            // Tạm thời: Auto-success, không có thanh toán
            // Trong thực tế, đây sẽ là nơi tích hợp cổng thanh toán.

            // Kiểm tra xem vai trò đã tồn tại chưa, nếu chưa thì tạo
            if (!await _roleManager.RoleExistsAsync(roleToAssign))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleToAssign));
                if (!roleResult.Succeeded)
                {
                    _logger.LogError("Failed to create role {RoleName}. Errors: {Errors}", roleToAssign, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    TempData["ErrorMessage"] = "Đã xảy ra lỗi trong quá trình đăng ký gói. Vui lòng thử lại.";
                    return RedirectToAction(nameof(Index));
                }
                _logger.LogInformation("Role {RoleName} created.", roleToAssign);
            }

            // Gán vai trò cho người dùng
            if (!await _userManager.IsInRoleAsync(user, roleToAssign))
            {
                var addToRoleResult = await _userManager.AddToRoleAsync(user, roleToAssign);
                if (!addToRoleResult.Succeeded)
                {
                    _logger.LogError("Failed to add user {UserId} to role {RoleName}. Errors: {Errors}", user.Id, roleToAssign, string.Join(", ", addToRoleResult.Errors.Select(e => e.Description)));
                    TempData["ErrorMessage"] = "Không thể gán quyền chủ sân. Vui lòng liên hệ quản trị viên.";
                    return RedirectToAction(nameof(Index));
                }
                _logger.LogInformation("User {UserId} successfully added to role {RoleName}.", user.Id, roleToAssign);
            }
            else
            {
                _logger.LogInformation("User {UserId} already in role {RoleName}.", user.Id, roleToAssign);
            }

            TempData["SuccessMessage"] = successMessage;
            // Chuyển hướng đến dashboard của chủ sân
            return RedirectToAction("Index", "CourtOwnerDashboard");
        }
    }
}
