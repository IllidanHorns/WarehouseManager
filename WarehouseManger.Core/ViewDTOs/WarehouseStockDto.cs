using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManager.Core.ViewDTOs
{
    public class WarehouseStockDto
    {
        public string WarehouseName { get; set; } = default!;
        public decimal TotalValue { get; set; }
    }
}
