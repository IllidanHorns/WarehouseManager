using System;
using Microsoft.AspNetCore.Mvc;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManagerContracts.DTOs.Employee;

namespace WarehouseManagerApi.Controllers
{
    [Route("api/[controller]")]
    public class EmployeesController : ApiControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployees([FromQuery] EmployeeFilters filter)
        {
            if (filter.Page <= 0)
            {
                filter.Page = 1;
            }

            if (filter.PageSize <= 0)
            {
                filter.PageSize = 20;
            }

            var result = await _employeeService.GetPagedAsync(filter);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeCommand command)
        {
            try
            {
                var created = await _employeeService.CreateAsync(command);
                return CreatedAtAction(nameof(GetEmployees), new { page = 1 }, created);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeCommand command)
        {
            try
            {
                var updateCommand = new UpdateEmployeeCommand
                {
                    UserId = command.UserId,
                    EmployeeId = id,
                    TargetUserId = command.TargetUserId,
                    Salary = command.Salary,
                    DateOfBirth = command.DateOfBirth
                };

                var updated = await _employeeService.UpdateAsync(updateCommand);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> ArchiveEmployee(int id, [FromQuery] int currentUserId)
        {
            try
            {
                await _employeeService.ArchiveAsync(id, currentUserId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
