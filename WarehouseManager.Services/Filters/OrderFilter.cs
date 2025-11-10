using WarehouseManager.Services.Filters.Interfaces;

namespace WarehouseManager.Services.Filters
{
    public class OrderFilter : IPaginationFilter
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public bool IncludeArchived { get; set; } = false;
        public int? WarehouseId { get; set; }
        public int? EmployeeId { get; set; }
        public int? StatusId { get; set; }
        public int? UserId { get; set; }
    }
}

