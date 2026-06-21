using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetShow.Data;

namespace PetShow.Controllers;

public class RankingsController : Controller
{
    private readonly AppDbContext _db;

    public RankingsController(AppDbContext db)
    {
        _db = db;
    }

    // ── Получаем данные из /Rankings ─────────────────────────────────────────────────────────
    // Показываем рейтинги на основании текущих/пройденных выставок
    public async Task<IActionResult> Index()
    {
        var shows = await _db.Shows
            .Include(s => s.Venue)
            .Include(s => s.Rankings)
            .ThenInclude(r => r.Pet)
            .Where(s => s.Rankings.Any())
            .OrderByDescending(s => s.StartDate)
            .ToListAsync();

        return View(shows);
    }

    // ── Получаем данные из /Rankings/Detail/5 ────────────────────────────────────────────────
    // Полный рейтинг с отбором по категориям
    public async Task<IActionResult> Detail(int id)
    {
        var show = await _db.Shows
            .Include(s => s.Venue)
            .Include(s => s.Rankings)
            .ThenInclude(r => r.Pet)
            .ThenInclude(p => p.Owner)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (show == null) return NotFound();

        // Рейтинг по категориям
        var grouped = show.Rankings
            .GroupBy(r => r.Category)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(r => r.Position).ToList()
            );

        ViewBag.Grouped = grouped;
        return View(show);
    }
}