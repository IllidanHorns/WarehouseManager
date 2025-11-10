using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManagerContracts.DTOs.OrderProduct;

namespace WarehouseManagerContracts.DTOs.Order
{
    public class CreateOrderDto
    {
        public int WarehouseId { get; set; }
        public int EmployeeId { get; set; } 
        public int UserId { get; set; }
        public int StatusId { get; set; }
        public decimal TotalPrice { get; set; }

        public List<CreateOrderProductDto>? Products { get; set; }
    }
}
