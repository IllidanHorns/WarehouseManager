using System;
using Microsoft.AspNetCore.Mvc;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManagerContracts.DTOs.Order;

namespace WarehouseManagerApi.Controllers
{
    [Route("api/[controller]")]
    public class OrdersController : ApiControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] OrderFilter filter)
        {
            var result = await _orderService.GetPagedAsync(filter);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            try
            {
                var order = await _orderService.GetByIdAsync(id);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
        {
            try
            {
                var created = await _orderService.CreateAsync(command);
                return CreatedAtAction(nameof(GetOrders), new { page = 1 }, created);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusCommand command)
        {
            try
            {
                command.OrderId = id;
                var updated = await _orderService.UpdateStatusAsync(command);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPatch("{id:int}/assign-employee")]
        public async Task<IActionResult> AssignEmployee(int id, [FromBody] AssignEmployeeToOrderCommand command)
        {
            try
            {
                command.OrderId = id;
                var updated = await _orderService.AssignEmployeeAsync(command);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}