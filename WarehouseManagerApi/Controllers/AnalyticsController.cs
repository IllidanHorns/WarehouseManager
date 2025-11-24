using System;
using Microsoft.AspNetCore.Mvc;
using WarehouseManager.Services.Services.Interfaces;

namespace WarehouseManagerApi.Controllers
{
    /// <summary>
    /// Предоставляет аналитические отчёты по складам, заказам и сотрудникам.
    /// </summary>
    [Route("api/[controller]")]
    public class AnalyticsController : ApiControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Возвращает распределение стоимости запасов по складам.
        /// </summary>
        [HttpGet("warehouse-stock")]
        public async Task<IActionResult> GetWarehouseStockDistribution()
        {
            var data = await _analyticsService.GetWarehouseStockDistributionAsync();
            return Ok(data);
        }

        /// <summary>
        /// Возвращает распределение статусов заказов.
        /// </summary>
        [HttpGet("order-status")]
        public async Task<IActionResult> GetOrderStatusDistribution()
        {
            var data = await _analyticsService.GetOrderStatusDistributionAsync();
            return Ok(data);
        }

        /// <summary>
        /// Возвращает количество товаров в каждой категории.
        /// </summary>
        [HttpGet("category-products")]
        public async Task<IActionResult> GetCategoryProductCount()
        {
            var data = await _analyticsService.GetCategoryProductCountAsync();
            return Ok(data);
        }

        /// <summary>
        /// Возвращает динамику выручки по месяцам.
        /// </summary>
        /// <param name="year">Год, для которого требуется статистика. По умолчанию — текущий.</param>
        [HttpGet("monthly-revenue")]
        public async Task<IActionResult> GetMonthlyRevenue([FromQuery] int? year)
        {
            var targetYear = year ?? DateTime.UtcNow.Year;
            var data = await _analyticsService.GetMonthlyRevenueTrendAsync(targetYear);
            return Ok(data);
        }

        /// <summary>
        /// Возвращает ТОП категорий по выручке за период.
        /// </summary>
        /// <param name="startDate">Дата начала периода.</param>
        /// <param name="endDate">Дата окончания периода.</param>
        /// <param name="topCount">Количество категорий в выборке.</param>
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

        /// <summary>
        /// Возвращает статистику заказов по складам.
        /// </summary>
        /// <param name="startDate">Дата начала периода.</param>
        /// <param name="endDate">Дата окончания периода.</param>
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

        /// <summary>
        /// Возвращает эффективность сотрудников за период.
        /// </summary>
        /// <param name="startDate">Дата начала периода.</param>
        /// <param name="endDate">Дата окончания периода.</param>
        /// <param name="top">Количество сотрудников в выборке.</param>
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

        /// <summary>
        /// Возвращает статистику цен по категориям.
        /// </summary>
        [HttpGet("category-price-stats")]
        public async Task<IActionResult> GetCategoryPriceStats()
        {
            var data = await _analyticsService.GetCategoryPriceStatsAsync();
            return Ok(data);
        }

        /// <summary>
        /// Возвращает детальные сведения по остаткам на складах.
        /// </summary>
        [HttpGet("warehouse-stock-details")]
        public async Task<IActionResult> GetWarehouseStockDetails()
        {
            var data = await _analyticsService.GetWarehouseStockDetailsAsync();
            return Ok(data);
        }

        internal static (DateTime start, DateTime end) ResolvePeriod(DateTime? startDate, DateTime? endDate)
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
