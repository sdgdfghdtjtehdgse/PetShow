namespace PetShow.Models;

public class PostLike
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public int PostId { get; set; }
    public Post Post { get; set; } = null!;
}

public class CommentLike
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public int CommentId { get; set; }
    public Comment Comment { get; set; } = null!;
}