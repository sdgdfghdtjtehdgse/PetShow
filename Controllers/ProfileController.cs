using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetShow.Data;
using PetShow.Models;

namespace PetShow.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // ── Получаем данные из /Profile ──────────────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var pets = await _db.Pets
            .Where(p => p.OwnerId == user.Id)
            .ToListAsync();

        var bookings = await _db.Bookings
            .Include(b => b.Show).ThenInclude(s => s.Venue)
            .Include(b => b.Pet)
            .Where(b => b.UserId == user.Id)
            .OrderByDescending(b => b.BookedAt)
            .Take(5)
            .ToListAsync();

        var questionnaires = await _db.ShowQuestionnaires
            .Include(q => q.Show)
            .Where(q => q.UserId == user.Id)
            .OrderByDescending(q => q.SubmittedAt)
            .Take(5)
            .ToListAsync();

        ViewBag.Pets = pets;
        ViewBag.Bookings = bookings;
        ViewBag.Questionnaires = questionnaires;
        return View(user);
    }

    // ── Получаем данные из /Profile/Edit ─────────────────────────────────────────────────────
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();
        return View(user);
    }

    // ── Получаем данные из /Profile/Edit ────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string fullName, string? bio, string? avatarUrl)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        user.FullName = fullName;
        user.Bio = bio;
        user.AvatarUrl = avatarUrl;

        await _userManager.UpdateAsync(user);
        TempData["Success"] = "Профиль успешно обновлен!";
        return RedirectToAction("Index");
    }

    // ── Получаем данные из /Profile/AddPet ───────────────────────────────────────────────────
    public IActionResult AddPet() => View(new Pet());

    // ── Получаем данные из /Profile/AddPet ──────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPet(Pet model)
    {
        var userId = _userManager.GetUserId(User)!;

        // Удаление организатора и его айди
        ModelState.Remove("OwnerId");
        ModelState.Remove("Owner");

        if (!ModelState.IsValid)
            return View(model);

        model.OwnerId = userId;
        model.CreatedAt = DateTime.UtcNow;

        _db.Pets.Add(model);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"{model.Name} теперь в списке ваших питомцев!";
        return RedirectToAction("Index");
    }

    // ── Получаем данные из /Profile/EditPet/5 ────────────────────────────────────────────────
    public async Task<IActionResult> EditPet(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var pet = await _db.Pets.FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);
        if (pet == null) return NotFound();
        return View(pet);
    }

    // ── Получаем данные из /Profile/EditPet ─────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPet(Pet model)
    {
        var userId = _userManager.GetUserId(User)!;
        var pet = await _db.Pets.FirstOrDefaultAsync(p => p.Id == model.Id && p.OwnerId == userId);
        if (pet == null) return NotFound();

        ModelState.Remove("OwnerId");
        ModelState.Remove("Owner");

        if (!ModelState.IsValid)
            return View(model);

        pet.Name = model.Name;
        pet.Species = model.Species;
        pet.Breed = model.Breed;
        pet.Age = model.Age;
        pet.PhotoUrl = model.PhotoUrl;
        pet.Description = model.Description;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Питомец обновлен!";
        return RedirectToAction("Index");
    }

    // ── Получаем данные из /Profile/DeletePet/5 ─────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePet(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var pet = await _db.Pets.FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);
        if (pet == null) return NotFound();

        _db.Pets.Remove(pet);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Питомец удален.";
        return RedirectToAction("Index");
    }
}