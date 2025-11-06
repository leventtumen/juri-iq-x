using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JurisIQ.Backend.Models
{
    public class Document
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string FileType { get; set; } = string.Empty; // pdf, doc, docx, txt, dot

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? CourtName { get; set; }

        [MaxLength(50)]
        public string? CaseNumber { get; set; }

        public DateTime? DecisionDate { get; set; }

        public long FileSize { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ProcessedAt { get; set; }

        public bool IsProcessed { get; set; } = false;

        // Extracted content
        public string? FullText { get; set; }

        public string? Summary { get; set; }

        public string? Keywords { get; set; }

        // Navigation properties
        public virtual ICollection<DocumentKeyword> DocumentKeywords { get; set; } = new List<DocumentKeyword>();
        public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
        public virtual ICollection<DocumentView> Views { get; set; } = new List<DocumentView>();
    }

    public class DocumentKeyword
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Document")]
        public int DocumentId { get; set; }
        public virtual Document Document { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Keyword { get; set; } = string.Empty;

        public double RelevanceScore { get; set; } = 0.0;

        public int Frequency { get; set; } = 1;
    }

    public class Bookmark
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [Required]
        [ForeignKey("Document")]
        public int DocumentId { get; set; }
        public virtual Document Document { get; set; } = null!;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Unique constraint on UserId + DocumentId
    }

    public class DocumentView
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Document")]
        public int DocumentId { get; set; }
        public virtual Document Document { get; set; } = null!;

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [MaxLength(45)]
        public string? IPAddress { get; set; }

        [MaxLength(255)]
        public string? UserAgent { get; set; }

        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    }

    public class SearchHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string Query { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Filter { get; set; }

        public int ResultCount { get; set; } = 0;

        [MaxLength(45)]
        public string? IPAddress { get; set; }

        public DateTime SearchedAt { get; set; } = DateTime.UtcNow;
    }
}