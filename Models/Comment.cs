using PetShow.Models;

namespace PetShow.Models;

public class Comment
{
    public int Id { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Уникальные ключи
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public int PostId { get; set; }
    public Post Post { get; set; } = null!;

    public ICollection<CommentLike> Likes { get; set; } = new List<CommentLike>();
}