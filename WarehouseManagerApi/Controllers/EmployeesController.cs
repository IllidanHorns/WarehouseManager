using System;
using Microsoft.AspNetCore.Mvc;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManagerContracts.DTOs.Employee;

namespace WarehouseManagerApi.Controllers
{
    /// <summary>
    /// Управляет сотрудниками компании.
    /// </summary>
    [Route("api/[controller]")]
    public class EmployeesController : ApiControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        /// <summary>
        /// Возвращает сотрудников согласно фильтру.
        /// </summary>
        /// <param name="filter">Фильтр поиска и параметры пагинации.</param>
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

        /// <summary>
        /// Создаёт нового сотрудника.
        /// </summary>
        /// <param name="command">Данные сотрудника.</param>
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

        /// <summary>
        /// Обновляет данные сотрудника.
        /// </summary>
        /// <param name="id">Идентификатор сотрудника.</param>
        /// <param name="command">Изменяемые данные.</param>
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

        /// <summary>
        /// Архивирует сотрудника.
        /// </summary>
        /// <param name="id">Идентификатор сотрудника.</param>
        /// <param name="currentUserId">Пользователь, выполняющий операцию.</param>
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
