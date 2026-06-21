using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetShow.Data;
using PetShow.Models;

namespace PetShow.Controllers;

[Authorize]
public class QuestionnaireController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public QuestionnaireController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // ── Получаем данные из /Questionnaire/Create?showId=5 ────────────────────────────────────
    public async Task<IActionResult> Create(int showId)
    {
        var show = await _db.Shows.FirstOrDefaultAsync(s => s.Id == showId);
        if (show == null) return NotFound();

        ViewBag.Show = show;
        return View(new ShowQuestionnaire { ShowId = showId });
    }

    // ── Получаем данные из /Questionnaire/Create ────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ShowQuestionnaire model)
    {
        var userId = _userManager.GetUserId(User)!;

        // Дубликат анкеты
        var exists = await _db.ShowQuestionnaires
            .AnyAsync(q => q.UserId == userId && q.ShowId == model.ShowId);
        if (exists)
        {
            TempData["Error"] = "Вы уже отправляли анкету для этой выставки.";
            return RedirectToAction("Detail", "Show", new { id = model.ShowId });
        }

        // Удаляем "родителей" навигации для валидации
        ModelState.Remove("UserId");
        ModelState.Remove("User");
        ModelState.Remove("Show");

        if (!ModelState.IsValid)
        {
            ViewBag.Show = await _db.Shows.FindAsync(model.ShowId);
            // Видимость ошибок валидации
            ViewBag.ValidationErrors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return View(model);
        }

        model.UserId      = userId;
        model.SubmittedAt = DateTime.UtcNow;

        _db.ShowQuestionnaires.Add(model);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Анкета успешно отправлена!";
        return RedirectToAction("Detail", "Show", new { id = model.ShowId });
    }

    // ── Получаем данные из /Questionnaire/MySubmissions ─────────────────────────────────────
    public async Task<IActionResult> MySubmissions()
    {
        var userId = _userManager.GetUserId(User)!;

        var submissions = await _db.ShowQuestionnaires
            .Include(q => q.Show)
            .Where(q => q.UserId == userId)
            .OrderByDescending(q => q.SubmittedAt)
            .ToListAsync();

        return View(submissions);
    }
}