using PetShow.Models;

namespace PetShow.Models;

public enum BookingStatus
{
    Pending,
    Confirmed,
    Cancelled
}

public class Booking
{
    public int Id { get; set; }
    public BookingStatus Status { get; set; } = "Pending";
    public DateTime BookedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    // Уникальные ключи
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public int ShowId { get; set; }
    public Show Show { get; set; } = null!;

    public int PetId { get; set; }
    public Pet Pet { get; set; } = null!;
}