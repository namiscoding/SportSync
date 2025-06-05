using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SportSync.Data.Entities;
using SportSync.Business.Interfaces; // Thêm using cho ICourtComplexService
using System.Linq; // Thêm using cho Any()
using System.Threading.Tasks;

namespace SportSync.Web.Controllers
{
    [Authorize(Roles = "StandardCourtOwner,ProCourtOwner,CourtOwner,Admin")]
    public class CourtOwnerDashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CourtOwnerDashboardController> _logger;
        private readonly ICourtComplexService _courtComplexService; // Inject service

        public CourtOwnerDashboardController(
            UserManager<ApplicationUser> userManager,
            ILogger<CourtOwnerDashboardController> logger,
            ICourtComplexService courtComplexService) // Thêm service vào constructor
        {
            _userManager = userManager;
            _logger = logger;
            _courtComplexService = courtComplexService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            _logger.LogInformation("User {UserId} accessed CourtOwnerDashboard.", user.Id);

            // Kiểm tra xem chủ sân đã có khu phức hợp nào chưa
            var courtComplexes = await _courtComplexService.GetCourtComplexesByOwnerAsync(user.Id);
            var existingComplex = courtComplexes.FirstOrDefault(); // Chủ sân chỉ có 1 complex

            ViewBag.HasCourtComplex = existingComplex != null;
            ViewBag.CourtComplexName = existingComplex?.Name; // Truyền tên complex nếu có

            return View();
        }
    }
}
