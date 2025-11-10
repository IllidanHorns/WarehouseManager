using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseManager.Core.Data;
using WarehouseManagerContracts.DTOs.OrderStatus;

namespace WarehouseManagerApi.Controllers;

[Route("api/[controller]")]
public class OrderStatusesController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrderStatusesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderStatusDto>>> GetStatuses([FromQuery] bool includeArchived = false)
    {
        var query = _context.OrderStatuses.AsNoTracking();

        if (!includeArchived)
        {
            query = query.Where(s => !s.IsArchived);
        }

        var statuses = await query
            .OrderBy(s => s.StatusName)
            .Select(s => new OrderStatusDto
            {
                OrderStatusId = s.Id,
                StatusName = s.StatusName,
                CreationDatetime = s.CreationDatetime
            })
            .ToListAsync();

        return Ok(statuses);
    }
}

