using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SportSync.Business.Dtos;
using SportSync.Business.Interfaces;
using SportSync.Data.Entities;
using SportSync.Data.Enums;
using System;
using System.Threading.Tasks;

namespace SportSync.Web.Controllers
{
    [Authorize(Roles = "StandardCourtOwner,ProCourtOwner")]
    public class BookingManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBookingManagementService _bookingService;
        private readonly ILogger<BookingManagementController> _logger;

        public BookingManagementController(
            UserManager<ApplicationUser> userManager,
            IBookingManagementService bookingService,
            ILogger<BookingManagementController> logger)
        {
            _userManager = userManager;
            _bookingService = bookingService;
            _logger = logger;
        }

        // GET: /BookingManagement/Index?courtId=5&date=2025-06-09
        public async Task<IActionResult> Schedule(int courtId, DateTime? date)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            // Nếu không có ngày nào được cung cấp, sử dụng ngày hiện tại
            var dateInWeek = date ?? DateTime.Today;

            var weeklySchedule = await _bookingService.GetWeeklyScheduleAsync(courtId, dateInWeek, currentUser.Id);

            if (weeklySchedule == null)
            {
                _logger.LogWarning("User {UserId} attempted to access booking management for court {CourtId} but was denied or court not found.", currentUser.Id, courtId);
                TempData["ErrorMessage"] = "Không tìm thấy sân hoặc bạn không có quyền truy cập.";
                return RedirectToAction("Index", "CourtOwnerDashboard");
            }

            return View(weeklySchedule);
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var bookings = await _bookingService.GetBookingsForOwnerAsync(currentUser.Id);
            return View(bookings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(long bookingId, BookingStatusType newStatus)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            _logger.LogInformation("User {UserId} attempting to update booking {BookingId} to status {NewStatus}", currentUser.Id, bookingId, newStatus);

            var (success, errorMessage) = await _bookingService.UpdateBookingStatusAsync(bookingId, newStatus, currentUser.Id);

            if (success)
            {
                TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn hàng #{bookingId} thành công.";

                // **THAY ĐỔI Ở ĐÂY: Nếu trạng thái mới là Completed, chuyển đến trang hóa đơn**
                if (newStatus == BookingStatusType.Completed)
                {
                    return RedirectToAction(nameof(CompleteBill), new { bookingId = bookingId });
                }
            }
            else
            {
                TempData["ErrorMessage"] = errorMessage ?? "Không thể cập nhật trạng thái đơn hàng.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> CompleteBill(long bookingId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var billDetails = await _bookingService.GetBookingDetailsForBillAsync(bookingId, currentUser.Id);
            if (billDetails == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền truy cập.";
                return RedirectToAction(nameof(Index));
            }

            return View(billDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBulkManualBooking([FromBody] CreateBulkManualBookingDto dto)
        {
            if (!ModelState.IsValid)
            {
                // Tổng hợp các lỗi validation và trả về
                var errorMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { success = false, message = string.Join(" ", errorMessages) });
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var (success, errorMessage) = await _bookingService.CreateBulkManualBookingAsync(dto, currentUser.Id);

            if (success)
            {
                return Ok(new { success = true, message = "Đặt chỗ cho khách vãng lai thành công!" });
            }
            else
            {
                return BadRequest(new { success = false, message = errorMessage ?? "Không thể tạo lượt đặt chỗ." });
            }
        }
    }
}
