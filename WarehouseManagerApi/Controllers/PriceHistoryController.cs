using Microsoft.AspNetCore.Mvc;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Services.Interfaces;

namespace WarehouseManagerApi.Controllers
{
    /// <summary>
    /// Предоставляет историю изменений цен.
    /// </summary>
    [Route("api/[controller]")]
    public class PriceHistoryController : ApiControllerBase
    {
        private readonly IPriceHistoryService _priceHistoryService;

        public PriceHistoryController(IPriceHistoryService priceHistoryService)
        {
            _priceHistoryService = priceHistoryService;
        }

        /// <summary>
        /// Возвращает историю изменений цен с фильтрами.
        /// </summary>
        /// <param name="filter">Параметры фильтрации и пагинации.</param>
        [HttpGet]
        public async Task<IActionResult> GetHistory([FromQuery] PriceHistoryFilter filter)
        {
            var result = await _priceHistoryService.GetPagedAsync(filter);
            return Ok(result);
        }
    }
}
