using Microsoft.AspNetCore.Mvc;
using SportSync.Business.Dtos;
using SportSync.Business.Interfaces;
using SportSync.Business.Services;
using SportSync.Data;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SportSync.Web.Controllers;


[Route("Courts")]                     
public sealed class CourtsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ICourtService _courtService;

    public CourtsController( ApplicationDbContext db, ICourtService courtService)
    {
        _db = db;
        _courtService = courtService;
    }

    [HttpGet("")]       
    public IActionResult Index()
    {
        ViewBag.SportTypes = _db.SportTypes
                                .Where(t => t.IsActive)
                                .OrderBy(t => t.Name)
                                .Select(t => new { t.SportTypeId, t.Name })
                                .ToList();

        ViewBag.Amenities = _db.Amenities
                                .Where(a => a.IsActive)
                                .OrderBy(a => a.Name)
                                .Select(a => new { a.AmenityId, a.Name })
                                .ToList();

        return View();               
    }


    [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int id, DateOnly? date)
    {
        var dto = await _courtService.GetCourtDetailAsync(id);
        if (dto == null) return NotFound();
        ViewBag.SuccessMessage = TempData["SuccessMessage"] as string;
        ViewBag.ErrorMessage = TempData["ErrorMessage"] as string;
        ViewBag.SelectedDate = date ?? DateOnly.FromDateTime(DateTime.Today);
        return View(dto);
    }

}