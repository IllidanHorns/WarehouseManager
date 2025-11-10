using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManager.Services.Filters.Interfaces;

namespace WarehouseManager.Services.Filters
{
    public class ProductsFilters : IPaginationFilter
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool IncludeArchived { get; set; }

        public int? CategoryId { get; set; }
        public int? WarehouseId { get; set; }
        public string? Name { get; set; }
    }
}
