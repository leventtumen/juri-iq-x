using System;
using System.Collections.Generic;

namespace JuriIQ.Core.DTOs
{
    public class DocumentDTO
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Summary { get; set; }
        public string? CourtName { get; set; }
        public string? CaseNumber { get; set; }
        public DateTime? DecisionDate { get; set; }
        public string? DocumentType { get; set; }
        public List<string> Keywords { get; set; } = new();
        public DocumentStatisticsDTO? Statistics { get; set; }
    }

    public class DocumentSearchResultDTO
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Summary { get; set; }
        public string? CourtName { get; set; }
        public decimal RelevanceScore { get; set; }
        public List<string> MatchedKeywords { get; set; } = new();
    }

    public class DocumentStatisticsDTO
    {
        public int WordCount { get; set; }
        public int SentenceCount { get; set; }
        public int ParagraphCount { get; set; }
        public int PageCount { get; set; }
    }

    public class SearchRequestDTO
    {
        public string Query { get; set; } = string.Empty;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? DocumentType { get; set; }
        public string? CourtName { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class SearchResponseDTO
    {
        public List<DocumentSearchResultDTO> Results { get; set; } = new();
        public int TotalResults { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}