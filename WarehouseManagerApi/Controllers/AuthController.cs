using Microsoft.AspNetCore.Mvc;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManagerContracts.DTOs.Auth;
using WarehouseManagerContracts.DTOs.User;

namespace WarehouseManagerApi.Controllers
{
    /// <summary>
    /// Предоставляет операции аутентификации пользователей.
    /// </summary>
    [Route("api/[controller]")]
    public class AuthController : ApiControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Выполняет вход по логину и паролю.
        /// </summary>
        /// <param name="command">Учётные данные пользователя.</param>
        /// <returns>Информация о пользователе при успешном входе.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            try
            {
                var user = await _authService.AuthenticateAsync(command);

                var dto = new UserDto
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    MiddleName = user.MiddleName,
                    Patronymic = user.Patronymic,
                    PhoneNumber = user.PhoneNumber,
                    RoleId = user.RoleId,
                    RoleName = user.Role?.RoleName ?? string.Empty,
                    CreationDatetime = user.CreationDatetime
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}