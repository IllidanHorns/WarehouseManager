using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManager.Services.Filters.Interfaces
{
    public interface IPaginationFilter
    {
        int Page { get; set; }
        int PageSize { get; set; }
        bool IncludeArchived { get; set; }
    }
}
