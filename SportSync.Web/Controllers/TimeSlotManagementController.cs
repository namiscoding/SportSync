using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SportSync.Business.Dtos;
using SportSync.Business.Interfaces;
using SportSync.Data.Entities;
using System.Threading.Tasks;

namespace SportSync.Web.Controllers
{
    [Authorize(Roles = "StandardCourtOwner,ProCourtOwner")]
    public class TimeSlotManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITimeSlotManagementService _timeSlotService;
        private readonly ICourtService _courtService; // **THÊM ICourtService**
        private readonly ILogger<TimeSlotManagementController> _logger;

        public TimeSlotManagementController(
            UserManager<ApplicationUser> userManager,
            ITimeSlotManagementService timeSlotService,
            ICourtService courtService, // **INJECT ICourtService**
            ILogger<TimeSlotManagementController> logger)
        {
            _userManager = userManager;
            _timeSlotService = timeSlotService;
            _courtService = courtService; // **GÁN SERVICE**
            _logger = logger;
        }

        // GET: /TimeSlotManagement/Index?courtId=5
        public async Task<IActionResult> Index(int courtId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            // **SỬA LẠI LOGIC LẤY THÔNG TIN**
            // 1. Lấy thông tin sân và kiểm tra quyền sở hữu
            var court = await _courtService.GetCourtForEditAsync(courtId, currentUser.Id); // Tái sử dụng hàm này vì nó đã include CourtComplex

            if (court == null)
            {
                _logger.LogWarning("User {UserId} attempted to access timeslot management for court {CourtId} but was denied or court not found.", currentUser.Id, courtId);
                TempData["ErrorMessage"] = "Không tìm thấy sân hoặc bạn không có quyền truy cập.";
                return RedirectToAction("Index", "CourtOwnerDashboard");
            }

            // 2. Lấy dữ liệu lịch trình cho sân đó
            var templateDto = await _timeSlotService.GetTimeSlotTemplateForCourtAsync(courtId, currentUser.Id);
            if (templateDto == null)
            {
                // Trường hợp này hiếm xảy ra nếu bước trên đã thành công, nhưng vẫn kiểm tra cho an toàn
                TempData["ErrorMessage"] = "Không thể tải dữ liệu lịch trình cho sân.";
                return RedirectToAction("Index", "OwnerCourt", new { courtComplexId = court.CourtComplexId });
            }

            // 3. Gán ViewBag với thông tin đã lấy được
            ViewBag.CourtComplexId = court.CourtComplexId;
            ViewBag.CourtComplexName = court.CourtComplex.Name;
            ViewBag.CourtName = court.Name; // Thêm tên sân vào ViewBag để dùng trong View

            return View(templateDto);
        }

        // POST: /TimeSlotManagement/UpdateBulk
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBulk([FromBody] BulkTimeSlotUpdateDto updateData)
        {
            if (updateData == null || updateData.Changes == null)
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            _logger.LogInformation("User {UserId} is submitting {ChangeCount} timeslot changes for CourtId {CourtId}",
                currentUser.Id, updateData.Changes.Count, updateData.CourtId);

            var (success, errorMessage) = await _timeSlotService.UpdateBulkTimeSlotsAsync(updateData, currentUser.Id);

            if (success)
            {
                TempData["SuccessMessage"] = "Đã lưu các thay đổi thành công!";
                return Ok(new
                {
                    success = true,
                    message = "Đã lưu các thay đổi thành công!",
                    redirectUrl = Url.Action("Index", new { courtId = updateData.CourtId, _v = Guid.NewGuid().ToString() })
                });
            }
            else
            {
                return BadRequest(new { success = false, message = errorMessage ?? "Đã xảy ra lỗi không xác định." });
            }
        }
    }
}
