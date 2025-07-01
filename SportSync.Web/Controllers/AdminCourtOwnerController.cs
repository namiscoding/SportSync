using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportSync.Business.Services;
using SportSync.Web.Models.ViewModels;
using SportSync.Web.Models.ViewModels.Admin;
using System.Linq;
using System.Threading.Tasks;
using SportSync.Data.Enums;
using Microsoft.AspNetCore.Identity;
using SportSync.Data.Entities;

namespace SportSync.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminCourtOwnerController : Controller
    {
        private readonly CourtOwnerManagementService _courtOwnerManagementService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminCourtOwnerController(CourtOwnerManagementService courtOwnerManagementService, UserManager<ApplicationUser> userManager)
        {
            _courtOwnerManagementService = courtOwnerManagementService;
            _userManager = userManager;
        }

        public async Task<IActionResult> OwnerManager(string searchTerm, string selectedRole)
        {
            var courtOwners = string.IsNullOrEmpty(searchTerm) && string.IsNullOrEmpty(selectedRole)
                ? await _courtOwnerManagementService.GetCourtOwnersAsync()
                : await _courtOwnerManagementService.SearchCourtOwnersAsync(searchTerm, selectedRole);

            var courtOwnerViewModels = new List<CourtOwnerViewModel>();
            foreach (var co in courtOwners)
            {
                var roles = await _userManager.GetRolesAsync(co);
                courtOwnerViewModels.Add(new CourtOwnerViewModel
                {
                    UserId = co.Id,
                    UserName = co.UserName,
                    Email = co.Email,
                    FullName = co.UserProfile?.FullName ?? "Chưa có tên",
                    AccountStatus = co.UserProfile != null ? ((AccountStatus)co.UserProfile.AccountStatusByAdmin).ToString() : "Không xác định",
                    RegisteredDate = co.UserProfile?.RegisteredDate ?? DateTime.MinValue,
                    LastLoginDate = co.UserProfile?.LastLoginDate,
                    Role = roles.FirstOrDefault() ?? "Không xác định"
                });
            }

            // Truyền danh sách vai trò cho dropdown
            ViewBag.Roles = new[] { "All", "CourtOwner" };
            ViewBag.SelectedRole = selectedRole;

            return View("~/Views/Admin/OwnerManager.cshtml", courtOwnerViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(string userId)
        {
            string adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
            {
                return Unauthorized();
            }
            await _courtOwnerManagementService.ApproveCourtOwnerAsync(userId, adminId);
            return RedirectToAction("OwnerManager");
        }

        [HttpPost]
        public async Task<IActionResult> Reject(string userId, string rejectionReason)
        {
            string adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
            {
                return Unauthorized();
            }
            await _courtOwnerManagementService.RejectCourtOwnerAsync(userId, adminId, rejectionReason);
            return RedirectToAction("OwnerManager");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string userId)
        {
            await _courtOwnerManagementService.ToggleCourtOwnerAccountStatusAsync(userId);
            return RedirectToAction("OwnerManager");
        }
    }
}