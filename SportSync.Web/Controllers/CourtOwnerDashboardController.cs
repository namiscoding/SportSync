using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SportSync.Data.Entities; // Namespace của ApplicationUser
using System.Threading.Tasks;

namespace SportSync.Web.Controllers // Hoặc SportBookingWebsite.Web.Controllers
{
    [Authorize(Roles = "StandardCourtOwner,ProCourtOwner")] // Các vai trò được phép truy cập
    public class CourtOwnerDashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CourtOwnerDashboardController> _logger;

        public CourtOwnerDashboardController(
            UserManager<ApplicationUser> userManager,
            ILogger<CourtOwnerDashboardController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /CourtOwnerDashboard/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge(); // Không tìm thấy người dùng
            }

            _logger.LogInformation("User {UserId} accessed CourtOwnerDashboard.", user.Id);

            // Bạn có thể truyền thêm dữ liệu vào View nếu cần, ví dụ:
            // ViewBag.UserName = user.UserName; // Hoặc FullName từ UserProfile nếu đã load
            // var courtComplexes = await _courtComplexService.GetCourtComplexesByOwnerAsync(user.Id);
            // return View(courtComplexes); // Nếu muốn hiển thị danh sách sân ngay trên dashboard

            return View(); // Trả về View dashboard đơn giản
        }
    }
}
