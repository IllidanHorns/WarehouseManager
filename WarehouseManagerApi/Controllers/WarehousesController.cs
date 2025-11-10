using Microsoft.AspNetCore.Mvc;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManagerContracts.DTOs.Warehouse;

namespace WarehouseManagerApi.Controllers
{
    [Route("api/[controller]")]
    public class WarehousesController : ApiControllerBase
    {
        private readonly IWarehouseService _warehouseService;

        public WarehousesController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetWarehouses([FromQuery] WarehouseFilter filter)
        {
            var result = await _warehouseService.GetPagedAsync(filter);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetWarehouse(int id)
        {
            try
            {
                var warehouse = await _warehouseService.GetByIdAsync(id);
                return Ok(warehouse);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateWarehouse([FromBody] CreateWarehouseCommand command)
        {
            try
            {
                var created = await _warehouseService.CreateAsync(command);
                return CreatedAtAction(nameof(GetWarehouse), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateWarehouse(int id, [FromBody] UpdateWarehouseCommand command)
        {
            try
            {
                var updateCommand = new UpdateWarehouseCommand
                {
                    Id = id,
                    Address = command.Address,
                    Square = command.Square,
                    UserId = command.UserId
                };

                var updated = await _warehouseService.UpdateAsync(updateCommand);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> ArchiveWarehouse(int id, [FromQuery] int userId)
        {
            try
            {
                await _warehouseService.ArchiveAsync(id, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
