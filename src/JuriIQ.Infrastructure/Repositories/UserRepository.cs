using Dapper;
using JuriIQ.Core.Entities;
using JuriIQ.Core.Interfaces;
using JuriIQ.Infrastructure.Data;

namespace JuriIQ.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id AS Id, email AS Email, password_hash AS PasswordHash,
                   first_name AS FirstName, last_name AS LastName, is_admin AS IsAdmin,
                   subscription_type AS SubscriptionType, is_blacklisted AS IsBlacklisted,
                   failed_login_attempts AS FailedLoginAttempts, last_failed_login AS LastFailedLogin,
                   blocked_until AS BlockedUntil, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM users WHERE id = @Id";

        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id AS Id, email AS Email, password_hash AS PasswordHash,
                   first_name AS FirstName, last_name AS LastName, is_admin AS IsAdmin,
                   subscription_type AS SubscriptionType, is_blacklisted AS IsBlacklisted,
                   failed_login_attempts AS FailedLoginAttempts, last_failed_login AS LastFailedLogin,
                   blocked_until AS BlockedUntil, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM users WHERE email = @Email";

        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
    }

    public async Task<int> CreateAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO users (email, password_hash, first_name, last_name, is_admin, subscription_type, created_at)
            VALUES (@Email, @PasswordHash, @FirstName, @LastName, @IsAdmin, @SubscriptionType, @CreatedAt)
            RETURNING id";

        return await connection.ExecuteScalarAsync<int>(sql, user);
    }

    public async Task UpdateAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE users
            SET email = @Email, first_name = @FirstName, last_name = @LastName,
                subscription_type = @SubscriptionType, is_blacklisted = @IsBlacklisted,
                failed_login_attempts = @FailedLoginAttempts, last_failed_login = @LastFailedLogin,
                blocked_until = @BlockedUntil, updated_at = @UpdatedAt
            WHERE id = @Id";

        await connection.ExecuteAsync(sql, user);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(1) FROM users WHERE email = @Email";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email });
        return count > 0;
    }

    public async Task IncrementFailedLoginAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE users
            SET failed_login_attempts = failed_login_attempts + 1,
                last_failed_login = @Now,
                updated_at = @Now
            WHERE id = @UserId";

        await connection.ExecuteAsync(sql, new { UserId = userId, Now = DateTime.UtcNow });
    }

    public async Task ResetFailedLoginAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE users
            SET failed_login_attempts = 0,
                last_failed_login = NULL,
                blocked_until = NULL,
                updated_at = @Now
            WHERE id = @UserId";

        await connection.ExecuteAsync(sql, new { UserId = userId, Now = DateTime.UtcNow });
    }

    public async Task BlockUserAsync(int userId, DateTime until)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE users
            SET blocked_until = @Until,
                updated_at = @Now
            WHERE id = @UserId";

        await connection.ExecuteAsync(sql, new { UserId = userId, Until = until, Now = DateTime.UtcNow });
    }

    public async Task BlacklistUserAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE users
            SET is_blacklisted = TRUE,
                updated_at = @Now
            WHERE id = @UserId";

        await connection.ExecuteAsync(sql, new { UserId = userId, Now = DateTime.UtcNow });
    }
}
