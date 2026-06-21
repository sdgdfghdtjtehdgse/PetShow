using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetShow.Data;
using PetShow.Models;

namespace PetShow.Controllers;

public class ReviewController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReviewController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // ── Получаем данные из /Review/Create ───────────────────────────────────────────────────
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int venueId, int showId, int rating, string title, string body)
    {
        var userId = _userManager.GetUserId(User)!;

        // Валидация цифры рейтинга (1-5)
        if (rating < 1 || rating > 5)
        {
            TempData["ReviewError"] = "Оценка должна быть от 1 до 5.";
            return RedirectToAction("Detail", "Show", new { id = showId });
        }

        // Пустая форма
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(body))
        {
            TempData["ReviewError"] = "Заполните заголовок и текст отзыва.";
            return RedirectToAction("Detail", "Show", new { id = showId });
        }

        // Смотрим дубликат отзыва
        var exists = await _db.Reviews
            .AnyAsync(r => r.UserId == userId && r.VenueId == venueId);

        if (exists)
        {
            TempData["ReviewError"] = "Вы уже оставляли отзыв об этой площадке.";
            return RedirectToAction("Detail", "Show", new { id = showId });
        }

        var review = new Review
        {
            UserId    = userId,
            VenueId   = venueId,
            Rating    = rating,
            Title     = title.Trim(),
            Body      = body.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();

        TempData["ReviewSuccess"] = "Ваш отзыв успешно опубликован!";
        return RedirectToAction("Detail", "Show", new { id = showId });
    }

    // ── Получаем данные из /Review/Delete ───────────────────────────────────────────────────
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int showId)
    {
        var userId  = _userManager.GetUserId(User)!;
        var isAdmin = User.IsInRole("Admin");

        var review = await _db.Reviews.FindAsync(id);
        if (review == null) return NotFound();

        // Только админ может удалять отзывы
        if (review.UserId != userId && !isAdmin)
            return Forbid();

        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync();

        TempData["ReviewSuccess"] = "Отзыв удалён.";
        return RedirectToAction("Detail", "Show", new { id = showId });
    }
}