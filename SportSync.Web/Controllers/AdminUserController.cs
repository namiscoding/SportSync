using Microsoft.AspNetCore.Authorization; // Thêm namespace
using Microsoft.AspNetCore.Mvc;
using SportSync.Business.Services;
using SportSync.Web.Models.ViewModels;
using SportSync.Web.Models.ViewModels.Admin;
using System.Linq;
using System.Threading.Tasks;

namespace SportSync.Web.Controllers
{
    [Authorize(Roles = "Admin")] // Chỉ Admin mới truy cập được
    public class AdminUserController : Controller
    {
        private readonly UserManagementService _userManagementService;

        public AdminUserController(UserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        public async Task<IActionResult> UserManager(string searchTerm)
        {
            var users = string.IsNullOrEmpty(searchTerm)
                ? await _userManagementService.GetUsersAsync()
                : await _userManagementService.SearchUsersAsync(searchTerm);

            var userViewModels = users.Select(u => new UserViewModel
            {
                UserId = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                FullName = u.UserProfile?.FullName,
                AccountStatus = u.UserProfile?.AccountStatusByAdmin.ToString(),
                RegisteredDate = u.UserProfile?.RegisteredDate ?? DateTime.MinValue,
                LastLoginDate = u.UserProfile?.LastLoginDate
            }).ToList();

            return View("~/Views/Admin/UserManager.cshtml", userViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string userId)
        {
            await _userManagementService.ToggleUserAccountStatusAsync(userId);
            return RedirectToAction("UserManager");
        }
    }
}