namespace JuriIQ.Core.Entities;

public class DocumentKeyword
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public string Keyword { get; set; } = string.Empty;
    public double Relevance { get; set; }  // 0.0 to 1.0
    public DateTime CreatedAt { get; set; }

    // Navigation property
    public Document Document { get; set; } = null!;
}
