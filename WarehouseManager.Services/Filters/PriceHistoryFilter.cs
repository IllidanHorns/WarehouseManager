using WarehouseManager.Services.Filters.Interfaces;

namespace WarehouseManager.Services.Filters
{
    public class PriceHistoryFilter : IPaginationFilter
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public bool IncludeArchived { get; set; }
        public string? ProductName { get; set; }
        public int? CategoryId { get; set; }
    }
}
