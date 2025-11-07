using System.Threading.Tasks;
using JuriIQ.Core.DTOs;

namespace JuriIQ.Core.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request, string ipAddress);
        Task<UserDTO> RegisterAsync(RegisterRequestDTO request);
        string GenerateToken(UserDTO user);
    }
}