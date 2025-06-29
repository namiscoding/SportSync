//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.Extensions.Logging;
//using SportSync.Business.Interfaces; // Cho ICourtService
//using SportSync.Data.Entities;      // Cho ApplicationUser
//using System.Linq;
//using System.Threading.Tasks;
//using SportSync.Business.Dtos;
//using SportSync.Web.Models.ViewModels.Owner;
//using SportSync.Web.Models.ViewModels.Court; // Cho CreateCourtDto

//namespace SportSync.Web.Controllers
//{
//    [Authorize(Roles = "StandardCourtOwner,ProCourtOwner,Admin")] // Giới hạn quyền truy cập
//    public class OwnerCourtController : Controller // ĐỔI TÊN CONTROLLER
//    {
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly ICourtService _courtService;
//        private readonly ICourtComplexService _courtComplexService;
//        private readonly ILogger<OwnerCourtController> _logger; // ĐỔI TÊN LOGGER

//        public OwnerCourtController( // ĐỔI TÊN CONSTRUCTOR
//            UserManager<ApplicationUser> userManager,
//            ICourtService courtService,
//            ICourtComplexService courtComplexService,
//            ILogger<OwnerCourtController> logger) // ĐỔI TÊN LOGGER
//        {
//            _userManager = userManager;
//            _courtService = courtService;
//            _courtComplexService = courtComplexService;
//            _logger = logger;
//        }

//        // GET: /OwnerCourt/Index?courtComplexId=1
//        public async Task<IActionResult> Index(int courtComplexId)
//        {
//            _logger.LogInformation("OwnerCourtController: Attempting to view courts for CourtComplexId: {CourtComplexId}", courtComplexId);

//            var complex = await _courtService.GetCourtComplexByIdAsync(courtComplexId);
//            if (complex == null)
//            {
//                _logger.LogWarning("OwnerCourtController: CourtComplex with Id {CourtComplexId} not found.", courtComplexId);
//                TempData["ErrorMessage"] = "Không tìm thấy khu phức hợp sân.";
//            }

//            var currentUser = await _userManager.GetUserAsync(User);
//            if (complex.OwnerUserId != currentUser.Id && !User.IsInRole("Admin"))
//            {
//                _logger.LogWarning("OwnerCourtController: User {UserId} does not have permission for CourtComplexId {CourtComplexId}.", currentUser.Id, courtComplexId);
//                TempData["ErrorMessage"] = "Bạn không có quyền truy cập vào khu phức hợp này.";
//            }   

//            ViewBag.CourtComplexName = complex.Name;
//            ViewBag.CourtComplexId = courtComplexId;

//            var courts = await _courtService.GetCourtsByComplexIdAsync(courtComplexId);
//            return View(courts);
//        }

//        // GET: /OwnerCourt/Create?courtComplexId=1
//        [HttpGet]
//        public async Task<IActionResult> Create(int courtComplexId)
//        {
//            _logger.LogInformation("OwnerCourtController: GET Create court for CourtComplexId: {CourtComplexId}", courtComplexId);

//            var complex = await _courtService.GetCourtComplexByIdAsync(courtComplexId);
//            if (complex == null)
//            {
//                _logger.LogWarning("OwnerCourtController: Create court - CourtComplex with Id {CourtComplexId} not found.", courtComplexId);
//                TempData["ErrorMessage"] = "Không tìm thấy khu phức hợp sân để thêm sân con.";
//                return RedirectToAction("MyComplexes", "CourtComplex");
//            }

//            var currentUser = await _userManager.GetUserAsync(User);
//            if (complex.OwnerUserId != currentUser.Id && !User.IsInRole("Admin"))
//            {
//                _logger.LogWarning("OwnerCourtController: User {UserId} does not have permission to create court for CourtComplexId {CourtComplexId}.", currentUser.Id, courtComplexId);
//                TempData["ErrorMessage"] = "Bạn không có quyền thêm sân vào khu phức hợp này.";
//                return RedirectToAction("Index", "OwnerCourt", new { courtComplexId = courtComplexId }); // Sửa controller ở đây
//            }

//            var model = new OwnerCourtViewModel
//            {
//                CourtComplexId = courtComplexId,
//                CourtComplexName = complex.Name,
//                SportTypes = (await _courtService.GetAllSportTypesAsync())
//                                .Select(st => new SelectListItem { Value = st.SportTypeId.ToString(), Text = st.Name }).ToList(),
//                AvailableAmenities = (await _courtService.GetAllAmenitiesAsync())
//                                .Select(a => new SelectListItem { Value = a.AmenityId.ToString(), Text = a.Name }).ToList()
//            };
//            return View(model);
//        }

