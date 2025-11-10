using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManager.Services.Filters.Interfaces;

namespace WarehouseManager.Services.Filters
{
    public class EmployeeFilters : IPaginationFilter
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public bool IncludeArchived { get; set; }

        public int? WarehouseId { get; set; }
    }
}
