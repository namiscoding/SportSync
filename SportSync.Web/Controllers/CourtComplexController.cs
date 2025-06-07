using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SportSync.Data.Entities;
using SportSync.Web.Models.ViewModels.CourtComplex;
using System.Linq;
using System.Threading.Tasks;
using SportSync.Business.Interfaces;
using SportSync.Business.Dtos;
using System.IO;

namespace SportSync.Web.Controllers
{
    public class CourtComplexController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICourtComplexService _courtComplexService;
        private readonly ILogger<CourtComplexController> _logger;

        public CourtComplexController(
            UserManager<ApplicationUser> userManager,
            ICourtComplexService courtComplexService,
            ILogger<CourtComplexController> logger)
        {
            _userManager = userManager;
            _courtComplexService = courtComplexService;
            _logger = logger;
        }

        [Authorize(Roles = "StandardCourtOwner,ProCourtOwner")]
        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var complexes = await _courtComplexService.GetCourtComplexesByOwnerAsync(currentUser.Id);
            var userComplex = complexes.FirstOrDefault();

            if (userComplex == null)
            {
                _logger.LogInformation("User {UserId} has no court complex. Redirecting to Create page.", currentUser.Id);
                TempData["InfoMessage"] = "Bạn chưa có khu phức hợp sân. Hãy bắt đầu bằng cách tạo một khu phức hợp mới.";
                return RedirectToAction(nameof(Create));
            }

            _logger.LogInformation("Displaying management page for CourtComplex {ComplexId} for user {UserId}", userComplex.CourtComplexId, currentUser.Id);

            var model = new CourtComplexViewModel
            {
                CourtComplexId = userComplex.CourtComplexId,
                Name = userComplex.Name,
                Address = userComplex.Address,
                City = userComplex.City,
                District = userComplex.District,
                Ward = userComplex.Ward,
                Description = userComplex.Description,
                ContactPhoneNumber = userComplex.ContactPhoneNumber,
                ContactEmail = userComplex.ContactEmail,
                DefaultOpeningTime = userComplex.DefaultOpeningTime,
                DefaultClosingTime = userComplex.DefaultClosingTime,
                Latitude = userComplex.Latitude,
                Longitude = userComplex.Longitude,
                MainImageCloudinaryUrl = userComplex.MainImageCloudinaryUrl
            };

