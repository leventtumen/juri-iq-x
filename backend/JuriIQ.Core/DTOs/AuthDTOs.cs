using System;

namespace JuriIQ.Core.DTOs
{
    public class LoginRequestDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string? DeviceName { get; set; }
        public string? DeviceType { get; set; }
    }

    public class LoginResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public UserDTO User { get; set; } = null!;
    }

    public class RegisterRequestDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

    public class UserDTO
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string SubscriptionType { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
    }
}