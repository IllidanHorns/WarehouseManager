namespace WarehouseManager.Core.ViewDTOs
{
    public class EmployeePerformanceDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int AssignedOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal? AvgProcessingHours { get; set; }
    }
}
