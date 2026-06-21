using PetShow.Models;

namespace PetShow.Models;

public class Pet
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
    public int Age { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Уникальный ключ к главному
    public string OwnerId { get; set; } = string.Empty;
    public ApplicationUser Owner { get; set; } = null!;

    // Навигация
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Ranking> Rankings { get; set; } = new List<Ranking>();
}