//        // POST: /OwnerCourt/Create
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(OwnerCourtViewModel model)
//        {
//            _logger.LogInformation("OwnerCourtController: POST Create court for CourtComplexId: {CourtComplexId}, Name: {CourtName}", model.CourtComplexId, model.Name);

//            var complex = await _courtService.GetCourtComplexByIdAsync(model.CourtComplexId);
//            var currentUser = await _userManager.GetUserAsync(User);

//            if (complex == null || (complex.OwnerUserId != currentUser.Id && !User.IsInRole("Admin")))
//            {
//                _logger.LogWarning("OwnerCourtController: POST Create court - User {UserId} permission denied or complex {ComplexId} not found.", currentUser?.Id, model.CourtComplexId);
//                ModelState.AddModelError(string.Empty, "Không tìm thấy khu phức hợp hoặc bạn không có quyền thực hiện hành động này.");
//                await PrepareViewModelForCreateView(model, complex?.Name); // Nạp lại dropdowns
//                return View(model);
//            }
//            // Gán lại tên complex cho view nếu có lỗi ModelState khác sau này
//            model.CourtComplexName = complex.Name;

//            if (ModelState.IsValid)
//            {
//                var createDto = new CreateCourtDto
//                {
//                    CourtComplexId = model.CourtComplexId,
//                    Name = model.Name,
//                    SportTypeId = model.SportTypeId,
//                    Description = model.Description,
//                    DefaultSlotDurationMinutes = model.DefaultSlotDurationMinutes,
//                    AdvanceBookingDaysLimit = model.AdvanceBookingDaysLimit,
//                    OpeningTime = model.OpeningTime,
//                    ClosingTime = model.ClosingTime,
//                    MainImageFile = model.MainImageFile,
//                    SelectedAmenityIds = model.SelectedAmenityIds
//                };

//                var (success, createdCourt, errors) = await _courtService.CreateCourtAsync(createDto, currentUser.Id);

//                if (success)
//                {
//                    _logger.LogInformation("OwnerCourtController: Court '{CourtName}' created successfully for complex {ComplexId}.", createdCourt.Name, model.CourtComplexId);
//                    TempData["SuccessMessage"] = $"Sân '{createdCourt.Name}' đã được thêm vào khu phức hợp '{complex.Name}'.";
//                    if (errors != null && errors.Any())
//                    {
//                        TempData["WarningMessage"] = string.Join(" ", errors);
//                    }
//                    return RedirectToAction(nameof(Index), "OwnerCourt", new { courtComplexId = model.CourtComplexId }); // Sửa controller ở đây
//                }
//                else
//                {
//                    _logger.LogWarning("OwnerCourtController: Failed to create court '{CourtName}'. Errors: {Errors}", model.Name, string.Join("; ", errors ?? new List<string>()));
//                    if (errors != null)
//                    {
//                        foreach (var error in errors)
//                        {
//                            ModelState.AddModelError(string.Empty, error);
//                        }
//                    }
//                    else
//                    {
//                        ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi không mong muốn khi tạo sân.");
//                    }
//                }
//            }
//            else
//            {
//                _logger.LogWarning("OwnerCourtController: POST Create court - ModelState is invalid. Errors: {Errors}",
//                                  string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
//            }

//            await PrepareViewModelForCreateView(model, complex?.Name); // Nạp lại dropdowns
//            return View(model);
//        }


//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> ToggleStatus(int courtId, int courtComplexId)
//        {
//            var currentUser = await _userManager.GetUserAsync(User);
//            if (currentUser == null) return Challenge();

//            var (success, newStatus, errorMessage) = await _courtService.ToggleCourtStatusAsync(courtId, currentUser.Id);

//            if (success)
//            {
//                TempData["SuccessMessage"] = $"Đã cập nhật trạng thái sân thành công: {newStatus}.";
//            }
//            else
//            {
//                TempData["ErrorMessage"] = errorMessage ?? "Không thể thay đổi trạng thái sân.";
//            }

//            return RedirectToAction(nameof(Index), new { courtComplexId = courtComplexId, _v = Guid.NewGuid().ToString() });

//        }
        
//        [HttpGet]
//        public async Task<IActionResult> Edit(int id)
//        {
//            var currentUser = await _userManager.GetUserAsync(User);
//            var court = await _courtService.GetCourtForEditAsync(id, currentUser.Id);

