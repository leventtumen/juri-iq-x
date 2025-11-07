using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using JuriIQ.Core.Interfaces;
using JuriIQ.Core.Models;
using JuriIQ.Infrastructure.Data;

namespace JuriIQ.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DbContext _dbContext;

        public UserRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            using var connection = _dbContext.CreateConnection();
            var sql = "SELECT * FROM users WHERE id = @Id";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            using var connection = _dbContext.CreateConnection();
            var sql = "SELECT * FROM users WHERE email = @Email";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
        }

        public async Task<Guid> CreateAsync(User user)
        {
            using var connection = _dbContext.CreateConnection();
            var sql = @"
                INSERT INTO users (email, password_hash, first_name, last_name, subscription_type, is_active, is_admin)
                VALUES (@Email, @PasswordHash, @FirstName, @LastName, @SubscriptionType, @IsActive, @IsAdmin)
                RETURNING id";
            
            return await connection.QuerySingleAsync<Guid>(sql, user);
        }

        public async Task UpdateAsync(User user)
        {
            using var connection = _dbContext.CreateConnection();
            var sql = @"
                UPDATE users 
                SET email = @Email, first_name = @FirstName, last_name = @LastName,
                    subscription_type = @SubscriptionType, is_active = @IsActive,
                    is_blocked = @IsBlocked, blocked_reason = @BlockedReason,
                    blocked_at = @BlockedAt, updated_at = CURRENT_TIMESTAMP
                WHERE id = @Id";
            
            await connection.ExecuteAsync(sql, user);
        }

        public async Task<List<UserDevice>> GetUserDevicesAsync(Guid userId)
        {
            using var connection = _dbContext.CreateConnection();
            var sql = "SELECT * FROM user_devices WHERE user_id = @UserId AND is_active = true";
            var devices = await connection.QueryAsync<UserDevice>(sql, new { UserId = userId });
            return devices.ToList();
        }

        public async Task AddDeviceAsync(UserDevice device)
        {
            using var connection = _dbContext.CreateConnection();
            var sql = @"
                INSERT INTO user_devices (user_id, device_id, device_name, device_type, last_login)
                VALUES (@UserId, @DeviceId, @DeviceName, @DeviceType, @LastLogin)
                ON CONFLICT (user_id, device_id) 
                DO UPDATE SET last_login = @LastLogin, is_active = true";
            
            await connection.ExecuteAsync(sql, device);
        }

        public async Task<int> GetActiveDeviceCountAsync(Guid userId)
        {
            using var connection = _dbContext.CreateConnection();
            var sql = "SELECT COUNT(*) FROM user_devices WHERE user_id = @UserId AND is_active = true";
            return await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId });
        }
    }
}
