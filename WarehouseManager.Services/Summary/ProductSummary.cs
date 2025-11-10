using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManager.Services.Summary.Interfaces;

namespace WarehouseManager.Services.Summary
{
    public class ProductSummary : ISummary
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public decimal Price { get; set; }
        public decimal Weight { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = default!;
        public bool IsArchived { get; set; }
        public DateTime CreationDatetime { get; set; }
    }
}
