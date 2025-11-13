using JuriIQ.Core.Enums;

namespace JuriIQ.Core.DTOs;

public class SearchResultDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public string? CourtName { get; set; }
    public string? CaseNumber { get; set; }
    public DateTime? DecisionDate { get; set; }
    public string? LawNumber { get; set; }
    public string? Category { get; set; }
    public string Summary { get; set; } = string.Empty;
    public double RelationPercentage { get; set; }  // NLP similarity score
    public int ViewCount { get; set; }
    public int BookmarkCount { get; set; }
    public bool IsBookmarked { get; set; }
}
