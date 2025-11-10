using WarehouseManager.Services;
using WarehouseManager.Services.Filters.Interfaces;
using WarehouseManager.Services.Summary;
using WarehouseManagerContracts.DTOs.Order;

namespace WarehouseManager.Services.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderSummary> CreateAsync(CreateOrderCommand command);
        Task<PagedResult<OrderSummary>> GetPagedAsync(IPaginationFilter filter);
        Task<OrderSummary> GetByIdAsync(int orderId);
        Task<OrderSummary> UpdateStatusAsync(UpdateOrderStatusCommand command);
        Task<OrderSummary> AssignEmployeeAsync(AssignEmployeeToOrderCommand command);
    }
}

