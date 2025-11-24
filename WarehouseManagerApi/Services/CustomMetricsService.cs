using Microsoft.EntityFrameworkCore;
using Prometheus;
using WarehouseManager.Core.Data;
using WarehouseManager.Services.Services.Interfaces;

namespace WarehouseManagerApi.Services
{
    public class CustomMetricsService
    {
        private readonly IMetricsService _metricsService;
        private readonly AppDbContext _context;

        private readonly Gauge _totalOrdersCreated;
        private readonly Gauge _activeUsersCount;
        private readonly Gauge _totalProductsInWarehouses;

        public CustomMetricsService(IMetricsService metricsService, AppDbContext context)
        {
            _metricsService = metricsService;
            _context = context;

            _totalOrdersCreated = Metrics
                .CreateGauge("warehouse_total_orders_created", 
                    "Общее количество созданных заказов с момента запуска системы");

            _activeUsersCount = Metrics
                .CreateGauge("warehouse_active_users_count", 
                    "Количество активных пользователей в системе");

            _totalProductsInWarehouses = Metrics
                .CreateGauge("warehouse_total_products_quantity", 
                    "Общее количество товаров на всех складах");
        }

        public async Task InitializeMetricsAsync()
        {
            var totalOrders = await _metricsService.GetMetricValueAsync("TotalOrdersCreated");
            _totalOrdersCreated.Set(totalOrders);

            await UpdateActiveUsersCountAsync();
            await UpdateTotalProductsInWarehousesAsync();
        }

        public async Task IncrementOrdersCreatedAsync()
        {
            await _metricsService.IncrementMetricAsync("TotalOrdersCreated");
            var currentValue = await _metricsService.GetMetricValueAsync("TotalOrdersCreated");
            _totalOrdersCreated.Set(currentValue);
        }

        public async Task UpdateActiveUsersCountAsync()
        {
            var activeUsersCount = await _context.Users
                .Where(u => !u.IsArchived)
                .CountAsync();

            await _metricsService.SetMetricValueAsync("ActiveUsersCount", activeUsersCount, 
                "Количество активных пользователей");
            _activeUsersCount.Set(activeUsersCount);
        }

        public async Task UpdateTotalProductsInWarehousesAsync()
        {
            var totalQuantity = await _context.Remaining
                .Where(r => !r.IsArchived)
                .SumAsync(r => (double)r.Quantity);

            await _metricsService.SetMetricValueAsync("TotalProductsInWarehouses", totalQuantity, 
                "Общее количество товаров на всех складах");
            _totalProductsInWarehouses.Set(totalQuantity);
        }
    }
}

