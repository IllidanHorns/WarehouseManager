using WarehouseManagerContracts.DTOs.Order;

namespace WarehouseManagerApi.Services.Interface
{
        public interface IOrderService
        {
            Task<IEnumerable<OrderDto>> GetAllAsync(bool includeArchived = false);
            Task<OrderDto?> GetByIdAsync(int id);
            Task<OrderDto> CreateAsync(CreateOrderDto dto);
            Task<OrderDto?> UpdateStatusAsync(int id, int newStatusId);
            Task<bool> DeleteAsync(int id);
            Task<OrderDto?> AssignEmployeeAsync(int orderId, int? employeeId);
        }
}
