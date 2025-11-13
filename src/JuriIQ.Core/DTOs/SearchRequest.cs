using JuriIQ.Core.Enums;

namespace JuriIQ.Core.DTOs;

public class SearchRequest
{
    public string Query { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public DocumentType? DocumentType { get; set; }
    public string? CourtName { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
