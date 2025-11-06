using JurisIQ.Backend.Configuration;
using JurisIQ.Backend.Data;
using JurisIQ.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace JurisIQ.Backend.Services
{
    public interface IAuthService
    {
        Task<(User? user, string? error)> LoginAsync(string email, string password, string deviceId, string deviceName, string deviceType, string userAgent, string ipAddress);
        Task<(User? user, string? error)> RegisterAsync(string email, string password, string firstName, string lastName);
        Task<User?> GetUserByIdAsync(int userId);
        Task LogUserDeviceAsync(int userId, string deviceId, string deviceName, string deviceType, string userAgent);
        Task LogFailedLoginAttemptAsync(string email, string userAgent, string ipAddress);
        Task<bool> IsUserBlockedAsync(string email);
        Task<bool> IsDeviceLimitExceededAsync(int userId);
        Task<List<User>> GetAllUsersAsync();
        Task BlockUserAsync(int userId);
        Task UnblockUserAsync(int userId);
    }

    public class AuthService : IAuthService
    {
        private readonly JurisIQDbContext _context;
        private readonly SecuritySettings _securitySettings;

        public AuthService(JurisIQDbContext context, SecuritySettings securitySettings)
        {
            _context = context;
            _securitySettings = securitySettings;
        }

        public async Task<(User? user, string? error)> LoginAsync(string email, string password, string deviceId, string deviceName, string deviceType, string userAgent, string ipAddress)
        {
            // Check if user is blocked
            if (await IsUserBlockedAsync(email))
            {
                return (null, "Account is temporarily blocked due to multiple failed login attempts");
            }

            var user = await _context.Users
                .Include(u => u.Devices)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user == null)
            {
                await LogFailedLoginAttemptAsync(email, userAgent, ipAddress);
                return (null, "Invalid email or password");
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                await LogFailedLoginAttemptAsync(email, userAgent, ipAddress);
                return (null, "Invalid email or password");
            }

            // Check device limit
            await LogUserDeviceAsync(user.Id, deviceId, deviceName, deviceType, userAgent);

            if (await IsDeviceLimitExceededAsync(user.Id))
            {
                return (null, "Device limit exceeded for your subscription plan");
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return (user, null);
        }

        public async Task<(User? user, string? error)> RegisterAsync(string email, string password, string firstName, string lastName)
        {
            // Check if user already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (existingUser != null)
            {
                return (null, "User with this email already exists");
            }

            var user = new User
            {
                Email = email.ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                FirstName = firstName,
                LastName = lastName,
                SubscriptionType = SubscriptionType.Simple,
                IsAdmin = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return (user, null);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Devices)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task LogUserDeviceAsync(int userId, string deviceId, string deviceName, string deviceType, string userAgent)
        {
            var existingDevice = await _context.UserDevices
                .FirstOrDefaultAsync(d => d.UserId == userId && d.DeviceId == deviceId);

            if (existingDevice != null)
            {
                existingDevice.LastLoginAt = DateTime.UtcNow;
                existingDevice.IsActive = true;
                existingDevice.UserAgent = userAgent;
                if (!string.IsNullOrEmpty(deviceName))
                    existingDevice.DeviceName = deviceName;
                if (!string.IsNullOrEmpty(deviceType))
                    existingDevice.DeviceType = deviceType;
            }
            else
            {
                var device = new UserDevice
                {
                    UserId = userId,
                    DeviceId = deviceId,
                    DeviceName = deviceName,
                    DeviceType = deviceType,
                    UserAgent = userAgent,
                    FirstLoginAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.UserDevices.Add(device);
            }

            await _context.SaveChangesAsync();
        }

        public async Task LogFailedLoginAttemptAsync(string email, string userAgent, string ipAddress)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user != null)
            {
                var failedAttempt = new FailedLoginAttempt
                {
                    UserId = user.Id,
                    IPAddress = ipAddress,
                    UserAgent = userAgent,
                    AttemptAt = DateTime.UtcNow
                };

                _context.FailedLoginAttempts.Add(failedAttempt);

                // Check if user should be blocked
                var recentAttempts = await _context.FailedLoginAttempts
                    .Where(f => f.UserId == user.Id && 
                               f.AttemptAt >= DateTime.UtcNow.AddMinutes(-_securitySettings.FailedLoginWindowMinutes))
                    .CountAsync();

                if (recentAttempts >= _securitySettings.MaxFailedLoginAttempts)
                {
                    user.IsBlocked = true;
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsUserBlockedAsync(string email)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user == null) return false;

            if (!user.IsBlocked) return false;

            // Check if block should be lifted
            var lastFailedAttempt = await _context.FailedLoginAttempts
                .Where(f => f.UserId == user.Id)
                .OrderByDescending(f => f.AttemptAt)
                .FirstOrDefaultAsync();

            if (lastFailedAttempt != null && 
                lastFailedAttempt.AttemptAt <= DateTime.UtcNow.AddMinutes(-_securitySettings.AccountLockoutMinutes))
            {
                user.IsBlocked = false;
                await _context.SaveChangesAsync();
                return false;
            }

            return user.IsBlocked;
        }

        public async Task<bool> IsDeviceLimitExceededAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Devices)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return true;

            var activeDevices = user.Devices.Count(d => d.IsActive);
            var maxDevices = user.SubscriptionType == SubscriptionType.Simple 
                ? _securitySettings.MaxDevicesSimple 
                : _securitySettings.MaxDevicesPro;

            return activeDevices > maxDevices;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Devices)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task BlockUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsBlocked = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UnblockUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsBlocked = false;
                // Clear failed login attempts
                var failedAttempts = await _context.FailedLoginAttempts
                    .Where(f => f.UserId == userId)
                    .ToListAsync();
                _context.FailedLoginAttempts.RemoveRange(failedAttempts);
                await _context.SaveChangesAsync();
            }
        }
    }
}