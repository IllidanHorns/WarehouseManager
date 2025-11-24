using Microsoft.AspNetCore.Mvc;
using WarehouseManager.Contracts.DTOs.Remaining;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManagerApi.Services;

namespace WarehouseManagerApi.Controllers
{
    /// <summary>
    /// Управляет остатками товаров на складах.
    /// </summary>
    [Route("api/[controller]")]
    public class StocksController : ApiControllerBase
    {
        private readonly IStockService _stockService;
        private readonly CustomMetricsService? _customMetricsService;

        public StocksController(IStockService stockService, CustomMetricsService? customMetricsService = null)
        {
            _stockService = stockService;
            _customMetricsService = customMetricsService;
        }

        /// <summary>
        /// Возвращает остатки с учётом фильтров.
        /// </summary>
        /// <param name="filter">Параметры фильтрации.</param>
        [HttpGet]
        public async Task<IActionResult> GetStock([FromQuery] StockFilter filter)
        {
            var result = await _stockService.GetPagedAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Возвращает конкретный остаток по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор остатка.</param>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetStock(int id)
        {
            try
            {
                var stock = await _stockService.GetByIdAsync(id);
                return Ok(stock);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Создаёт запись об остатке.
        /// </summary>
        /// <param name="command">Данные нового остатка.</param>
        [HttpPost]
        public async Task<IActionResult> CreateStock([FromBody] CreateStockCommand command)
        {
            try
            {
                var created = await _stockService.CreateAsync(command);
                
                if (_customMetricsService != null)
                {
                    await _customMetricsService.UpdateTotalProductsInWarehousesAsync();
                }
                
                return CreatedAtAction(nameof(GetStock), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Обновляет количество товара на складе.
        /// </summary>
        /// <param name="id">Идентификатор остатка.</param>
        /// <param name="command">Новые данные остатка.</param>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockCommand command)
        {
            try
            {
                var updateCommand = command with { RemainingId = id };
                var updated = await _stockService.UpdateStockAsync(updateCommand);
                
                if (_customMetricsService != null)
                {
                    await _customMetricsService.UpdateTotalProductsInWarehousesAsync();
                }
                
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
