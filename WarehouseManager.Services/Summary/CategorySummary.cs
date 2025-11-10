using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManager.Services.Summary.Interfaces;

namespace WarehouseManager.Services.Summary
{
    public class CategorySummary : ISummary
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsArchived { get; set; }
        public DateTime CreationDatetime { get; set; }
        public int ActiveProductCount { get; set; }
    }
}
