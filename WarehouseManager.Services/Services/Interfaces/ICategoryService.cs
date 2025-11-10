using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManager.Services;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Filters.Interfaces;
using WarehouseManager.Services.Summary;
using WarehouseManagerContracts.DTOs.Category;

namespace WarehouseManager.Services.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<CategorySummary> CreateAsync(CreateCategoryCommand command);
        Task<CategorySummary> UpdateAsync(UpdateCategoryCommand command);
        Task<CategorySummary> GetByIdAsync(int id);
        Task<bool> ArchiveAsync(int id, int userId); 
        Task<PagedResult<CategorySummary>> GetPagedAsync(IPaginationFilter filter);
    }
}
