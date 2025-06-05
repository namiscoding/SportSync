using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportSync.Business.Services;
using SportSync.Data.Enums;
using SportSync.Web.Models.ViewModels;
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
                ApprovalStatus = cc.ApprovalStatus.ToString(),
                IsActiveByAdmin = cc.IsActiveByAdmin
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
                Latitude = courtComplex.Latitude,
                Longitude = courtComplex.Longitude,
                Description = courtComplex.Description,
                ContactPhoneNumber = courtComplex.ContactPhoneNumber,
                ContactEmail = courtComplex.ContactEmail,
                DefaultOpeningTime = courtComplex.DefaultOpeningTime,
                DefaultClosingTime = courtComplex.DefaultClosingTime,
                ApprovalStatus = courtComplex.ApprovalStatus.ToString(),
                IsActiveByAdmin = courtComplex.IsActiveByAdmin,
                RejectionReason = courtComplex.RejectionReason,
                Courts = courtComplex.Courts?.Select(c => new CourtViewModel
                {
                    CourtId = c.CourtId,
                    Name = c.Name,
                    SportTypeName = c.SportType?.Name,
                    StatusByOwner = c.StatusByOwner.ToString(),
                    IsActiveByAdmin = c.IsActiveByAdmin
                }).ToList() ?? new List<CourtViewModel>()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int courtComplexId)
        {
            string adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
            {
                return Unauthorized();
            }
            await _courtComplexManagementService.ApproveCourtComplexAsync(courtComplexId, adminId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int courtComplexId, string rejectionReason)
        {
            string adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
            {
                return Unauthorized();
            }
            await _courtComplexManagementService.RejectCourtComplexAsync(courtComplexId, adminId, rejectionReason);
            return RedirectToAction("Index");
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