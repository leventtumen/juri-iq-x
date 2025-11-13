namespace JuriIQ.Core.Entities;

public class Bookmark
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int DocumentId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Document Document { get; set; } = null!;
}
