using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManagerContracts.DTOs.Warehouse
{
    public class WarehouseDto
    {
        public int WarehouseId { get; set; }
        public string Address { get; set; } = string.Empty;
        public int Square { get; set; }
        public DateTime CreationDatetime { get; set; }
    }
}
