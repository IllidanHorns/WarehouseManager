using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Filters.Interfaces;
using WarehouseManager.Services.Summary;
using WarehouseManagerContracts.DTOs.Product;

namespace WarehouseManager.Services.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductSummary> CreateAsync(CreateProductCommand command);
        Task<ProductSummary> UpdateAsync(UpdateProductCommand command);
        Task<ProductSummary> GetByIdAsync(int id);
        Task<bool> ArchiveAsync(int id, int userId);
        Task<PagedResult<ProductSummary>> GetPagedAsync(IPaginationFilter filter);
    }
}
