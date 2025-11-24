using Microsoft.AspNetCore.Mvc;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManagerContracts.DTOs.EmployeeWarehouse;

namespace WarehouseManagerApi.Controllers
{
    /// <summary>
    /// Управляет назначениями сотрудников на склады.
    /// </summary>
    [Route("api/[controller]")]
    public class EmployeeWarehousesController : ApiControllerBase
    {
        private readonly IEmployeeWarehouseService _employeeWarehouseService;

        public EmployeeWarehousesController(IEmployeeWarehouseService employeeWarehouseService)
        {
            _employeeWarehouseService = employeeWarehouseService;
        }

        /// <summary>
        /// Возвращает список назначений сотрудников.
        /// </summary>
        /// <param name="filter">Фильтр назначений.</param>
        [HttpGet]
        public async Task<IActionResult> GetAssignments([FromQuery] EmployeeWarehouseFilter filter)
        {
            var result = await _employeeWarehouseService.GetPagedAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Создаёт новое назначение сотрудника на склад.
        /// </summary>
        /// <param name="command">Данные назначения.</param>
        [HttpPost]
        public async Task<IActionResult> CreateAssignment([FromBody] CreateEmployeeWarehouseCommand command)
        {
            try
            {
                var created = await _employeeWarehouseService.CreateAsync(command);
                return CreatedAtAction(nameof(GetAssignments), new { page = 1 }, created);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Архивирует назначение сотрудника.
        /// </summary>
        /// <param name="id">Идентификатор назначения.</param>
        /// <param name="currentUserId">Пользователь, выполняющий операцию.</param>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> ArchiveAssignment(int id, [FromQuery] int currentUserId)
        {
            try
            {
                await _employeeWarehouseService.ArchiveAsync(id, currentUserId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
