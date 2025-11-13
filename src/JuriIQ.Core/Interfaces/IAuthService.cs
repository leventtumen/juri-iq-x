using JuriIQ.Core.DTOs;

namespace JuriIQ.Core.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> RegisterAsync(RegisterRequest request);
    string GenerateJwtToken(int userId, string email, bool isAdmin);
}
