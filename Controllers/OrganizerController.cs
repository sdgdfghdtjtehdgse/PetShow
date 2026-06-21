using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetShow.Data;
using PetShow.Models;

namespace PetShow.Controllers;

[Authorize(Roles = "Organizer,Admin")]
[Route("Organizer")]
public class OrganizerController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrganizerController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // ── Получаем данные из /Organizer ────────────────────────────────────────────────────────
    [Route("")]
    [Route("Index")]
    public async Task<IActionResult> Index()
    {
        var userId  = _userManager.GetUserId(User)!;
        var isAdmin = User.IsInRole("Admin");

        var shows = isAdmin
            ? await _db.Shows
                .Include(s => s.Venue)
                .Include(s => s.Bookings)
                .OrderByDescending(s => s.StartDate)
                .ToListAsync()
            : await _db.Shows
                .Include(s => s.Venue)
                .Include(s => s.Bookings)
                .Where(s => s.OrganizerId == userId)
                .OrderByDescending(s => s.StartDate)
                .ToListAsync();

        return View(shows);
    }

    // ── Получаем данные из /Organizer/CreateShow ─────────────────────────────────────────────
    [Route("CreateShow")]
    public async Task<IActionResult> CreateShow()
    {
        ViewBag.Venues = await _db.Venues.ToListAsync();
        return View(new Show());
    }

    // ── Получаем данные из /Organizer/CreateShow ────────────────────────────────────────────
    [HttpPost, Route("CreateShow"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateShow(Show model)
    {
        ModelState.Remove("Venue");
        ModelState.Remove("Bookings");
        ModelState.Remove("Rankings");
        ModelState.Remove("Questionnaires");
        ModelState.Remove("OrganizerId");
        ModelState.Remove("Organizer");

        if (!ModelState.IsValid)
        {
            ViewBag.Venues = await _db.Venues.ToListAsync();
            return View(model);
        }

        model.OrganizerId = _userManager.GetUserId(User)!;
        model.CreatedAt   = DateTime.UtcNow;
        _db.Shows.Add(model);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Выставка создана!";
        return RedirectToAction("Index");
    }

    // ── Получаем данные из /Organizer/EditShow/5 ─────────────────────────────────────────────
    [Route("EditShow/{id}")]
    public async Task<IActionResult> EditShow(int id)
    {
        var show = await GetOwnedShow(id);
        if (show == null) return Forbid();

        ViewBag.Venues = await _db.Venues.ToListAsync();
        return View(show);
    }

    // ── Получаем данные из /Organizer/EditShow/5 ────────────────────────────────────────────
    [HttpPost, Route("EditShow/{id}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditShow(Show model)
    {
        var show = await GetOwnedShow(model.Id);
        if (show == null) return Forbid();

        ModelState.Remove("Venue");
        ModelState.Remove("Bookings");
        ModelState.Remove("Rankings");
        ModelState.Remove("Questionnaires");
        ModelState.Remove("OrganizerId");
        ModelState.Remove("Organizer");

        if (!ModelState.IsValid)
        {
            ViewBag.Venues = await _db.Venues.ToListAsync();
            return View(model);
        }

        show.Title          = model.Title;
        show.Theme          = model.Theme;
        show.Description    = model.Description;
        show.StartDate      = model.StartDate;
        show.EndDate        = model.EndDate;
        show.AllowedAnimals = model.AllowedAnimals;
        show.MaxParticipants = model.MaxParticipants;
        show.VenueId        = model.VenueId;
        show.PosterUrl      = model.PosterUrl;
        show.IsActive       = model.IsActive;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Выставка обновлена!";
        return RedirectToAction("Index");
    }

    // ── Получаем данные из /Organizer/DeleteShow/5 ──────────────────────────────────────────
    [HttpPost, Route("DeleteShow/{id}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteShow(int id)
    {
        var show = await GetOwnedShow(id);
        if (show == null) return Forbid();

        _db.Shows.Remove(show);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Выставка удалена!";
        return RedirectToAction("Index");
    }

    // ── Вспомогательная функция: отображать только в том случае, если текущий пользователь является владельцем (или администратором) ───────────
    private async Task<Show?> GetOwnedShow(int id)
    {
        var userId  = _userManager.GetUserId(User)!;
        var isAdmin = User.IsInRole("Admin");
        var show    = await _db.Shows.FindAsync(id);

        if (show == null) return null;
        if (isAdmin || show.OrganizerId == userId) return show;
        return null;
    }
}