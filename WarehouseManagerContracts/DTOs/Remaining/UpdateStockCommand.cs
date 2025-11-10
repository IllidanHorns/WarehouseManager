using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManager.Contracts.DTOs.Remaining
{
    public record class UpdateStockCommand
    {
        public int UserId { get; init; }
        public int RemainingId { get; init; }
        public int NewQuantity { get; init; }
    }
}
