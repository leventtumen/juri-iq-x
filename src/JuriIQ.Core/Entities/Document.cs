using JuriIQ.Core.Enums;

namespace JuriIQ.Core.Entities;

public class Document
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DocumentType DocumentType { get; set; }
    public DocumentStatus Status { get; set; }
    public string? CourtName { get; set; }
    public string? CaseNumber { get; set; }
    public DateTime? DecisionDate { get; set; }
    public string? LawNumber { get; set; }
    public string? Category { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? ErrorMessage { get; set; }
    public int ViewCount { get; set; }
    public int BookmarkCount { get; set; }
    public DateTime ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<DocumentKeyword> Keywords { get; set; } = new List<DocumentKeyword>();
    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
}
