using WarehouseManager.Core.Models;
using WarehouseManagerContracts.DTOs.Auth;

namespace WarehouseManager.Services.Services.Interfaces
{
    public interface IAuthService
    {
        Task<User> AuthenticateAsync(LoginCommand command);
    }
}