using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SportSync.Business.Dtos;
using SportSync.Business.Interfaces;
using SportSync.Data.Entities;

namespace SportSync.Web.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly IBookingService _svc;
        private readonly UserManager<ApplicationUser> _userMgr;

        public BookingController(IBookingService svc,
                                 UserManager<ApplicationUser> userMgr)
        {
            _svc = svc;
            _userMgr = userMgr;
        }

        [HttpPost]
        public async Task<IActionResult> Create(int courtId,
                                                DateOnly date,
                                                List<int> selectedSlotIds,
                                                CancellationToken ct)
        {
            if (selectedSlotIds.Count == 0)
            {
                TempData["Error"] = "Bạn chưa chọn slot nào.";
                return RedirectToAction("Details", "Court",
                                        new { id = courtId, date });
            }

            var dto = new CreateBookingDto
            {
                CourtId = courtId,
                Date = date,
                SlotIds = selectedSlotIds,
                BookerUserId = _userMgr.GetUserId(User)!
            };

            var (ok, err, bookingId) = await _svc.CreateAsync(dto, ct);

            if (!ok)
            {
                TempData["Error"] = err ?? "Đặt sân thất bại.";
                return RedirectToAction("Details", "Court",
                                        new { id = courtId, date });
            }

            // thành công
            TempData["Success"] = "Đặt sân thành công!";
            return RedirectToAction(nameof(Success), new { id = bookingId });
        }

        // GET  /Booking/Success/{id}
        public async Task<IActionResult> Success(long id)
        {
            var invoice = await _svc.GetInvoiceAsync(id);   // DTO gồm Complex, Court, slot, total…
            if (invoice == null) return NotFound();
            return View(invoice);                           // /Views/Booking/Success.cshtml
        }
    }
}
