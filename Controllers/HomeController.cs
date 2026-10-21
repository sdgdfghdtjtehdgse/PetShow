using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetShow.Data;

namespace PetShow.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        // 3 Предстоящих шоу для показа на главной странице
        var upcomingShows = await _db.Shows
            .Include(s => s.Venue)
            .Where(s => s.IsActive && s.StartDate <= DateTime.UtcNow)
            .OrderBy(s => s.StartDate)
            .Take(3)
            .ToListAsync();

        return View(upcomingShows);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}