using WarehouseManagerContracts.DTOs.Remaining;
using WarehouseManagerContracts.DTOs.Warehouse;

namespace WarehouseManagerApi.Services.Interface
{
    public interface IWarehouseService
    {
        Task<IEnumerable<WarehouseDto>> GetAllAsync(bool includeArchived = false);
        Task<WarehouseDto?> GetByIdAsync(int id);
        Task<WarehouseDto> CreateAsync(CreateWarehouseCommand dto);
        Task<WarehouseDto?> UpdateAsync(int id, UpdateWarehouseCommand dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<RemainingDto>> GetRemainingAsync(int warehouseId);
    }
}
