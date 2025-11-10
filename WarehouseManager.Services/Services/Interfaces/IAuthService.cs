using WarehouseManagerContracts.DTOs.Auth;
using WarehouseManager.Core.Models;

namespace WarehouseManager.Services.Services.Interfaces
{
    public interface IAuthService
    {
        Task<User> AuthenticateAsync(LoginCommand command);
    }
}