using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetShow.Data;
using PetShow.Models;

namespace PetShow.Controllers;

[Authorize(Roles = "Admin")]
[Route("Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // ── ДЭШБОРД ─────────────────────────────────────────────────────────────
    [Route("")]
    [Route("Index")]
    public async Task<IActionResult> Index()
    {
        ViewBag.UserCount          = await _db.Users.CountAsync();
        ViewBag.ShowCount          = await _db.Shows.CountAsync();
        ViewBag.VenueCount         = await _db.Venues.CountAsync();
        ViewBag.BookingCount       = await _db.Bookings.CountAsync();
        ViewBag.PetCount           = await _db.Pets.CountAsync();
        ViewBag.PostCount          = await _db.Posts.CountAsync();
        ViewBag.QuestionnaireCount = await _db.ShowQuestionnaires.CountAsync();
        ViewBag.RankingCount       = await _db.Rankings.CountAsync();
        ViewBag.ReviewCount        = await _db.Reviews.CountAsync();

        ViewBag.RecentBookings = await _db.Bookings
            .Include(b => b.User).Include(b => b.Show).Include(b => b.Pet)
            .OrderByDescending(b => b.BookedAt).Take(5).ToListAsync();

        return View();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // Места проведения
    // ══════════════════════════════════════════════════════════════════════════

    [Route("Venues")]
    public async Task<IActionResult> Venues() =>
        View(await _db.Venues.Include(v => v.Shows).ToListAsync());

    [Route("Venues/Create")]
    public IActionResult CreateVenue() => View(new Venue());

    [HttpPost, Route("Venues/Create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateVenue(Venue model)

    {
        if (!ModelState.IsValid) return View(model);
        _db.Venues.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Место создано!";
        return RedirectToAction("Venues");
    }

    [Route("Venues/Edit/{id}")]
    public async Task<IActionResult> EditVenue(int id)
    {
        var venue = await _db.Venues.FindAsync(id);
        if (venue == null) return NotFound();
        return View(venue);
    }

    [HttpPost, Route("Venues/Edit/{id}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditVenue(Venue model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.Venues.Update(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Место обновлено!";
        return RedirectToAction("Venues");
    }

    [HttpPost, Route("Venues/Delete/{id}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteVenue(int id)
    {
        var venue = await _db.Venues.FindAsync(id);
        if (venue == null) return NotFound();
        _db.Venues.Remove(venue);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Место удалено!";
        return RedirectToAction("Venues");
    }

    // ══════════════════════════════════════════════════════════════════════════
    // Выставки
    // ══════════════════════════════════════════════════════════════════════════

    [Route("Shows")]
    public async Task<IActionResult> Shows() =>
        View(await _db.Shows.Include(s => s.Venue).OrderByDescending(s => s.StartDate).ToListAsync());

    [Route("Shows/Create")]
    public async Task<IActionResult> CreateShow()
    {
        ViewBag.Venues = await _db.Venues.ToListAsync();
        return View(new Show());
    }

    [HttpPost, Route("Shows/Create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateShow(Show model)
    {
        ModelState.Remove("Venue");
        ModelState.Remove("Bookings");
        ModelState.Remove("Rankings");
        ModelState.Remove("Questionnaires");
        if (!ModelState.IsValid)
        {
            ViewBag.Venues = await _db.Venues.ToListAsync();
            return View(model);
        }
        model.CreatedAt = DateTime.UtcNow;
        _db.Shows.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Выставка создана!";
        return RedirectToAction("Shows");
    }

    [Route("Shows/Edit/{id}")]
    public async Task<IActionResult> EditShow(int id)
    {
        var show = await _db.Shows.FindAsync(id);
        if (show == null) return NotFound();
        ViewBag.Venues = await _db.Venues.ToListAsync();
        return View(show);
    }

    [HttpPost, Route("Shows/Edit/{id}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditShow(Show model)
    {
        ModelState.Remove("Venue");
        ModelState.Remove("Bookings");
        ModelState.Remove("Rankings");
        ModelState.Remove("Questionnaires");
        if (!ModelState.IsValid)
        {
            ViewBag.Venues = await _db.Venues.ToListAsync();
            return View(model);
        }
        _db.Shows.Update(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Выставка обновлена!";
        return RedirectToAction("Shows");
    }

    [HttpPost, Route("Shows/Delete/{id}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteShow(int id)
    {
        var show = await _db.Shows.FindAsync(id);
        if (show == null) return NotFound();
        _db.Shows.Remove(show);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Выставка удалена!";
        return RedirectToAction("Shows");
    }

    // ══════════════════════════════════════════════════════════════════════════
    // Бронирование
    // ══════════════════════════════════════════════════════════════════════════

    [Route("Bookings")]
    public async Task<IActionResult> Bookings() =>
        View(await _db.Bookings
            .Include(b => b.User).Include(b => b.Show).Include(b => b.Pet)
            .OrderByDescending(b => b.BookedAt).ToListAsync());

    [HttpPost, Route("Bookings/UpdateStatus/{id}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateBookingStatus(int id, BookingStatus status)
    {
        var booking = await _db.Bookings.FindAsync(id);
        if (booking == null) return NotFound();
        booking.Status = status;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Статус бронирования обновлен!";
        return RedirectToAction("Bookings");
    }

    [HttpPost, Route("Bookings/Delete/{id}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBooking(int id)
    {
        var booking = await _db.Bookings.FindAsync(id);
        if (booking == null) return NotFound();
        _db.Bookings.Remove(booking);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Бронирование удалено!";
        return RedirectToAction("Bookings");
    }

    // ══════════════════════════════════════════════════════════════════════════
    // Рейтинги
    // ══════════════════════════════════════════════════════════════════════════

    [Route("Rankings")]
    public async Task<IActionResult> Rankings() =>
        View(await _db.Rankings
            .Include(r => r.Show).Include(r => r.Pet)
            .OrderByDescending(r => r.AwardedAt).ToListAsync());

    [Route("Rankings/Create")]
    public async Task<IActionResult> CreateRanking()
    {
        ViewBag.Shows = await _db.Shows.ToListAsync();
        ViewBag.Pets  = await _db.Pets.ToListAsync();
        return View(new Ranking());
    }

    [HttpPost, Route("Rankings/Create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRanking(Ranking model)
    {
        ModelState.Remove("Show");
        ModelState.Remove("Pet");
        if (!ModelState.IsValid)
        {
            ViewBag.Shows = await _db.Shows.ToListAsync();
            ViewBag.Pets  = await _db.Pets.ToListAsync();
            return View(model);
        }
        model.AwardedAt = DateTime.UtcNow;
        _db.Rankings.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Рейтинг создан!";
        return RedirectToAction("Rankings");
    }

    [HttpPost, Route("Rankings/Delete/{id}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRanking(int id)
    {
        var ranking = await _db.Rankings.FindAsync(id);
        if (ranking == null) return NotFound();
        _db.Rankings.Remove(ranking);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Рейтинг удален!";
        return RedirectToAction("Rankings");
    }

    // ══════════════════════════════════════════════════════════════════════════
    // Анкеты
    // ══════════════════════════════════════════════════════════════════════════

    [Route("Questionnaires")]
    public async Task<IActionResult> Questionnaires() =>
        View(await _db.ShowQuestionnaires
            .Include(q => q.User).Include(q => q.Show)
            .OrderByDescending(q => q.SubmittedAt).ToListAsync());

    [HttpPost, Route("Questionnaires/Delete/{id}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuestionnaire(int id)
    {
        var q = await _db.ShowQuestionnaires.FindAsync(id);
        if (q == null) return NotFound();
        _db.ShowQuestionnaires.Remove(q);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Заявка удалена!";
        return RedirectToAction("Questionnaires");
    }

    // ══════════════════════════════════════════════════════════════════════════
    // Пользователи
    // ══════════════════════════════════════════════════════════════════════════

    [Route("Users")]
    public async Task<IActionResult> Users()
    {
        var users = _userManager.Users.ToList();
        var userRoles = new Dictionary<string, IList<string>>();
        foreach (var u in users)
            userRoles[u.Id] = await _userManager.GetRolesAsync(u);
        ViewBag.UserRoles = userRoles;
        return View(users);
    }

    [HttpPost, Route("Users/ToggleOrganizer/{userId}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleOrganizerRole(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();
        if (await _userManager.IsInRoleAsync(user, "Organizer"))
            await _userManager.RemoveFromRoleAsync(user, "Organizer");
        else
            await _userManager.AddToRoleAsync(user, "Organizer");
        TempData["Success"] = "Роль организатора обновлена!";
        return RedirectToAction("Users");
    }

    [HttpPost, Route("Users/ToggleAdmin/{userId}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAdminRole(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();
        if (await _userManager.IsInRoleAsync(user, "Admin"))
            await _userManager.RemoveFromRoleAsync(user, "Admin");
        else
            await _userManager.AddToRoleAsync(user, "Admin");
        TempData["Success"] = "Роль пользователя обновлена!";
        return RedirectToAction("Users");
    }

    [HttpPost, Route("Users/Delete/{userId}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var currentUserId = _userManager.GetUserId(User);
        if (userId == currentUserId)
        {
            TempData["Error"] = "Вы не можете удалить управляющий аккаунт...";
            return RedirectToAction("Users");
        }
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();
        await _userManager.DeleteAsync(user);
        TempData["Success"] = "Пользователь удален!";
        return RedirectToAction("Users");
    }

    // ══════════════════════════════════════════════════════════════════════════
    // Отзывы
    // ══════════════════════════════════════════════════════════════════════════

    [Route("Reviews")]
    public async Task<IActionResult> Reviews() =>
        View(await _db.Reviews
            .Include(r => r.User)
            .Include(r => r.Venue)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync());

    [HttpPost, Route("Reviews/Delete/{id}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var review = await _db.Reviews.FindAsync(id);
        if (review == null) return NotFound();
        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Отзыв удален!";
        return RedirectToAction("Reviews");
    }

    // ══════════════════════════════════════════════════════════════════════════
    // Питомцы
    // ══════════════════════════════════════════════════════════════════════════

    [Route("Pets")]
    public async Task<IActionResult> Pets() =>
        View(await _db.Pets
            .Include(p => p.Owner)
            .OrderBy(p => p.Name)
            .ToListAsync());

    [HttpPost, Route("Pets/Delete/{id}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePet(int id)
    {
        var pet = await _db.Pets.FindAsync(id);
        if (pet == null) return NotFound();
        _db.Pets.Remove(pet);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Питомец удален!";
        return RedirectToAction("Pets");
    }

    // ══════════════════════════════════════════════════════════════════════════
    // Модерация постов
    // ══════════════════════════════════════════════════════════════════════════

    [Route("Posts")]
    public async Task<IActionResult> Posts() =>
        View(await _db.Posts
            .Include(p => p.User).Include(p => p.Comments)
            .OrderByDescending(p => p.CreatedAt).ToListAsync());

    [HttpPost, Route("Posts/TogglePin/{id}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> TogglePin(int id)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post == null) return NotFound();
        post.IsPinned = !post.IsPinned;
        await _db.SaveChangesAsync();
        TempData["Success"] = post.IsPinned ? "Пост закреплен!" : "Пост откреплен!";
        return RedirectToAction("Posts");
    }

    [HttpPost, Route("Posts/Delete/{id}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePost(int id)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post == null) return NotFound();
        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Пост удален!";
        return RedirectToAction("Posts");
    }
}