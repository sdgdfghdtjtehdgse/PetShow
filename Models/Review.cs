using PetShow.Models;

namespace PetShow.Models;

public class Review
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Уникальные ключи
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public int VenueId { get; set; }
    public Venue Venue { get; set; } = null!;
}