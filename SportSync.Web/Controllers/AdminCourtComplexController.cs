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
            return View();
        }
    }
}