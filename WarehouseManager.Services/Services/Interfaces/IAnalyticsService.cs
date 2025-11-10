using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WarehouseManager.Services.Summary;
using WarehouseManager.Services.Summary.Analytics;

namespace WarehouseManager.Services.Services.Interfaces
{
    public interface IAnalyticsService
    {
        Task<List<WarehouseStockDistributionSummary>> GetWarehouseStockDistributionAsync();
        Task<List<OrderStatusDistributionSummary>> GetOrderStatusDistributionAsync();
        Task<List<CategoryProductCountSummary>> GetCategoryProductCountAsync();
        Task<List<MonthlyRevenueSummary>> GetMonthlyRevenueTrendAsync(int year);
        Task<List<EmployeePerformanceSummary>> GetEmployeePerformanceStatsAsync(DateTime startDate, DateTime endDate, int top = 5);
        Task<List<CategoryRevenueSummary>> GetTopCategoryRevenueAsync(DateTime startDate, DateTime endDate, int topCount = 5);
        Task<List<WarehouseOrderStatsSummary>> GetWarehouseOrderStatsAsync(DateTime startDate, DateTime endDate);
        Task<List<CategoryPriceStatsSummary>> GetCategoryPriceStatsAsync();
        Task<List<WarehouseStockDetailSummary>> GetWarehouseStockDetailsAsync();
    }
}

