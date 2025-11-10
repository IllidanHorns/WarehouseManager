using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManager.Services.Summary.Interfaces;

namespace WarehouseManager.Services.Summary
{
    public class WarehouseSummary : ISummary
    {
        public int Id { get; set; }
        public string Address { get; set; } = default!;
        public int Square { get; set; }
        public bool IsArchived { get; set; }
        public DateTime CreationDatetime { get; set; }
        public int ActiveStockCount { get; set; }
        public int ActiveOrderCount { get; set; }
    }
}
