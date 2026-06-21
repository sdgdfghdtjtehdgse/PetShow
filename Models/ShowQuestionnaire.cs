using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using PetShow.Models;

namespace PetShow.Models;

public class ShowQuestionnaire
{
    public int Id { get; set; }
    public string PetName { get; set; } = string.Empty;
    public string PetSpecies { get; set; } = string.Empty;
    public string PetBreed { get; set; } = string.Empty;
    public int PetAge { get; set; }
    public bool IsVaccinated { get; set; }
    public bool HasSpecialNeeds { get; set; }
    public string? SpecialNeedsDetails { get; set; }
    public string? AdditionalInfo { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    // Уникальные ключи
    public string UserId { get; set; } = string.Empty;

    [ValidateNever]
    public ApplicationUser User { get; set; } = null!;

    public int ShowId { get; set; }

    [ValidateNever]
    public Show Show { get; set; } = null!;
}