using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManagerContracts.DTOs.Category
{
    public class UpdateCategoryCommand
    {
        public int? UserId { get; init; }
        public int Id { get; init; }
        public string Name { get; init; }
        public string? Description { get; init; }
    }
}
