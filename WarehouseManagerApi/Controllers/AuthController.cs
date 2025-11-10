using Microsoft.AspNetCore.Mvc;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManagerContracts.DTOs.Auth;
using WarehouseManagerContracts.DTOs.User;

namespace WarehouseManagerApi.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : ApiControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

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