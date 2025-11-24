namespace WarehouseManager.Services.Services.Interfaces
{
    public interface IMetricsService
    {
        Task<double> GetMetricValueAsync(string metricName);
        Task SetMetricValueAsync(string metricName, double value, string? description = null);
        Task IncrementMetricAsync(string metricName, double increment = 1.0);
        Task InitializeMetricsFromDatabaseAsync();
    }
}

