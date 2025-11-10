using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Filters.Interfaces;
using WarehouseManager.Services.Summary;
using WarehouseManagerContracts.DTOs.User;

namespace WarehouseManager.Services.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserSummary> CreateAsync(CreateUserCommand command);
        Task<UserSummary> UpdateAsync(UpdateUserCommand command);
        Task<UserSummary> GetByIdAsync(int id);
        Task<bool> ArchiveAsync(int targetUserId, int currentUserId);
        Task<PagedResult<UserSummary>> GetPagedAsync(IPaginationFilter filter);
        Task<string> HashPasswordAsync(string password);
    }
}
