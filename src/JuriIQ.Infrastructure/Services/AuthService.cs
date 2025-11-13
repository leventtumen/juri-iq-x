using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using JuriIQ.Core.DTOs;
using JuriIQ.Core.Entities;
using JuriIQ.Core.Enums;
using JuriIQ.Core.Interfaces;

namespace JuriIQ.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        IDeviceRepository deviceRepository,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _deviceRepository = deviceRepository;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        // Get user by email
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Check if user is blacklisted
        if (user.IsBlacklisted)
        {
            throw new UnauthorizedAccessException("Account has been blacklisted. Please contact support.");
        }

        // Check if user is temporarily blocked
        if (user.BlockedUntil.HasValue && user.BlockedUntil.Value > DateTime.UtcNow)
        {
            var remainingMinutes = (user.BlockedUntil.Value - DateTime.UtcNow).TotalMinutes;
            throw new UnauthorizedAccessException($"Account is temporarily blocked. Try again in {Math.Ceiling(remainingMinutes)} minutes.");
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            await HandleFailedLogin(user);
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Check device limits based on subscription
        var activeDevices = await _deviceRepository.GetActiveDeviceCountAsync(user.Id);
        var maxDevices = user.SubscriptionType == SubscriptionType.Pro ? 4 : 1;

        var existingDevice = await _deviceRepository.GetByDeviceIdAsync(request.DeviceId);

        if (existingDevice == null)
        {
            // New device login
            if (activeDevices >= maxDevices)
            {
                // Blacklist user for exceeding device limit
                await _userRepository.BlacklistUserAsync(user.Id);
                throw new UnauthorizedAccessException($"Maximum device limit ({maxDevices}) exceeded. Account has been blacklisted.");
            }

            // Create new device
            var device = new Device
            {
                UserId = user.Id,
                DeviceId = request.DeviceId,
                DeviceName = request.DeviceName,
                DeviceType = request.DeviceType,
                LastLoginAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _deviceRepository.CreateAsync(device);
        }
        else if (existingDevice.UserId != user.Id)
        {
            // Device already registered to different user
            throw new UnauthorizedAccessException("This device is registered to another account.");
        }
        else
        {
            // Update existing device login time
            await _deviceRepository.UpdateLastLoginAsync(existingDevice.Id);
        }

        // Reset failed login attempts on successful login
        await _userRepository.ResetFailedLoginAsync(user.Id);

        // Generate JWT token
        var token = GenerateJwtToken(user.Id, user.Email, user.IsAdmin);

        return new LoginResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsAdmin = user.IsAdmin,
            SubscriptionType = user.SubscriptionType
        };
    }

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
    {
        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(request.Email))
        {
            throw new InvalidOperationException("Email already registered.");
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Create user
        var user = new User
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsAdmin = false,
            SubscriptionType = SubscriptionType.Simple, // Default to Simple
            CreatedAt = DateTime.UtcNow
        };

        var userId = await _userRepository.CreateAsync(user);
        user.Id = userId;

        // Generate JWT token
        var token = GenerateJwtToken(user.Id, user.Email, user.IsAdmin);

        return new LoginResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsAdmin = user.IsAdmin,
            SubscriptionType = user.SubscriptionType
        };
    }

    public string GenerateJwtToken(int userId, string email, bool isAdmin)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured.");
        var issuer = jwtSettings["Issuer"] ?? "JuriIQ";
        var audience = jwtSettings["Audience"] ?? "JuriIQ";
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "1440"); // 24 hours default

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new("isAdmin", isAdmin.ToString())
        };

        if (isAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task HandleFailedLogin(User user)
    {
        await _userRepository.IncrementFailedLoginAsync(user.Id);

        // Reload user to get updated failed login count
        user = await _userRepository.GetByIdAsync(user.Id) ?? user;

        // Block after 5 failed attempts within 15 minutes
        if (user.FailedLoginAttempts >= 5)
        {
            if (user.LastFailedLogin.HasValue &&
                (DateTime.UtcNow - user.LastFailedLogin.Value).TotalMinutes <= 15)
            {
                // Block for 30 minutes
                await _userRepository.BlockUserAsync(user.Id, DateTime.UtcNow.AddMinutes(30));
            }
        }
    }
}
