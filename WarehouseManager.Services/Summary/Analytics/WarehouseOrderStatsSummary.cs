namespace WarehouseManager.Services.Summary.Analytics
{
    public class WarehouseOrderStatsSummary
    {
        public int WarehouseId { get; set; }
        public string WarehouseAddress { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public int ActiveOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int CompletedOrders { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
