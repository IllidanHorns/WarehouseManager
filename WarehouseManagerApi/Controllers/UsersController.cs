using Microsoft.AspNetCore.Mvc;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManagerContracts.DTOs.User;

namespace WarehouseManagerApi.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : ApiControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] UserFilter filter)
        {
            var result = await _userService.GetPagedAsync(filter);
            return Ok(result);
        }

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

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
        {
            try
            {
                var created = await _userService.CreateAsync(command);
                return CreatedAtAction(nameof(GetUser), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

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
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> ArchiveUser(int id, [FromQuery] int currentUserId)
        {
            try
            {
                await _userService.ArchiveAsync(id, currentUserId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
