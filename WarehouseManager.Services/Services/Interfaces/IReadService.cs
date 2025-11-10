using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManager.Services.Filters.Interfaces;
using WarehouseManager.Services.Summary.Interfaces;

namespace WarehouseManager.Services.Services.Interfaces
{
    public interface IReadService<TSummary>
        where TSummary: class, ISummary
    {
        Task<TSummary> GetByIdAsync(int id);
        Task<PagedResult<TSummary>> GetPagedAsync(IPaginationFilter filter);
    }
}
