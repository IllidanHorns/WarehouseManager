using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManagerContracts.DTOs.OrderProduct
{
    public class CreateOrderProductDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal OrderPrice { get; set; }
    }
}
