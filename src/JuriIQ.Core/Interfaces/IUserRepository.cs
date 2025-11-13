using JuriIQ.Core.Entities;

namespace JuriIQ.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<int> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task<bool> EmailExistsAsync(string email);
    Task IncrementFailedLoginAsync(int userId);
    Task ResetFailedLoginAsync(int userId);
    Task BlockUserAsync(int userId, DateTime until);
    Task BlacklistUserAsync(int userId);
}
