using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManagerContracts.DTOs.OrderStatus
{
    public class OrderStatusDto
    {
        public int OrderStatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public DateTime CreationDatetime { get; set; }
    }
}