//            if (court == null)
//            {
//                _logger.LogWarning("Edit (GET): Court {CourtId} not found or user {UserId} has no permission.", id, currentUser.Id);
//                TempData["ErrorMessage"] = "Không tìm thấy sân hoặc bạn không có quyền chỉnh sửa.";
//                return RedirectToAction("Index", "CourtOwnerDashboard"); // Quay về dashboard chính
//            }

//            // Mapping từ Entity sang ViewModel
//            var model = new OwnerCourtViewModel
//            {
//                CourtId = court.CourtId,
//                CourtComplexId = court.CourtComplexId,
//                Name = court.Name,
//                SportTypeId = court.SportTypeId,
//                Description = court.Description,
//                DefaultSlotDurationMinutes = court.DefaultSlotDurationMinutes,
//                AdvanceBookingDaysLimit = court.AdvanceBookingDaysLimit,
//                OpeningTime = court.OpeningTime,
//                ClosingTime = court.ClosingTime,
//                MainImageCloudinaryUrl = court.MainImageCloudinaryUrl,
//                SelectedAmenityIds = court.CourtAmenities.Select(ca => ca.AmenityId).ToList(),
//                CourtComplexName = court.CourtComplex.Name
//            };

//            await PrepareViewModelForCreateView(model, model.CourtComplexName); // Tái sử dụng helper để nạp dropdowns
//            return View(model);
//        }

//        // **ACTION MỚI: POST EDIT**
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, OwnerCourtViewModel model)
//        {
//            if (id != model.CourtId)
//            {
//                return BadRequest();
//            }

//            var currentUser = await _userManager.GetUserAsync(User);
//            if (currentUser == null) return Challenge();
            
//            // Lấy lại tên complex để hiển thị lại nếu có lỗi
//            var complex = await _courtService.GetCourtComplexByIdAsync(model.CourtComplexId, currentUser.Id);
//            if(complex == null) return Forbid(); // Kiểm tra quyền sở hữu complex một lần nữa
//            model.CourtComplexName = complex.Name;


//            if (ModelState.IsValid)
//            {
//                var updateDto = new UpdateCourtDto
//                {
//                    CourtId = model.CourtId,
//                    CourtComplexId = model.CourtComplexId,
//                    Name = model.Name,
//                    SportTypeId = model.SportTypeId,
//                    Description = model.Description,
//                    DefaultSlotDurationMinutes = model.DefaultSlotDurationMinutes,
//                    AdvanceBookingDaysLimit = model.AdvanceBookingDaysLimit,
//                    OpeningTime = model.OpeningTime,
//                    ClosingTime = model.ClosingTime,
//                    NewMainImageFile = model.MainImageFile,
//                    SelectedAmenityIds = model.SelectedAmenityIds
//                };

//                var (success, errors) = await _courtService.UpdateCourtAsync(updateDto, currentUser.Id);

//                if (success)
//                {
//                    TempData["SuccessMessage"] = $"Thông tin sân '{model.Name}' đã được cập nhật thành công.";
//                    if (errors != null && errors.Any())
//                    {
//                        TempData["WarningMessage"] = string.Join(" ", errors);
//                    }
//                    return RedirectToAction(nameof(Index), new { courtComplexId = model.CourtComplexId });
//                }
//                else
//                {
//                    _logger.LogWarning("Failed to update court {CourtId}. Errors: {Errors}", model.CourtId, string.Join("; ", errors ?? new List<string>()));
//                    if (errors != null)
//                    {
//                        foreach (var error in errors) { ModelState.AddModelError(string.Empty, error); }
//                    }
//                }
//            }

//            // Nếu ModelState không hợp lệ hoặc service thất bại, nạp lại dữ liệu cho dropdowns
//            await PrepareViewModelForCreateView(model, model.CourtComplexName);
//            return View(model);
//        }
//        private async Task PrepareViewModelForCreateView(OwnerCourtViewModel model, string courtComplexName)
//        {
//            model.CourtComplexName = courtComplexName;
//            model.SportTypes = (await _courtService.GetAllSportTypesAsync())
//                                   .Select(st => new SelectListItem { Value = st.SportTypeId.ToString(), Text = st.Name })
//                                   .ToList();
//            model.AvailableAmenities = (await _courtService.GetAllAmenitiesAsync())
//                                   .Select(a => new SelectListItem { Value = a.AmenityId.ToString(), Text = a.Name })
//                                   .ToList();
//        }
//    }
//}
