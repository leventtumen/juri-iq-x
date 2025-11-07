using System;
using System.Collections.Generic;

namespace JuriIQ.Core.Models
{
    public class Document
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long? FileSize { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Summary { get; set; }
        public string? CourtName { get; set; }
        public string? CaseNumber { get; set; }
        public DateTime? DecisionDate { get; set; }
        public string? DocumentType { get; set; }
        public ProcessingStatus ProcessingStatus { get; set; } = ProcessingStatus.Pending;
        public string? ProcessingError { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public List<DocumentKeyword> Keywords { get; set; } = new();
        public DocumentStatistics? Statistics { get; set; }
    }

    public enum ProcessingStatus
    {
        Pending,
        Processing,
        Completed,
        Failed
    }

    public class DocumentKeyword
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public string Keyword { get; set; } = string.Empty;
        public decimal RelevanceScore { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class DocumentStatistics
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public int WordCount { get; set; }
        public int SentenceCount { get; set; }
        public int ParagraphCount { get; set; }
        public int PageCount { get; set; }
        public string? Language { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}