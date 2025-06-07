using Microsoft.AspNetCore.Mvc;
using SportSync.Business.Dtos;
using SportSync.Business.Interfaces;
using SportSync.Data;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SportSync.Web.Controllers;


[Route("Courts")]                     
public sealed class CourtsController : Controller
{
    private readonly ICourtSearchService _searchSvc;
    private readonly ApplicationDbContext _db;

    public CourtsController(ICourtSearchService searchSvc, ApplicationDbContext db)
    {
        _searchSvc = searchSvc;
        _db = db;
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

    [HttpGet("Search")]
    public async Task<IActionResult> Search([FromQuery] CourtSearchRequest req,
                                            CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var data = await _searchSvc.SearchAsync(req, ct);
        return Json(data);             
    }

    [HttpGet("Nearby")]
    public async Task<IActionResult> Nearby(
        [FromQuery] double userLat,
        [FromQuery] double userLng,
        [FromQuery] double radiusKm,
        [FromQuery] CourtSearchRequest baseFilter,
        CancellationToken ct)
    {
        var data = await _searchSvc.SearchNearbyAsync(
            userLat, userLng, radiusKm, baseFilter, ct);
        return Json(data);
    }
  
    [HttpGet("/Court/Details/{id:int}")]
    public async Task<IActionResult> Details(int id, DateOnly? date)
    {
        var day = date ?? DateOnly.FromDateTime(DateTime.Today);
        var dto = await _searchSvc.GetDetailAsync(id, day);
        if (dto == null) return NotFound();

        ViewBag.SelectedDate = day;
        return View(dto);                   
    }
}