using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JuriIQ.Core.Models;

namespace JuriIQ.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<Guid> CreateAsync(User user);
        Task UpdateAsync(User user);
        Task<List<UserDevice>> GetUserDevicesAsync(Guid userId);
        Task AddDeviceAsync(UserDevice device);
        Task<int> GetActiveDeviceCountAsync(Guid userId);
    }
}