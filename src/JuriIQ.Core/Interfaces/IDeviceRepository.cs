using JuriIQ.Core.Entities;

namespace JuriIQ.Core.Interfaces;

public interface IDeviceRepository
{
    Task<Device?> GetByDeviceIdAsync(string deviceId);
    Task<List<Device>> GetUserDevicesAsync(int userId);
    Task<int> GetActiveDeviceCountAsync(int userId);
    Task<int> CreateAsync(Device device);
    Task UpdateLastLoginAsync(int deviceId);
    Task DeactivateDeviceAsync(int deviceId);
}
