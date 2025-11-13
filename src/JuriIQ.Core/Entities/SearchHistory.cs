namespace JuriIQ.Core.Entities;

public class SearchHistory
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Query { get; set; } = string.Empty;
    public int ResultCount { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation property
    public User User { get; set; } = null!;
}
