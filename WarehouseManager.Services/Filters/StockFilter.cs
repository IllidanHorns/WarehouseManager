using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManager.Services.Filters.Interfaces;

namespace WarehouseManager.Services.Filters
{
    public class StockFilter : IPaginationFilter
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool IncludeArchived { get; set; }

        public int? ProductId { get; set; }
        public int? WarehouseId { get; set; }
        public string? ProductName { get; set; }
        public string? WarehouseAddress { get; set; }
    }
}
