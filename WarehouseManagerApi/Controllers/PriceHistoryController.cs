using Microsoft.AspNetCore.Mvc;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Services.Interfaces;

namespace WarehouseManagerApi.Controllers
{
    [Route("api/[controller]")]
    public class PriceHistoryController : ApiControllerBase
    {
        private readonly IPriceHistoryService _priceHistoryService;

        public PriceHistoryController(IPriceHistoryService priceHistoryService)
        {
            _priceHistoryService = priceHistoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetHistory([FromQuery] PriceHistoryFilter filter)
        {
            var result = await _priceHistoryService.GetPagedAsync(filter);
            return Ok(result);
        }
    }
}
