using Microsoft.AspNetCore.Mvc;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManagerContracts.DTOs.User;
using WarehouseManagerApi.Services;

namespace WarehouseManagerApi.Controllers
{
    /// <summary>
    /// Управляет пользователями системы.
    /// </summary>
    [Route("api/[controller]")]
    public class UsersController : ApiControllerBase
    {
        private readonly IUserService _userService;
        private readonly CustomMetricsService? _customMetricsService;

        public UsersController(IUserService userService, CustomMetricsService? customMetricsService = null)
        {
            _userService = userService;
            _customMetricsService = customMetricsService;
        }

        /// <summary>
        /// Возвращает пользователей по фильтру.
        /// </summary>
        /// <param name="filter">Параметры фильтра и пагинации.</param>
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] UserFilter filter)
        {
            var result = await _userService.GetPagedAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Возвращает пользователя по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор пользователя.</param>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUser(int id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Создаёт нового пользователя.
        /// </summary>
        /// <param name="command">Данные пользователя.</param>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
        {
            try
            {
                var created = await _userService.CreateAsync(command);
                
                if (_customMetricsService != null)
                {
                    await _customMetricsService.UpdateActiveUsersCountAsync();
                }
                
                return CreatedAtAction(nameof(GetUser), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Обновляет данные пользователя.
        /// </summary>
        /// <param name="id">Идентификатор пользователя.</param>
        /// <param name="command">Изменяемые данные.</param>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserCommand command)
        {
            try
            {
                var updateCommand = new UpdateUserCommand
                {
                    UserId = command.UserId,
                    TargetUserId = id,
                    Email = command.Email,
                    NewPassword = command.NewPassword,
                    FirstName = command.FirstName,
                    MiddleName = command.MiddleName,
                    Patronymic = command.Patronymic,
                    PhoneNumber = command.PhoneNumber,
                    RoleId = command.RoleId
                };

                var updated = await _userService.UpdateAsync(updateCommand);
                
                if (_customMetricsService != null)
                {
                    await _customMetricsService.UpdateActiveUsersCountAsync();
                }
                
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Архивирует пользователя.
        /// </summary>
        /// <param name="id">Идентификатор пользователя.</param>
        /// <param name="currentUserId">Пользователь, выполняющий операцию.</param>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> ArchiveUser(int id, [FromQuery] int currentUserId)
        {
            try
            {
                await _userService.ArchiveAsync(id, currentUserId);
                
                if (_customMetricsService != null)
                {
                    await _customMetricsService.UpdateActiveUsersCountAsync();
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
