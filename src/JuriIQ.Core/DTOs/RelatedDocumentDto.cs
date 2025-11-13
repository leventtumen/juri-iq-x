using JuriIQ.Core.Enums;

namespace JuriIQ.Core.DTOs;

public class RelatedDocumentDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public double SimilarityScore { get; set; }
}
