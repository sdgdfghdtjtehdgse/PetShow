using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetShow.Data;

namespace PetShow.Controllers;

public class ShowController : Controller
{
    private readonly AppDbContext _db;

    public ShowController(AppDbContext db)
    {
        _db = db;
    }

    // ── Получаем данные из /Show ─────────────────────────────────────────────────────────────
    // Список выставок с фильтрами
    public async Task<IActionResult> Index(string? theme, string? animal, string? city)
    {
        var query = _db.Shows
            .Include(s => s.Venue)
            .Where(s => s.IsActive)
            .AsQueryable();

        if (!string.IsNullOrEmpty(theme))
            query = query.Where(s => s.Theme.Contains(theme));

        if (!string.IsNullOrEmpty(animal))
            query = query.Where(s => s.AllowedAnimals.Contains(animal));

        if (!string.IsNullOrEmpty(city))
            query = query.Where(s => s.Venue.City.Contains(city));

        var shows = await query.OrderBy(s => s.StartDate).ToListAsync();

        // Выбор фильтров для просмотра выставки
        ViewBag.Theme  = theme;
        ViewBag.Animal = animal;
        ViewBag.City   = city;
        
        ViewBag.Themes  = await _db.Shows.Select(s => s.Theme).Distinct().ToListAsync();
        ViewBag.Animals = await _db.Shows.Select(s => s.AllowedAnimals).Distinct().ToListAsync();
        ViewBag.Cities  = await _db.Venues.Select(v => v.City).Distinct().ToListAsync();

        return View(shows);
    }

    // ── Получаем данные из /Show/Detail/5 ────────────────────────────────────────────────────
    public async Task<IActionResult> Detail(int id)
    {
        var show = await _db.Shows
            .Include(s => s.Venue)
                .ThenInclude(v => v.Reviews)
                    .ThenInclude(r => r.User)
            .Include(s => s.Bookings)
            .Include(s => s.Rankings)
                .ThenInclude(r => r.Pet)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (show == null) return NotFound();

        return View(show);
    }
}