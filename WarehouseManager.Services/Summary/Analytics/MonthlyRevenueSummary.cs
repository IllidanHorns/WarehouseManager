namespace WarehouseManager.Services.Summary.Analytics
{
    public class MonthlyRevenueSummary
    {
        public int MonthNumber { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
