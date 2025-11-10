using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WWarehouseManager.Core.ViewDTOs
{
    public class OrderStatusDistributionDto
    {
        public string StatusName { get; set; } = default!;
        public int OrderCount { get; set; }
    }
}
