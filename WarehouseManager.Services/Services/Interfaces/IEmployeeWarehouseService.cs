using WarehouseManager.Services;
using WarehouseManager.Services.Filters.Interfaces;
using WarehouseManager.Services.Summary;
using WarehouseManagerContracts.DTOs.EmployeeWarehouse;

namespace WarehouseManager.Services.Services.Interfaces
{
    public interface IEmployeeWarehouseService
    {
        Task<EmployeeWarehouseSummary> CreateAsync(CreateEmployeeWarehouseCommand command);
        Task<PagedResult<EmployeeWarehouseSummary>> GetPagedAsync(IPaginationFilter filter);
        Task<bool> ArchiveAsync(int employeeWarehouseId, int currentUserId);
    }
}

