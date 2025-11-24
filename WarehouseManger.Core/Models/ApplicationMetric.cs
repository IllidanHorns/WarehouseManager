namespace WarehouseManager.Core.Models
{
    public class ApplicationMetric
    {
        public int Id { get; set; }
        public string MetricName { get; set; } = string.Empty;
        public double Value { get; set; }
        public DateTime LastUpdated { get; set; }
        public string? Description { get; set; }
    }
}

