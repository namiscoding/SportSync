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
