using Microsoft.AspNetCore.Mvc;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManagerContracts.DTOs.Warehouse;

namespace WarehouseManagerApi.Controllers
{
    /// <summary>
    /// Управляет данными о складах.
    /// </summary>
    [Route("api/[controller]")]
    public class WarehousesController : ApiControllerBase
    {
        private readonly IWarehouseService _warehouseService;

        public WarehousesController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        /// <summary>
        /// Возвращает список складов.
        /// </summary>
        /// <param name="filter">Фильтр и пагинация.</param>
        [HttpGet]
        public async Task<IActionResult> GetWarehouses([FromQuery] WarehouseFilter filter)
        {
            var result = await _warehouseService.GetPagedAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Возвращает склад по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор склада.</param>
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

        /// <summary>
        /// Создаёт новый склад.
        /// </summary>
        /// <param name="command">Данные склада.</param>
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

        /// <summary>
        /// Обновляет сведения о складе.
        /// </summary>
        /// <param name="id">Идентификатор склада.</param>
        /// <param name="command">Обновлённые данные.</param>
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

        /// <summary>
        /// Архивирует склад.
        /// </summary>
        /// <param name="id">Идентификатор склада.</param>
        /// <param name="userId">Пользователь, выполняющий операцию.</param>
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
