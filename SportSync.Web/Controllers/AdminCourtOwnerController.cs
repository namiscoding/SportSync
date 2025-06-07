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
    public class AdminCourtOwnerController : Controller
    {
        private readonly CourtOwnerManagementService _courtOwnerManagementService;

        public AdminCourtOwnerController(CourtOwnerManagementService courtOwnerManagementService)
        {
            _courtOwnerManagementService = courtOwnerManagementService;
        }

        public async Task<IActionResult> OwnerManager(string searchTerm)
        {
            var courtOwners = string.IsNullOrEmpty(searchTerm)
                ? await _courtOwnerManagementService.GetCourtOwnersAsync()
                : await _courtOwnerManagementService.SearchCourtOwnersAsync(searchTerm);

            var courtOwnerViewModels = courtOwners.Select(co => new CourtOwnerViewModel
            {
                UserId = co.Id,
                UserName = co.UserName,
                Email = co.Email,
                FullName = co.UserProfile?.FullName,
                AccountStatus = co.UserProfile?.AccountStatusByAdmin.ToString(),
                RegisteredDate = co.UserProfile?.RegisteredDate ?? DateTime.MinValue,
                LastLoginDate = co.UserProfile?.LastLoginDate
            }).ToList();

            return View("~/Views/Admin/OwnerManager.cshtml", courtOwnerViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(string userId)
        {
            string adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
            {
                return Unauthorized(); // Nếu không lấy được adminId
            }
            await _courtOwnerManagementService.ApproveCourtOwnerAsync(userId, adminId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Reject(string userId, string rejectionReason)
        {
            string adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
            {
                return Unauthorized(); // Nếu không lấy được adminId
            }
            await _courtOwnerManagementService.RejectCourtOwnerAsync(userId, adminId, rejectionReason);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string userId)
        {
            await _courtOwnerManagementService.ToggleCourtOwnerAccountStatusAsync(userId);
            return RedirectToAction("Index");
        }
    }
}