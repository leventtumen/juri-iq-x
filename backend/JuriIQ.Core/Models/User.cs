using System;

namespace JuriIQ.Core.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public SubscriptionType SubscriptionType { get; set; } = SubscriptionType.Simple;
        public bool IsActive { get; set; } = true;
        public bool IsAdmin { get; set; } = false;
        public bool IsBlocked { get; set; } = false;
        public string? BlockedReason { get; set; }
        public DateTime? BlockedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum SubscriptionType
    {
        Simple,
        Pro
    }
}
