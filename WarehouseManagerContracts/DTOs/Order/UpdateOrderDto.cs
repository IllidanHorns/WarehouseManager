using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManagerContracts.DTOs.Order
{
    public class UpdateOrderDto
    {
        public decimal TotalPrice { get; set; }
        public int WarehouseId { get; set; }
        public int EmployeeId { get; set; } 
        public int StatusId { get; set; }
    }
}
