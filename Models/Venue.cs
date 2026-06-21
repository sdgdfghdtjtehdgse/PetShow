namespace PetShow.Models;

public class Venue
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PhotoUrl { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    // Навигация
    public ICollection<Show> Shows { get; set; } = new List<Show>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}