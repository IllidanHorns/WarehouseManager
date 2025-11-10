using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManagerContracts.DTOs.Remaining
{
    public class RemainingDto
    {
        public int RemainingId { get; set; }
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int WarehouseId { get; set; }
        public string WarehouseAddress { get; set; } = string.Empty;
        public DateTime CreationDatetime { get; set; }
    }
}
