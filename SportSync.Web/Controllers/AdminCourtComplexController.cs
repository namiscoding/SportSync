using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportSync.Business.Services;
using SportSync.Data.Enums;
using SportSync.Web.Models.ViewModels;
using SportSync.Web.Models.ViewModels.Court;
using System.Linq;
using System.Threading.Tasks;

namespace SportSync.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminCourtComplexController : Controller
    {
        private readonly CourtComplexManagementService _courtComplexManagementService;
        private readonly CourtManagementService _courtManagementService;

        public AdminCourtComplexController(CourtComplexManagementService courtComplexManagementService, CourtManagementService courtManagementService)
        {
            _courtComplexManagementService = courtComplexManagementService;
            _courtManagementService = courtManagementService;
        }

        public async Task<IActionResult> Index(string searchTerm)
        {
            var courtComplexes = string.IsNullOrEmpty(searchTerm)
                ? await _courtComplexManagementService.GetCourtComplexesAsync()
                : await _courtComplexManagementService.SearchCourtComplexesAsync(searchTerm);

            var courtComplexViewModels = courtComplexes.Select(cc => new CourtComplexViewModel
            {
                CourtComplexId = cc.CourtComplexId,
                Name = cc.Name,
                OwnerPhoneNumber = cc.OwnerUser?.PhoneNumber,
                OwnerFullName = cc.OwnerUser?.UserProfile?.FullName,
                Address = cc.Address,
                City = cc.City,
                District = cc.District,
                SportTypeName = cc.SportType?.Name,
                IsActiveByOwner = cc.IsActiveByOwner
            }).ToList();

            return View(courtComplexViewModels);
        }

        public async Task<IActionResult> Details(int id)
        {
            var courtComplex = await _courtComplexManagementService.GetCourtComplexByIdAsync(id);
            if (courtComplex == null)
            {
                return NotFound();
            }

            var viewModel = new CourtComplexDetailViewModel
            {
                CourtComplexId = courtComplex.CourtComplexId,
                Name = courtComplex.Name,
                OwnerPhoneNumber = courtComplex.OwnerUser?.PhoneNumber,
                OwnerFullName = courtComplex.OwnerUser?.UserProfile?.FullName,
                Address = courtComplex.Address,
                City = courtComplex.City,
                District = courtComplex.District,
                Ward = courtComplex.Ward,
                GoogleMapsLink = courtComplex.GoogleMapsLink,
                Description = courtComplex.Description,
                ContactPhoneNumber = courtComplex.ContactPhoneNumber,
                ContactEmail = courtComplex.ContactEmail,
                DefaultOpeningTime = courtComplex.DefaultOpeningTime,
                DefaultClosingTime = courtComplex.DefaultClosingTime,
                SportTypeName = courtComplex.SportType?.Name,
                IsActiveByOwner = courtComplex.IsActiveByOwner,
                MainImageUrl = courtComplex.MainImageCloudinaryUrl,
                CreatedAt = courtComplex.CreatedAt,
                UpdatedAt = courtComplex.UpdatedAt,
                Courts = courtComplex.Courts?.Select(c => new CourtViewModel
                {
                    CourtId = c.CourtId,
                    Name = c.Name,
                    SportTypeName = courtComplex.SportType?.Name,
                    StatusByOwner = c.StatusByOwner.ToString(),
                    IsActiveByOwner = c.StatusByOwner == (int)CourtStatusByOwner.Available
                }).ToList() ?? new List<CourtViewModel>()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleCourtComplexStatus(int courtComplexId)
        {
            await _courtComplexManagementService.ToggleCourtComplexStatusAsync(courtComplexId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleCourtStatus(int courtId)
        {
            await _courtManagementService.ToggleCourtStatusAsync(courtId);
            return RedirectToAction("Details", new { id = (await _courtManagementService.GetCourtByIdAsync(courtId)).CourtComplexId });
        }
    }
}