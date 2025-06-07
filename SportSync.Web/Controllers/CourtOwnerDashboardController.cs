using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SportSync.Business.Interfaces; // Thêm
using SportSync.Data.Entities;
using System.Threading.Tasks;

namespace SportSync.Web.Controllers
{
    [Authorize(Roles = "StandardCourtOwner,ProCourtOwner,CourtOwner,Admin")]
    public class CourtOwnerDashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CourtOwnerDashboardController> _logger;
        private readonly ICourtOwnerDashboardService _dashboardService; // Sử dụng service mới

        public CourtOwnerDashboardController(
            UserManager<ApplicationUser> userManager,
            ILogger<CourtOwnerDashboardController> logger,
            ICourtOwnerDashboardService dashboardService) // Inject service mới
        {
            _userManager = userManager;
            _logger = logger;
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            _logger.LogInformation("User {UserId} accessing CourtOwnerDashboard.", user.Id);

            var dashboardData = await _dashboardService.GetDashboardDataAsync(user.Id);

            if (!dashboardData.HasComplex)
            {
                _logger.LogInformation("User {UserId} has no court complex. Redirecting to Create page.", user.Id);
                TempData["InfoMessage"] = "Chào mừng đến với khu vực Chủ sân! Hãy bắt đầu bằng cách tạo khu phức hợp sân của bạn.";
                return RedirectToAction("Create", "CourtComplex");
            }

            return View(dashboardData);
        }
    }
}
