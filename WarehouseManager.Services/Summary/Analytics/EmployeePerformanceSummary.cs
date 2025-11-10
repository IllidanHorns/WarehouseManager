namespace WarehouseManager.Services.Summary.Analytics
{
    public class EmployeePerformanceSummary
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int OrdersHandled { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal? AverageProcessingHours { get; set; }
    }
}