            return View(model);
        }

        // POST: /CourtComplex/Manage
        // Xử lý việc cập nhật thông tin
        [Authorize(Roles = "StandardCourtOwner,ProCourtOwner")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(CourtComplexViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Manage [POST]: ModelState is invalid for user {UserId}", currentUser.Id);
                return View(model); // Quay lại view với lỗi validation
            }

            var updateDto = new UpdateCourtComplexDto
            {
                Id = model.CourtComplexId,
                Name = model.Name,
                Address = model.Address,
                City = model.City,
                District = model.District,
                Ward = model.Ward,
                Description = model.Description,
                ContactPhoneNumber = model.ContactPhoneNumber,
                ContactEmail = model.ContactEmail,
                DefaultOpeningTime = model.DefaultOpeningTime,
                DefaultClosingTime = model.DefaultClosingTime,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                NewMainImageFile = model.MainImageFile
            };

            var (success, errors) = await _courtComplexService.UpdateCourtComplexAsync(updateDto, currentUser.Id);

            if (success)
            {
                TempData["SuccessMessage"] = $"Thông tin khu phức hợp '{model.Name}' đã được cập nhật thành công.";
                if (errors != null && errors.Any())
                {
                    TempData["WarningMessage"] = string.Join(" ", errors);
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật.";
                if (errors != null)
                {
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }
                return View(model); // Quay lại view với lỗi
            }

            return RedirectToAction(nameof(Manage));
        }

        [Authorize] // Bất kỳ ai đăng nhập đều có thể thử vào trang Create
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            // Kiểm tra xem người dùng này (nếu là chủ sân) đã có khu phức hợp nào chưa
            // Điều này quan trọng nếu họ đã là chủ sân từ trước và cố gắng tạo thêm.
            // Đối với người dùng mới đăng ký gói, họ sẽ chưa có khu phức hợp nào.
            bool isCourtOwnerType = User.IsInRole("CourtOwner") || User.IsInRole("StandardCourtOwner") || User.IsInRole("ProCourtOwner");
            if (isCourtOwnerType)
            {
                var existingComplexes = await _courtComplexService.GetCourtComplexesByOwnerAsync(currentUser.Id);
                if (existingComplexes.Any())
                {
                    _logger.LogInformation("User {UserId} is a court owner and already has a complex. Redirecting to MyComplexes.", currentUser.Id);
                    TempData["InfoMessage"] = "Bạn đã có một khu phức hợp sân. Bạn chỉ có thể quản lý một khu phức hợp.";
                    return RedirectToAction(nameof(Manage));
                }
            }
            // Nếu không phải chủ sân, hoặc là chủ sân nhưng chưa có complex, cho phép vào trang Create.
            // Việc người dùng có được phép tạo không sẽ được kiểm tra lại ở service khi POST.
            return View(new CourtComplexViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(CourtComplexViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            bool isCourtOwnerType = User.IsInRole("CourtOwner") || User.IsInRole("StandardCourtOwner") || User.IsInRole("ProCourtOwner");
            if (isCourtOwnerType)
            {
                var existingComplexes = await _courtComplexService.GetCourtComplexesByOwnerAsync(currentUser.Id);
                if (existingComplexes.Any())
                {
                    ModelState.AddModelError(string.Empty, "Bạn chỉ được phép quản lý một khu phức hợp sân.");
                    return View(model);
                }
            }

            if (ModelState.IsValid)
            {
                _logger.LogInformation("Create [POST]: ModelState is valid for creating CourtComplex by User {UserId} ({UserName})", currentUser.Id, currentUser.UserName);

                var createDto = new CreateCourtComplexDto
                {
                    Name = model.Name,
                    Address = model.Address,
                    City = model.City,
                    District = model.District,
                    Ward = model.Ward,
                    Description = model.Description,
                    ContactPhoneNumber = model.ContactPhoneNumber,
                    ContactEmail = model.ContactEmail,
                    DefaultOpeningTime = model.DefaultOpeningTime,
                    DefaultClosingTime = model.DefaultClosingTime,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude,
                    MainImageFile = model.MainImageFile
                };

                var (success, createdComplex, errors) = await _courtComplexService.CreateCourtComplexAsync(createDto, currentUser.Id);

                if (success)
                {
                    _logger.LogInformation("CourtComplex '{ComplexName}' (ID: {ComplexId}) created by User {UserId}. Redirecting.", createdComplex.Name, createdComplex.CourtComplexId, currentUser.Id);
                    TempData["SuccessMessage"] = $"Khu phức hợp sân '{createdComplex.Name}' đã được tạo. Vui lòng thêm ít nhất một sân con.";
                    return RedirectToAction("Index", "Court", new { courtComplexId = createdComplex.CourtComplexId });
                }
                else
                {
                    _logger.LogWarning("Create [POST]: Failed to create CourtComplex. Errors: {Errors}", errors != null ? string.Join("; ", errors) : "Unknown");
                    if (errors != null)
                    {
                        foreach (var error in errors) { ModelState.AddModelError(string.Empty, error); }
                    }
                    else { ModelState.AddModelError(string.Empty, "Lỗi tạo khu phức hợp."); }
                }
            }
            else
            {
                _logger.LogWarning("Create [POST]: ModelState is invalid. Errors: {Errors}",
                                   string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            return View(model);
        }

      

        [HttpGet("/CourtComplex/Details/{id:int}")]
        public async Task<IActionResult> Details(int id, DateOnly? date)
        {
            var dto = await _courtComplexService.GetDetailAsync(id, date);
            if (dto == null) return NotFound();

            ViewBag.SelectedDate = date ?? DateOnly.FromDateTime(DateTime.Today);
            return View(dto);    // /Views/CourtComplex/Details.cshtml
        }
    }
}
