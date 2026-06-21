namespace PetShow.Models;

public class Ranking
{
    public int Id { get; set; }
    public int Position { get; set; }          // 1, 2, 3 место и так далее
    public string Category { get; set; } = string.Empty; // За что наградили? Самая умная собака? Самый тупой хомяк?
    public string? AwardTitle { get; set; }    // Награды: Золото, серебро и прочее
    public string? Notes { get; set; }
    public DateTime AwardedAt { get; set; } = DateTime.UtcNow;

    // Уникальные ключи
    public int ShowId { get; set; }
    public Show Show { get; set; } = null!;

    public int PetId { get; set; }
    public Pet Pet { get; set; } = null!;
}