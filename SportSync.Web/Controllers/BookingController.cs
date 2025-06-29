using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SportSync.Business.Dtos;
using SportSync.Business.Interfaces;
using SportSync.Data.Entities;

namespace SportSync.Web.Controllers
{
    [Authorize]
    [Route("Bookings")]
    public class BookingController : Controller
    {
        private readonly IBookingService _svc;
        private readonly UserManager<ApplicationUser> _userMgr;
        private readonly ILogger<BookingController> _log;
        public BookingController(IBookingService svc, UserManager<ApplicationUser> userMgr, ILogger<BookingController> log)
        {
            _svc = svc;
            _userMgr = userMgr;
            _log = log;
        }
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        [Produces("application/json")]
        public async Task<IActionResult> Create([FromForm] CreateBookingRequestDto dto,
                                          CancellationToken ct = default)
        {
            var userId = _userMgr.GetUserId(User);
            if (userId == null) return Json(new { ok = false, message = "Bạn chưa đăng nhập!" });
            dto = dto with { BookerUserId = userId };

            try
            {
                var rs = await _svc.CreateBookingAsync(dto, userId, ct);

                return Json(new
                {
                    ok = true,
                    redirect = Url.Action("Success", "Bookings", new { id = rs.BookingId })
                });
            }
            catch (Exception ex) 
            {
                _log.LogWarning(ex, "Đặt sân lỗi");
                return Json(new { ok = false, message = ex.Message });
            }
        }

        [HttpGet("success/{id:long}")]
        public async Task<IActionResult> Success(long id, CancellationToken ct = default)
        {
            var userId = _userMgr.GetUserId(User)
                         ?? throw new InvalidOperationException("Không xác định người dùng.");

            var dto = await _svc.GetBookingDetailAsync(id, userId, ct);
            if (dto == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn đặt hoặc bạn không có quyền xem.";
                return RedirectToAction("Index", "Courts");
            }

            return View("Success", dto); 
        }
    }
}
