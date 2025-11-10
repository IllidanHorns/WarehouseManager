using Microsoft.AspNetCore.Mvc;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManagerContracts.DTOs.EmployeeWarehouse;

namespace WarehouseManagerApi.Controllers
{
    [Route("api/[controller]")]
    public class EmployeeWarehousesController : ApiControllerBase
    {
        private readonly IEmployeeWarehouseService _employeeWarehouseService;

        public EmployeeWarehousesController(IEmployeeWarehouseService employeeWarehouseService)
        {
            _employeeWarehouseService = employeeWarehouseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAssignments([FromQuery] EmployeeWarehouseFilter filter)
        {
            var result = await _employeeWarehouseService.GetPagedAsync(filter);
            return Ok(result);
        }

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
