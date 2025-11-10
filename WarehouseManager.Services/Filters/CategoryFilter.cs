using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManager.Services.Filters.Interfaces;

namespace WarehouseManager.Services.Filters
{
    public class CategoryFilter : IPaginationFilter
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public bool IncludeArchived { get; set; } = false;
        public string? Name { get; set; }
    }
}
