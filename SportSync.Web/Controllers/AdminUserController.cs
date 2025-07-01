using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportSync.Business.Services;
using SportSync.Data.Enums;
using SportSync.Web.Models.ViewModels;
using SportSync.Web.Models.ViewModels.Admin;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SportSync.Data.Entities;

namespace SportSync.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUserController : Controller
    {
        private readonly UserManagementService _userManagementService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminUserController(UserManagementService userManagementService, UserManager<ApplicationUser> userManager)
        {
            _userManagementService = userManagementService;
            _userManager = userManager;
        }

        public async Task<IActionResult> UserManager(string searchTerm)
        {
            var customers = string.IsNullOrEmpty(searchTerm)
                ? await _userManagementService.GetCustomersAsync()
                : await _userManagementService.SearchCustomersAsync(searchTerm);

            var customerViewModels = new List<UserViewModel>();
            foreach (var customer in customers)
            {
                var roles = await _userManager.GetRolesAsync(customer);
                customerViewModels.Add(new UserViewModel
                {
                    UserId = customer.Id,
                    UserName = customer.UserName,
                    Email = customer.Email,
                    FullName = customer.UserProfile?.FullName ?? "Chưa có tên",
                    AccountStatus = customer.UserProfile != null ? ((AccountStatus)customer.UserProfile.AccountStatusByAdmin).ToString() : "Không xác định",
                    RegisteredDate = customer.UserProfile?.RegisteredDate ?? DateTime.MinValue,
                    LastLoginDate = customer.UserProfile?.LastLoginDate,
                    Role = roles.FirstOrDefault() ?? "Customer"
                });
            }

            return View("~/Views/Admin/UserManager.cshtml", customerViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string userId)
        {
            await _userManagementService.ToggleUserAccountStatusAsync(userId);
            return RedirectToAction("UserManager");
        }

        [HttpPost]
        public async Task<IActionResult> ChangeToCourtOwner(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction("UserManager");
            }

            // Kiểm tra xem user có phải là customer không
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains("Customer"))
            {
                TempData["Error"] = "Chỉ có thể thay đổi role của customer.";
                return RedirectToAction("UserManager");
            }

            // Xóa role Customer và thêm role CourtOwner
            await _userManager.RemoveFromRoleAsync(user, "Customer");
            await _userManager.AddToRoleAsync(user, "CourtOwner");

            TempData["Success"] = "Đã thay đổi role thành công từ Customer thành Court Owner.";
            return RedirectToAction("UserManager");
        }
    }
}