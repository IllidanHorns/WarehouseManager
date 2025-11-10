using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManager.Services.Summary.Interfaces;

namespace WarehouseManager.Services.Summary
{
    public class WarehouseStockSummary : ISummary
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = default!;
        public string WarehouseAddress { get; set; } = default!;
        public int WarehouseId { get; set; }
        public int Quantity { get; set; }
        public bool ProductIsArchived { get; set; }
        public bool WarehouseIsArchived { get; set; }
    }
}
