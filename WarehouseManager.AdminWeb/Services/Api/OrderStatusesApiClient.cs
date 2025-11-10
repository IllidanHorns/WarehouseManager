using WarehouseManagerContracts.DTOs.OrderStatus;

namespace WarehouseManager.AdminWeb.Services.Api;

public class OrderStatusesApiClient : ApiClientBase
{
    public OrderStatusesApiClient(HttpClient httpClient) : base(httpClient)
    {
    }

    public Task<List<OrderStatusDto>> GetStatusesAsync(bool includeArchived = false, CancellationToken cancellationToken = default) =>
        GetAsync<List<OrderStatusDto>>("api/orderstatuses", new { includeArchived }, cancellationToken);
}

