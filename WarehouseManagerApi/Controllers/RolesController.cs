using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseManager.Core.Data;
using WarehouseManagerContracts.DTOs.Role;

namespace WarehouseManagerApi.Controllers;

/// <summary>
/// Возвращает доступные роли пользователей.
/// </summary>
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly AppDbContext _context;

    public RolesController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получает список ролей.
    /// </summary>
    /// <param name="includeArchived">Признак включения архивных ролей.</param>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles([FromQuery] bool includeArchived = false)
    {
        var query = _context.Roles.AsNoTracking();

        if (!includeArchived)
        {
            query = query.Where(r => !r.IsArchived);
        }

        var roles = await query
            .OrderBy(r => r.RoleName)
            .Select(r => new RoleDto
            {
                RoleId = r.Id,
                RoleName = r.RoleName,
                CreationDatetime = r.CreationDatetime
            })
            .ToListAsync();

        return Ok(roles);
    }
}

