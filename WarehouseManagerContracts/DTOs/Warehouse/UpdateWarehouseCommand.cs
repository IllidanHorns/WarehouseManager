using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManagerContracts.DTOs.Warehouse
{
    public class UpdateWarehouseCommand
    {
        public int UserId { get; init; }
        public int Id { get; init; }
        public string Address { get; init; }
        public int Square { get; init; }
    }
}
