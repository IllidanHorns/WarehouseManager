using Microsoft.EntityFrameworkCore;
using WarehouseManager.Core.Data;
using WarehouseManager.Core.Models;
using WarehouseManager.Services.Services.Interfaces;

namespace WarehouseManager.Application.Services
{
    public class MetricsService : IMetricsService
    {
        private readonly AppDbContext _context;

        public MetricsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<double> GetMetricValueAsync(string metricName)
        {
            var metric = await _context.ApplicationMetrics
                .FirstOrDefaultAsync(m => m.MetricName == metricName);

            return metric?.Value ?? 0.0;
        }

        public async Task SetMetricValueAsync(string metricName, double value, string? description = null)
        {
            var metric = await _context.ApplicationMetrics
                .FirstOrDefaultAsync(m => m.MetricName == metricName);

            if (metric == null)
            {
                metric = new ApplicationMetric
                {
                    MetricName = metricName,
                    Value = value,
                    Description = description,
                    LastUpdated = DateTime.UtcNow
                };
                _context.ApplicationMetrics.Add(metric);
            }
            else
            {
                metric.Value = value;
                metric.LastUpdated = DateTime.UtcNow;
                if (description != null)
                {
                    metric.Description = description;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task IncrementMetricAsync(string metricName, double increment = 1.0)
        {
            var metric = await _context.ApplicationMetrics
                .FirstOrDefaultAsync(m => m.MetricName == metricName);

            if (metric == null)
            {
                metric = new ApplicationMetric
                {
                    MetricName = metricName,
                    Value = increment,
                    LastUpdated = DateTime.UtcNow
                };
                _context.ApplicationMetrics.Add(metric);
            }
            else
            {
                metric.Value += increment;
                metric.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task InitializeMetricsFromDatabaseAsync()
        {
            await Task.CompletedTask;
        }
    }
}

