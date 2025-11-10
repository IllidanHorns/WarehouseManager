using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManagerContracts.DTOs.OrderProduct
{
    public class OrderProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal OrderPrice { get; set; }
        public decimal TotalPrice => Quantity * OrderPrice;
    }
}
