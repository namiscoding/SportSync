using Microsoft.AspNetCore.Mvc;
using SportSync.Business.Dtos;
using SportSync.Business.Interfaces;
using SportSync.Data;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SportSync.Web.Controllers;


public class CourtsController : Controller
{
    private readonly ICourtSearchService _svc;
    private readonly ApplicationDbContext _db;
    public CourtsController(ICourtSearchService svc, ApplicationDbContext db) {
        _svc = svc;
        _db = db;
    }
       


    public IActionResult Index()
    {
        // Lấy các loại sân còn hoạt động
        var types = _db.SportTypes
                      .Where(t => t.IsActive)
                      .OrderBy(t => t.Name)
                      .Select(t => new { t.SportTypeId, t.Name })
                      .ToList();

        ViewBag.SportTypes = types;         
        return View();
    }

    /* ---------- API JSON ---------- */
    [HttpGet("api/v1/courts/search")]           
    [ProducesResponseType(typeof(IEnumerable<CourtComplexResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search(
        [FromQuery] CourtSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var result = await _svc.SearchAsync(request, cancellationToken);
        return Ok(result);                         
    }
}
