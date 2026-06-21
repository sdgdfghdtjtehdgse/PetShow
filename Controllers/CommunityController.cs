using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetShow.Data;
using PetShow.Models;

namespace PetShow.Controllers;

public class CommunityController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public CommunityController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // ── Получаем данные из /Community ────────────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var posts = await _db.Posts
            .Include(p => p.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .OrderByDescending(p => p.IsPinned)
            .ThenByDescending(p => p.CreatedAt)
            .ToListAsync();

        return View(posts);
    }

    // ── Получаем данные из /Community/PostDetail/5 ───────────────────────────────────────────
    public async Task<IActionResult> PostDetail(int id)
    {
        var post = await _db.Posts
            .Include(p => p.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
            .Include(p => p.Comments)
                .ThenInclude(c => c.Likes)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        ViewBag.CurrentUserId    = userId;
        ViewBag.UserLikedPost    = userId != null && post.Likes.Any(l => l.UserId == userId);
        ViewBag.UserLikedComment = post.Comments
            .ToDictionary(c => c.Id, c => userId != null && c.Likes.Any(l => l.UserId == userId));

        return View(post);
    }

    // ── Получаем данные из /Community/CreatePost ─────────────────────────────────────────────
    [Authorize]
    public IActionResult CreatePost() => View(new Post());

    // ── Получаем данные из /Community/CreatePost ────────────────────────────────────────────
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePost(Post model)
    {
        var userId = _userManager.GetUserId(User)!;

        ModelState.Remove("UserId");
        ModelState.Remove("User");
        ModelState.Remove("Comments");

        if (!ModelState.IsValid)
            return View(model);

        model.UserId    = userId;
        model.CreatedAt = DateTime.UtcNow;
        model.IsPinned  = false;

        _db.Posts.Add(model);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Пост опубликован!";
        return RedirectToAction("Index");
    }

    // ── Получаем данные из /Community/AddComment ────────────────────────────────────────────
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int postId, string body)
    {
        var userId = _userManager.GetUserId(User)!;

        if (string.IsNullOrWhiteSpace(body))
        {
            TempData["Error"] = "Ошибка! Комментарий не должен быть пустым.";
            return RedirectToAction("PostDetail", new { id = postId });
        }

        var comment = new Comment
        {
            PostId    = postId,
            UserId    = userId,
            Body      = body.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        return RedirectToAction("PostDetail", new { id = postId });
    }

    // ── Получаем данные из /Community/EditPost/5 ─────────────────────────────────────────────
    [Authorize]
    public async Task<IActionResult> EditPost(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id);
        if (post == null) return NotFound();

        // Редактирование только от имени автора
        if (post.UserId != userId)
            return Forbid();

        return View(post);
    }

    // ── Получаем данные из /Community/EditPost ──────────────────────────────────────────────
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(int id, string title, string body, string? imageUrl)
    {
        var userId = _userManager.GetUserId(User)!;
        var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id);
        if (post == null) return NotFound();

        if (post.UserId != userId)
            return Forbid();

        post.Title    = title;
        post.Body     = body;
        post.ImageUrl = imageUrl;

        await _db.SaveChangesAsync();

        TempData["Success"] = "Пост успешно обновлен!";
        return RedirectToAction("PostDetail", new { id });
    }

    // ── Получаем данные из /Community/LikePost/5 ────────────────────────────────────────────
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LikePost(int postId)
    {
        var userId = _userManager.GetUserId(User)!;
        var existing = await _db.PostLikes
            .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

        if (existing == null)
            _db.PostLikes.Add(new PetShow.Models.PostLike { PostId = postId, UserId = userId });
        else
            _db.PostLikes.Remove(existing);

        await _db.SaveChangesAsync();
        return RedirectToAction("PostDetail", new { id = postId });
    }

    // ── Получаем данные из /Community/LikeComment/5 ────────────────────────────────────────
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LikeComment(int commentId, int postId)
    {
        var userId = _userManager.GetUserId(User)!;
        var existing = await _db.CommentLikes
            .FirstOrDefaultAsync(l => l.CommentId == commentId && l.UserId == userId);

        if (existing == null)
            _db.CommentLikes.Add(new PetShow.Models.CommentLike { CommentId = commentId, UserId = userId });
        else
            _db.CommentLikes.Remove(existing);

        await _db.SaveChangesAsync();
        return RedirectToAction("PostDetail", new { id = postId });
    }

    // ── Получаем данные из /Community/DeletePost/5 ──────────────────────────────────────────
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePost(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var isAdmin = User.IsInRole("Admin");

        var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id);
        if (post == null) return NotFound();

        // Only the author or an admin can delete
        if (post.UserId != userId && !isAdmin)
            return Forbid();

        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Пост удален!";
        return RedirectToAction("Index");
    }

    // ── Получаем данные из /Community/DeleteComment/5 ──────────────────────────────────────
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteComment(int id, int postId)
    {
        var userId  = _userManager.GetUserId(User)!;
        var isAdmin = User.IsInRole("Admin");

        var comment = await _db.Comments.FirstOrDefaultAsync(c => c.Id == id);
        if (comment == null) return NotFound();

        if (comment.UserId != userId && !isAdmin)
            return Forbid();

        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync();

        return RedirectToAction("PostDetail", new { id = postId });
    }
}