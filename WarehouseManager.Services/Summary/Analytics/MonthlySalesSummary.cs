namespace WarehouseManager.Services.Summary.Analytics
{
    public class MonthlySalesSummary
    {
        public string PeriodLabel { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
