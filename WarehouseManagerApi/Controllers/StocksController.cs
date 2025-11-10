using Microsoft.AspNetCore.Mvc;
using WarehouseManager.Contracts.DTOs.Remaining;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Services.Interfaces;

namespace WarehouseManagerApi.Controllers
{
    [Route("api/[controller]")]
    public class StocksController : ApiControllerBase
    {
        private readonly IStockService _stockService;

        public StocksController(IStockService stockService)
        {
            _stockService = stockService;
        }

        [HttpGet]
        public async Task<IActionResult> GetStock([FromQuery] StockFilter filter)
        {
            var result = await _stockService.GetPagedAsync(filter);
            return Ok(result);
        }

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

        [HttpPost]
        public async Task<IActionResult> CreateStock([FromBody] CreateStockCommand command)
        {
            try
            {
                var created = await _stockService.CreateAsync(command);
                return CreatedAtAction(nameof(GetStock), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockCommand command)
        {
            try
            {
                var updateCommand = command with { RemainingId = id };
                var updated = await _stockService.UpdateStockAsync(updateCommand);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
