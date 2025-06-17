using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using SportSync.Business.Interfaces; // Cho ICourtService
using SportSync.Data.Entities;      // Cho ApplicationUser
using System.Linq;
using System.Threading.Tasks;
using SportSync.Business.Dtos;
using SportSync.Web.Models.ViewModels.Owner;
using SportSync.Web.Models.ViewModels.Court; // Cho CreateCourtDto

namespace SportSync.Web.Controllers
{
    [Authorize(Roles = "StandardCourtOwner,ProCourtOwner,Admin")] // Giới hạn quyền truy cập
    public class OwnerCourtController : Controller // ĐỔI TÊN CONTROLLER
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICourtService _courtService;
        private readonly ICourtComplexService _courtComplexService;
        private readonly ILogger<OwnerCourtController> _logger; // ĐỔI TÊN LOGGER

        public OwnerCourtController( // ĐỔI TÊN CONSTRUCTOR
            UserManager<ApplicationUser> userManager,
            ICourtService courtService,
            ICourtComplexService courtComplexService,
            ILogger<OwnerCourtController> logger) // ĐỔI TÊN LOGGER
        {
            _userManager = userManager;
            _courtService = courtService;
            _courtComplexService = courtComplexService;
            _logger = logger;
        }

        // GET: /OwnerCourt/Index?courtComplexId=1
        public async Task<IActionResult> Index(int courtComplexId)
        {
            _logger.LogInformation("OwnerCourtController: Attempting to view courts for CourtComplexId: {CourtComplexId}", courtComplexId);

            var complex = await _courtService.GetCourtComplexByIdAsync(courtComplexId);
            if (complex == null)
            {
                _logger.LogWarning("OwnerCourtController: CourtComplex with Id {CourtComplexId} not found.", courtComplexId);
                TempData["ErrorMessage"] = "Không tìm thấy khu phức hợp sân.";
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (complex.OwnerUserId != currentUser.Id && !User.IsInRole("Admin"))
            {
                _logger.LogWarning("OwnerCourtController: User {UserId} does not have permission for CourtComplexId {CourtComplexId}.", currentUser.Id, courtComplexId);
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập vào khu phức hợp này.";
            }

            ViewBag.CourtComplexName = complex.Name;
            ViewBag.CourtComplexId = courtComplexId;

            var courts = await _courtService.GetCourtsByComplexIdAsync(courtComplexId);
            return View(courts);
        }

        // GET: /OwnerCourt/Create?courtComplexId=1
        [HttpGet]
        public async Task<IActionResult> Create(int courtComplexId)
        {
            _logger.LogInformation("OwnerCourtController: GET Create court for CourtComplexId: {CourtComplexId}", courtComplexId);

            var complex = await _courtService.GetCourtComplexByIdAsync(courtComplexId);
            if (complex == null)
            {
                _logger.LogWarning("OwnerCourtController: Create court - CourtComplex with Id {CourtComplexId} not found.", courtComplexId);
                TempData["ErrorMessage"] = "Không tìm thấy khu phức hợp sân để thêm sân con.";
                return RedirectToAction("MyComplexes", "CourtComplex");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (complex.OwnerUserId != currentUser.Id && !User.IsInRole("Admin"))
            {
                _logger.LogWarning("OwnerCourtController: User {UserId} does not have permission to create court for CourtComplexId {CourtComplexId}.", currentUser.Id, courtComplexId);
                TempData["ErrorMessage"] = "Bạn không có quyền thêm sân vào khu phức hợp này.";
                return RedirectToAction("Index", "OwnerCourt", new { courtComplexId = courtComplexId }); // Sửa controller ở đây
            }

            var model = new OwnerCourtViewModel
            {
                CourtComplexId = courtComplexId,
                CourtComplexName = complex.Name,
                SportTypes = (await _courtService.GetAllSportTypesAsync())
                                .Select(st => new SelectListItem { Value = st.SportTypeId.ToString(), Text = st.Name }).ToList(),
                AvailableAmenities = (await _courtService.GetAllAmenitiesAsync())
                                .Select(a => new SelectListItem { Value = a.AmenityId.ToString(), Text = a.Name }).ToList()
            };
            return View(model);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int courtId, int courtComplexId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var (success, newStatus, errorMessage) = await _courtService.ToggleCourtStatusAsync(courtId, currentUser.Id);

            if (success)
            {
                TempData["SuccessMessage"] = $"Đã cập nhật trạng thái sân thành công: {newStatus}.";
            }
            else
            {
                TempData["ErrorMessage"] = errorMessage ?? "Không thể thay đổi trạng thái sân.";
            }

            return RedirectToAction(nameof(Index), new { courtComplexId = courtComplexId, _v = Guid.NewGuid().ToString() });

        }

    }
}
