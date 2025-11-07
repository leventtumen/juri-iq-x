using System;

namespace JuriIQ.Core.Models
{
    public class SearchHistory
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Query { get; set; } = string.Empty;
        public int ResultsCount { get; set; }
        public string? Filters { get; set; }
        public DateTime SearchTime { get; set; } = DateTime.UtcNow;
    }
}