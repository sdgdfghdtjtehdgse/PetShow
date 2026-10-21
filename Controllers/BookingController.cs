using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetShow.Data;
using PetShow.Models;

namespace PetShow.Controllers;

[Authorize] // Все бронирования авторизованного пользователя
public class BookingController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public BookingController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // ── Получаем из /Booking/Create?showId=5 ──────────────────────────────────────────
    public async Task<IActionResult> Create(int showId)
    {
        var show = await _db.Shows
            .Include(s => s.Venue)
            .FirstOrDefaultAsync(s => s.Id == showId);

        if (show == null) return NotFound();

        var userId = _userManager.GetUserId(User)!;

        // Load user's pets for the dropdown
        var pets = await _db.Pets
            .Where(p => p.OwnerId == userId)
            .ToListAsync();

        ViewBag.Show = show;
        ViewBag.Pets = pets;
        return View();
    }

    // ── Получаем данные из /Booking/Create ──────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int showId, int petId, string? notes)
    {
        var userId = _userManager.GetUserId(User)!;

        // Просмотр активных выставок
        var show = await _db.Shows
            .Include(s => s.Venue)
            .FirstOrDefaultAsync(s => s.Id == showId && s.IsActive);

        if (show == null) return NotFound();

        // Просмотр питомцев
        var pet = await _db.Pets.FirstOrDefaultAsync(p => p.Id == petId && p.OwnerId == userId);
        if (pet == null)
        {
            TempData["Error"] = "Выбранный питомец не найден!";
            return RedirectToAction("Create", new { showId });
        }

        // Проверяем вметимость участников
        var confirmedCount = await _db.Bookings
            .CountAsync(b => b.ShowId == showId && b.Status == BookingStatus.Confirmed);
        if (confirmedCount > show.MaxParticipants)
        {
            TempData["Error"] = "К сожалению, мест на данную выставку больше нет...";
            return RedirectToAction("Detail", "Show", new { id = showId });
        }

        // Дубликат на бронирование
        var exists = await _db.Bookings
            .AnyAsync(b => b.UserId == userId && b.ShowId == showId && b.PetId == petId);
        if (exists)
        {
            TempData["Error"] = "Вы уже забронировали своего питомца на эту выставку!";
            return RedirectToAction("Detail", "Show", new { id = showId });
        }

        var booking = new Booking
        {
            UserId   = userId,
            ShowId   = showId,
            PetId    = petId,
            Notes    = notes,
            Status   = BookingStatus.Pending,
            BookedAt = DateTime.UtcNow
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Бронь отправлена! Проверяем информацию...";
        return RedirectToAction("MyBookings");
    }

    // ── Получаем данные из /Booking/MyBookings ───────────────────────────────────────────────
    public async Task<IActionResult> MyBookings()
    {
        var userId = _userManager.GetUserId(User)!;

        var bookings = await _db.Bookings
            .Include(b => b.Show)
                .ThenInclude(s => s.Venue)
            .Include(b => b.Pet)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookedAt)
            .ToListAsync();

        return View(bookings);
    }

    // ── Получаем данные из /Booking/Cancel/5 ────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var booking = await _db.Bookings
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null) return NotFound();

        booking.Status = BookingStatus.Cancelled;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Бронь отменена!";
        return RedirectToAction("MyBookings");
    }
}