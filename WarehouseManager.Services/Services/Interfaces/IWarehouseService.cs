using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Filters.Interfaces;
using WarehouseManager.Services.Summary;
using WarehouseManagerContracts.DTOs.Warehouse;

namespace WarehouseManager.Services.Services.Interfaces
{
    public interface IWarehouseService
    {
        Task<WarehouseSummary> CreateAsync(CreateWarehouseCommand command);
        Task<WarehouseSummary> UpdateAsync(UpdateWarehouseCommand command);
        Task<WarehouseSummary> GetByIdAsync(int id);
        Task<bool> ArchiveAsync(int id, int userId);
        Task<PagedResult<WarehouseSummary>> GetPagedAsync(IPaginationFilter filter);
    }
}
