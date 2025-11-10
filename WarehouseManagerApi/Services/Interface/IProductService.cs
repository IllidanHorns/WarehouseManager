/*using WarehouseManagerContracts.DTOs.Product;
using WarehouseManagerContracts.DTOs.Remaining;

namespace WarehouseManagerApi.Services.Interface
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync(bool includeArchived = false);
        Task<ProductDto?> GetByIdAsync(int id);
        Task<ProductDto> CreateAsync(CreateProductDto dto);
        Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto);
        Task<bool> aDeleteAsync(int id);
        Task<IEnumerable<RemainingDto>> GetStockAsync(int productId);
    }
}
*/