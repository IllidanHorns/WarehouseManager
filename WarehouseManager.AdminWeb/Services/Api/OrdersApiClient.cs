using WarehouseManager.Services;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Summary;
using WarehouseManagerContracts.DTOs.Order;

namespace WarehouseManager.AdminWeb.Services.Api;

public class OrdersApiClient : ApiClientBase
{
    public OrdersApiClient(HttpClient httpClient) : base(httpClient)
    {
    }

    public Task<PagedResult<OrderSummary>> GetPagedAsync(OrderFilter filter, CancellationToken cancellationToken = default) =>
        GetAsync<PagedResult<OrderSummary>>("api/orders", filter, cancellationToken);

    public Task<OrderSummary> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        GetAsync<OrderSummary>($"api/orders/{id}", cancellationToken: cancellationToken);

    public Task<OrderSummary> UpdateStatusAsync(int id, UpdateOrderStatusCommand command, CancellationToken cancellationToken = default) =>
        PatchAsync<UpdateOrderStatusCommand, OrderSummary>($"api/orders/{id}/status", command, cancellationToken);

    public Task<OrderSummary> AssignEmployeeAsync(int id, AssignEmployeeToOrderCommand command, CancellationToken cancellationToken = default) =>
        PatchAsync<AssignEmployeeToOrderCommand, OrderSummary>($"api/orders/{id}/assign-employee", command, cancellationToken);
}
