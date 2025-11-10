using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManagerContracts.DTOs.Category
{
    public class CreateCategoryCommand
    {
        public int? UserId { get; init; }
        public string Name { get; init; }
        public string? Description { get; init; }
    }
}
