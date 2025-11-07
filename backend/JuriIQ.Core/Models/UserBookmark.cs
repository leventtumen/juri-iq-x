using System;

namespace JuriIQ.Core.Models
{
    public class UserBookmark
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid DocumentId { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public Document? Document { get; set; }
    }
}