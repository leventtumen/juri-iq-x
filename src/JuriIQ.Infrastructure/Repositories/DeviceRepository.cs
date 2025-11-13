using Dapper;
using JuriIQ.Core.Entities;
using JuriIQ.Core.Interfaces;
using JuriIQ.Infrastructure.Data;

namespace JuriIQ.Infrastructure.Repositories;

public class DeviceRepository : IDeviceRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DeviceRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Device?> GetByDeviceIdAsync(string deviceId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id AS Id, user_id AS UserId, device_id AS DeviceId,
                   device_name AS DeviceName, device_type AS DeviceType,
                   last_login_at AS LastLoginAt, created_at AS CreatedAt,
                   is_active AS IsActive
            FROM devices WHERE device_id = @DeviceId";

        return await connection.QueryFirstOrDefaultAsync<Device>(sql, new { DeviceId = deviceId });
    }

    public async Task<List<Device>> GetUserDevicesAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id AS Id, user_id AS UserId, device_id AS DeviceId,
                   device_name AS DeviceName, device_type AS DeviceType,
                   last_login_at AS LastLoginAt, created_at AS CreatedAt,
                   is_active AS IsActive
            FROM devices WHERE user_id = @UserId
            ORDER BY last_login_at DESC";

        var devices = await connection.QueryAsync<Device>(sql, new { UserId = userId });
        return devices.ToList();
    }

    public async Task<int> GetActiveDeviceCountAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT COUNT(*) FROM devices
            WHERE user_id = @UserId AND is_active = TRUE";

        return await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId });
    }

    public async Task<int> CreateAsync(Device device)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO devices (user_id, device_id, device_name, device_type, last_login_at, created_at, is_active)
            VALUES (@UserId, @DeviceId, @DeviceName, @DeviceType, @LastLoginAt, @CreatedAt, @IsActive)
            RETURNING id";

        return await connection.ExecuteScalarAsync<int>(sql, device);
    }

    public async Task UpdateLastLoginAsync(int deviceId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE devices
            SET last_login_at = @Now, is_active = TRUE
            WHERE id = @DeviceId";

        await connection.ExecuteAsync(sql, new { DeviceId = deviceId, Now = DateTime.UtcNow });
    }

    public async Task DeactivateDeviceAsync(int deviceId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE devices SET is_active = FALSE WHERE id = @DeviceId";
        await connection.ExecuteAsync(sql, new { DeviceId = deviceId });
    }
}
