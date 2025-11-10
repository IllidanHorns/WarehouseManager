namespace WarehouseManager.Services.Summary.Analytics
{
    public class CategoryRevenueSummary
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int TotalUnits { get; set; }
    }
}
