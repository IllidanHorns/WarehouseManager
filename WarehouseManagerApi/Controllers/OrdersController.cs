using System;
using Microsoft.AspNetCore.Mvc;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManagerContracts.DTOs.Order;
using WarehouseManagerApi.Services;

namespace WarehouseManagerApi.Controllers
{
    /// <summary>
    /// Управляет заказами клиентов.
    /// </summary>
    [Route("api/[controller]")]
    public class OrdersController : ApiControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly CustomMetricsService? _customMetricsService;

        public OrdersController(IOrderService orderService, CustomMetricsService? customMetricsService = null)
        {
            _orderService = orderService;
            _customMetricsService = customMetricsService;
        }

        /// <summary>
        /// Возвращает список заказов с фильтрами.
        /// </summary>
        /// <param name="filter">Фильтр поиска заказов.</param>
        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] OrderFilter filter)
        {
            var result = await _orderService.GetPagedAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Возвращает заказ по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор заказа.</param>
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

        /// <summary>
        /// Создаёт новый заказ.
        /// </summary>
        /// <param name="command">Данные заказа.</param>
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
        {
            try
            {
                var created = await _orderService.CreateAsync(command);
                
                if (_customMetricsService != null)
                {
                    await _customMetricsService.IncrementOrdersCreatedAsync();
                    await _customMetricsService.UpdateTotalProductsInWarehousesAsync();
                }
                
                return CreatedAtAction(nameof(GetOrders), new { page = 1 }, created);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Обновляет статус заказа.
        /// </summary>
        /// <param name="id">Идентификатор заказа.</param>
        /// <param name="command">Запрос на изменение статуса.</param>
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

        /// <summary>
        /// Назначает сотрудника ответственным за заказ.
        /// </summary>
        /// <param name="id">Идентификатор заказа.</param>
        /// <param name="command">Данные о назначении.</param>
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