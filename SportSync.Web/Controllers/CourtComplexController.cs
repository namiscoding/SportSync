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
    // [Authorize(Roles = "CourtOwner,Admin")] 
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

        [Authorize(Roles = "CourtOwner,Admin")]
        public async Task<IActionResult> MyComplexes()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                _logger.LogWarning("MyComplexes: Current user not found.");
                return Challenge();
            }

            _logger.LogInformation("Fetching court complexes for owner {OwnerId}", currentUser.Id);
            var courtComplexes = await _courtComplexService.GetCourtComplexesByOwnerAsync(currentUser.Id);

            _logger.LogInformation("Fetched {Count} court complexes for user {UserId}", courtComplexes.Count(), currentUser.Id);
            return View(courtComplexes);
        }

        [Authorize]
        public IActionResult Create()
        {
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
                _logger.LogWarning("Create [POST]: Current user not found despite [Authorize] attribute.");
                return Challenge();
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
                    DefaultClosingTime = model.DefaultClosingTime
                };

                if (model.MainImageFile != null && model.MainImageFile.Length > 0)
                {
                    // **THAY ĐỔI Ở ĐÂY: Bỏ 'using' cho imageStream**
                    // Stream này sẽ được dispose bởi CloudinaryImageUploadService
                    var imageStream = new MemoryStream();
                    await model.MainImageFile.CopyToAsync(imageStream);
                    imageStream.Position = 0;

                    createDto.MainImage = new ImageInputDto
                    {
                        Content = imageStream,
                        FileName = model.MainImageFile.FileName,
                        ContentType = model.MainImageFile.ContentType
                    };
                }

                var (success, createdComplex, errors) = await _courtComplexService.CreateCourtComplexAsync(createDto, currentUser.Id);

                if (success)
                {
                    _logger.LogInformation("CourtComplex '{ComplexName}' (ID: {ComplexId}) created successfully by User {UserId}. Redirecting to add courts.", createdComplex.Name, createdComplex.CourtComplexId, currentUser.Id);
                    TempData["SuccessMessage"] = $"Khu phức hợp sân '{createdComplex.Name}' đã được tạo. Vui lòng thêm ít nhất một sân con.";
                    return RedirectToAction("Index", "Court", new { courtComplexId = createdComplex.CourtComplexId });
                }
                else
                {
                    _logger.LogWarning("Create [POST]: Failed to create CourtComplex by User {UserId}. Errors: {Errors}", currentUser.Id, errors != null ? string.Join("; ", errors) : "Unknown error from service.");
                    if (errors != null)
                    {
                        foreach (var error in errors)
                        {
                            ModelState.AddModelError(string.Empty, error);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi không mong muốn khi tạo khu phức hợp.");
                    }
                    if (errors != null && errors.Any(e => e.ToLower().Contains("ảnh")))
                    {
                        ModelState.AddModelError("MainImageFile", errors.First(e => e.ToLower().Contains("ảnh")));
                    }
                }
            }
            else
            {
                _logger.LogWarning("Create [POST]: ModelState is invalid for creating CourtComplex by User {UserId}. Errors: {Errors}",
                                   currentUser.Id,
                                   string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            return View(model);
        }
    }
}
