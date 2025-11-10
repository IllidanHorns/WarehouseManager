using System.Threading.Tasks;
using WarehouseManager.Services;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Filters.Interfaces;
using WarehouseManager.Services.Summary;

namespace WarehouseManager.Services.Services.Interfaces
{
    public interface IPriceHistoryService
    {
        Task<PriceHistorySummary> GetByIdAsync(int id);
        Task<PagedResult<PriceHistorySummary>> GetPagedAsync(IPaginationFilter filter);
    }
}
