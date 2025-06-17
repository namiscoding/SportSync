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
    private readonly ApplicationDbContext _db;

    public CourtsController( ApplicationDbContext db)
    {
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

}