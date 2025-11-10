using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManagerContracts.DTOs.PriceHistory
{
    public class PriceChangeDto
    {
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public DateTime CreationDatetime { get; set; }
    }
}
