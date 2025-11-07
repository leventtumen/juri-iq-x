using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using JuriIQ.Core.DTOs;
using JuriIQ.Core.Interfaces;
using JuriIQ.Core.Models;

namespace JuriIQ.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly string _jwtSecret;
        private readonly string _jwtIssuer;
        private readonly int _jwtExpireMinutes;

        public AuthService(IUserRepository userRepository, string jwtSecret, string jwtIssuer, int jwtExpireMinutes)
        {
            _userRepository = userRepository;
            _jwtSecret = jwtSecret;
            _jwtIssuer = jwtIssuer;
            _jwtExpireMinutes = jwtExpireMinutes;
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request, string ipAddress)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            if (user.IsBlocked)
            {
                throw new UnauthorizedAccessException("Account is blocked: " + user.BlockedReason);
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("Account is not active");
            }

            // Check device limits based on subscription
            var activeDevices = await _userRepository.GetActiveDeviceCountAsync(user.Id);
            var maxDevices = user.SubscriptionType == SubscriptionType.Pro ? 4 : 1;

            var existingDevices = await _userRepository.GetUserDevicesAsync(user.Id);
            var deviceExists = existingDevices.Exists(d => d.DeviceId == request.DeviceId);

            if (!deviceExists && activeDevices >= maxDevices)
            {
                throw new UnauthorizedAccessException($"Maximum device limit reached for {user.SubscriptionType} subscription");
            }

            // Add or update device
            var device = new UserDevice
            {
                UserId = user.Id,
                DeviceId = request.DeviceId,
                DeviceName = request.DeviceName,
                DeviceType = request.DeviceType,
                LastLogin = DateTime.UtcNow
            };
            await _userRepository.AddDeviceAsync(device);

            var userDto = new UserDTO
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                SubscriptionType = user.SubscriptionType.ToString(),
                IsAdmin = user.IsAdmin
            };

            var token = GenerateToken(userDto);

            return new LoginResponseDTO
            {
                Token = token,
                User = userDto
            };
        }

        public async Task<UserDTO> RegisterAsync(RegisterRequestDTO request)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Email already registered");
            }

            var user = new User
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                SubscriptionType = SubscriptionType.Simple,
                IsActive = true,
                IsAdmin = false
            };

            var userId = await _userRepository.CreateAsync(user);
            user.Id = userId;

            return new UserDTO
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                SubscriptionType = user.SubscriptionType.ToString(),
                IsAdmin = user.IsAdmin
            };
        }

        public string GenerateToken(UserDTO user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtExpireMinutes),
                Issuer = _jwtIssuer,
                Audience = _jwtIssuer,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
