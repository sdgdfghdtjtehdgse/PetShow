using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetShow.Models;

namespace PetShow.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── Таблицы ────────────────────────────────────────────────────────────────
    public DbSet<Pet> Pets { get; set; }
    public DbSet<Venue> Venues { get; set; }
    public DbSet<Show> Shows { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Ranking> Rankings { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<ShowQuestionnaire> ShowQuestionnaires { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<PostLike> PostLikes { get; set; }
    public DbSet<CommentLike> CommentLikes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // Вызов базового сервера для таблиц идентификаторов

        // ── Бронирование: комбинация пользователь+шоу+питомец должен быть уникальным ───────────
        builder.Entity<Booking>()
            .HasIndex(b => new { b.UserId, b.ShowId, b.PetId })
            .IsUnique();

        // ── Отзывы: один отзыв на одного человека ─────────────────────────────
        builder.Entity<Review>()
            .HasIndex(r => new { r.UserId, r.VenueId })
            .IsUnique();

        // ── Рейтинг: один рейтинг на одного питомца каждой выставки ───────────────────────
        builder.Entity<Ranking>()
            .HasIndex(r => new { r.ShowId, r.Position })
            .IsUnique();

        // ── Каскадное удаление: удаление записи приводит к удалению комментариев к ней ──────────────
        builder.Entity<Comment>()
            .HasOne(c => c.Post)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Один лайк на один пост для одного пользователя ────────────────────────────────────────
        builder.Entity<PostLike>()
            .HasIndex(l => new { l.UserId, l.PostId })
            .IsUnique();

        // ── Один лайк на комментарий на один пост для одного пользователя ─────────────────────────────────────
        builder.Entity<CommentLike>()
            .HasIndex(l => new { l.UserId, l.CommentId })
            .IsUnique();

        // ── Ограничение: удаление пользователя НЕ приводит к автоматическому удалению бронирования ──────────
        builder.Entity<Booking>()
            .HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Ограничение: удаление пользователя НЕ приводит к автоматическому удалению уровня.Ограничение: удаление пользователя НЕ приводит к автоматическому удалению уровня ────────────
        builder.Entity<Booking>()
            .HasOne(b => b.Pet)
            .WithMany(p => p.Bookings)
            .HasForeignKey(b => b.PetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}