using WarehouseManager.Services;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Summary;
using WarehouseManagerContracts.DTOs.Warehouse;

namespace WarehouseManager.AdminWeb.Services.Api;

public class WarehousesApiClient : ApiClientBase
{
    public WarehousesApiClient(HttpClient httpClient) : base(httpClient)
    {
    }

    public Task<PagedResult<WarehouseSummary>> GetPagedAsync(WarehouseFilter filter, CancellationToken cancellationToken = default) =>
        GetAsync<PagedResult<WarehouseSummary>>("api/warehouses", filter, cancellationToken);

    public Task<WarehouseSummary> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        GetAsync<WarehouseSummary>($"api/warehouses/{id}", cancellationToken: cancellationToken);

    public Task<WarehouseSummary> CreateAsync(CreateWarehouseCommand command, CancellationToken cancellationToken = default) =>
        PostAsync<CreateWarehouseCommand, WarehouseSummary>("api/warehouses", command, cancellationToken);

    public Task<WarehouseSummary> UpdateAsync(int id, UpdateWarehouseCommand command, CancellationToken cancellationToken = default) =>
        PutAsync<UpdateWarehouseCommand, WarehouseSummary>($"api/warehouses/{id}", command, cancellationToken);

    public Task ArchiveAsync(int id, int userId, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/warehouses/{id}", new { userId }, cancellationToken);
}

