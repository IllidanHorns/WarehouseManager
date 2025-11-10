namespace WarehouseManager.Core.ViewDTOs
{
    public class TopCategoryRevenueDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int TotalUnits { get; set; }
    }
}
