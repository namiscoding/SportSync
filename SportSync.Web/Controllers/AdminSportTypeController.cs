using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportSync.Business.Services;
using SportSync.Web.Models.ViewModels;
using SportSync.Web.Models.ViewModels.SportType;
using System.Linq;
using System.Threading.Tasks;

namespace SportSync.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminSportTypeController : Controller
    {
        private readonly SportTypeManagementService _sportTypeManagementService;

        public AdminSportTypeController(SportTypeManagementService sportTypeManagementService)
        {
            _sportTypeManagementService = sportTypeManagementService;
        }

        public async Task<IActionResult> Index()
        {
            var sportTypes = await _sportTypeManagementService.GetSportTypesAsync();
            var viewModels = sportTypes.Select(st => new SportTypeViewModel
            {
                SportTypeId = st.SportTypeId,
                Name = st.Name,
                Description = st.Description,
                IsActive = st.IsActive,
                //CreatedAt = st.CreatedAt,
                //UpdatedAt = st.UpdatedAt
            }).ToList();

            return View(viewModels);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string name, string description)
        {
            if (string.IsNullOrEmpty(name))
            {
                ModelState.AddModelError("", "Tên loại sân không được để trống.");
                return View();
            }

            await _sportTypeManagementService.AddSportTypeAsync(name, description);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var sportType = await _sportTypeManagementService.GetSportTypeByIdAsync(id);
            if (sportType == null)
            {
                return NotFound();
            }

            var viewModel = new SportTypeViewModel
            {
                SportTypeId = sportType.SportTypeId,
                Name = sportType.Name,
                Description = sportType.Description,
                IsActive = sportType.IsActive,
                //CreatedAt = sportType.CreatedAt,
                //UpdatedAt = sportType.UpdatedAt
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int sportTypeId, string name, string description, bool isActive)
        {
            if (string.IsNullOrEmpty(name))
            {
                ModelState.AddModelError("", "Tên loại sân không được để trống.");
                return View(new SportTypeViewModel { SportTypeId = sportTypeId, Name = name, Description = description, IsActive = isActive });
            }

            await _sportTypeManagementService.UpdateSportTypeAsync(sportTypeId, name, description, isActive);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int sportTypeId)
        {
            await _sportTypeManagementService.DeleteSportTypeAsync(sportTypeId);
            return RedirectToAction("Index");
        }
    }
}