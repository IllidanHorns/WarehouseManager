using WarehouseManager.Services.Filters.Interfaces;

namespace WarehouseManager.Services.Filters
{
    public class EmployeeWarehouseFilter : IPaginationFilter
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public bool IncludeArchived { get; set; } = false;
        public int? EmployeeId { get; set; }
        public int? WarehouseId { get; set; }
    }
}

