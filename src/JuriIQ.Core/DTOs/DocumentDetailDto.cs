using JuriIQ.Core.Enums;

namespace JuriIQ.Core.DTOs;

public class DocumentDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public string? CourtName { get; set; }
    public string? CaseNumber { get; set; }
    public DateTime? DecisionDate { get; set; }
    public string? LawNumber { get; set; }
    public string? Category { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public List<string> Keywords { get; set; } = new();
    public int ViewCount { get; set; }
    public int BookmarkCount { get; set; }
    public bool IsBookmarked { get; set; }
    public List<RelatedDocumentDto> RelatedDocuments { get; set; } = new();
}
