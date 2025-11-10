namespace WarehouseManager.Core.ViewDTOs
{
    public class WarehouseOrderStatsDto
    {
        public int WarehouseId { get; set; }
        public string WarehouseAddress { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int ActiveOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int CompletedOrders { get; set; }
    }
}
