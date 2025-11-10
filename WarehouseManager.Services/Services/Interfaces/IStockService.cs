using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManager.Contracts.DTOs.Remaining;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Filters.Interfaces;
using WarehouseManager.Services.Summary;

namespace WarehouseManager.Services.Services.Interfaces
{
    public interface IStockService
    {
        Task<PagedResult<WarehouseStockSummary>> GetPagedAsync(IPaginationFilter filter);
        Task<WarehouseStockSummary> GetByIdAsync(int remainingId);
        Task<WarehouseStockSummary> CreateAsync(CreateStockCommand command);
        Task<WarehouseStockSummary> UpdateStockAsync(UpdateStockCommand stockCommand);
    }
}
