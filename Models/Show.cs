namespace PetShow.Models;

public class Show
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;         // Тематика выставка: напр. спорт
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string AllowedAnimals { get; set; } = string.Empty; // Кому можно? Кошки-собаки
    public int MaxParticipants { get; set; }
    public string? PosterUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Организатор, создавший это шоу (значение может быть пустым — у шоу, созданных администратором, нет организатора)
    public string? OrganizerId { get; set; }
    public ApplicationUser? Organizer { get; set; }

    // Уникальный ключ - к месту проведения
    public int VenueId { get; set; }
    public Venue Venue { get; set; } = null!;

    // Навигация
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Ranking> Rankings { get; set; } = new List<Ranking>();
    public ICollection<ShowQuestionnaire> Questionnaires { get; set; } = new List<ShowQuestionnaire>();
}