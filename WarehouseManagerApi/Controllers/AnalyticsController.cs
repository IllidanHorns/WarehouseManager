using System;
using Microsoft.AspNetCore.Mvc;
using WarehouseManager.Services.Services.Interfaces;

namespace WarehouseManagerApi.Controllers
{
    [Route("api/[controller]")]
    public class AnalyticsController : ApiControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("warehouse-stock")]
        public async Task<IActionResult> GetWarehouseStockDistribution()
        {
            var data = await _analyticsService.GetWarehouseStockDistributionAsync();
            return Ok(data);
        }

        [HttpGet("order-status")]
        public async Task<IActionResult> GetOrderStatusDistribution()
        {
            var data = await _analyticsService.GetOrderStatusDistributionAsync();
            return Ok(data);
        }

        [HttpGet("category-products")]
        public async Task<IActionResult> GetCategoryProductCount()
        {
            var data = await _analyticsService.GetCategoryProductCountAsync();
            return Ok(data);
        }

        [HttpGet("monthly-revenue")]
        public async Task<IActionResult> GetMonthlyRevenue([FromQuery] int? year)
        {
            var targetYear = year ?? DateTime.UtcNow.Year;
            var data = await _analyticsService.GetMonthlyRevenueTrendAsync(targetYear);
            return Ok(data);
        }

        [HttpGet("top-categories")]
        public async Task<IActionResult> GetTopCategories([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] int topCount = 5)
        {
            try
            {
                var (start, end) = ResolvePeriod(startDate, endDate);
                var data = await _analyticsService.GetTopCategoryRevenueAsync(start, end, topCount);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("warehouse-orders")]
        public async Task<IActionResult> GetWarehouseOrderStats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var (start, end) = ResolvePeriod(startDate, endDate);
                var data = await _analyticsService.GetWarehouseOrderStatsAsync(start, end);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("employee-performance")]
        public async Task<IActionResult> GetEmployeePerformance([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] int top = 5)
        {
            try
            {
                var (start, end) = ResolvePeriod(startDate, endDate);
                var data = await _analyticsService.GetEmployeePerformanceStatsAsync(start, end, top);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("category-price-stats")]
        public async Task<IActionResult> GetCategoryPriceStats()
        {
            var data = await _analyticsService.GetCategoryPriceStatsAsync();
            return Ok(data);
        }

        [HttpGet("warehouse-stock-details")]
        public async Task<IActionResult> GetWarehouseStockDetails()
        {
            var data = await _analyticsService.GetWarehouseStockDetailsAsync();
            return Ok(data);
        }

        private static (DateTime start, DateTime end) ResolvePeriod(DateTime? startDate, DateTime? endDate)
        {
            var end = endDate?.ToUniversalTime() ?? DateTime.UtcNow;
            var start = startDate?.ToUniversalTime() ?? end.AddMonths(-3);

            if (start > end)
            {
                throw new ArgumentException("Дата начала не может быть больше даты окончания.");
            }

            return (start, end);
        }
    }
}
