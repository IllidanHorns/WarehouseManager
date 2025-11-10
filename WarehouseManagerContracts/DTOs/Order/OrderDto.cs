using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManagerContracts.DTOs.OrderProduct;

namespace WarehouseManagerContracts.DTOs.Order
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreationDatetime { get; set; }
        public string WarehouseAddress { get; set; } = string.Empty;
        public string? EmployeeFullName { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string OrderStatusName { get; set; } = string.Empty;
        public List<OrderProductDto>? Products { get; set; }
    }
}
