namespace WarehouseManager.Services.Summary.Analytics
{
    public class WarehouseStockDetailSummary
    {
        public int WarehouseId { get; set; }
        public string WarehouseAddress { get; set; } = string.Empty;
        public int Square { get; set; }
        public int DistinctProducts { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
    }
}
