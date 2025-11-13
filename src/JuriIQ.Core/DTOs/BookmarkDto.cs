using JuriIQ.Core.Enums;

namespace JuriIQ.Core.DTOs;

public class BookmarkDto
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public string DocumentTitle { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
