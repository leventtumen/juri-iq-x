using System;

namespace JuriIQ.Core.Models
{
    public class LoginAttempt
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public DateTime AttemptTime { get; set; } = DateTime.UtcNow;
        public bool Success { get; set; }
        public string? FailureReason { get; set; }
    }
}