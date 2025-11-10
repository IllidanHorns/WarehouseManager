using WarehouseManager.Contracts.DTOs.Remaining;
using WarehouseManager.Services;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Summary;

namespace WarehouseManager.AdminWeb.Services.Api;

public class StocksApiClient : ApiClientBase
{
    public StocksApiClient(HttpClient httpClient) : base(httpClient)
    {
    }

    public Task<PagedResult<WarehouseStockSummary>> GetPagedAsync(StockFilter filter, CancellationToken cancellationToken = default) =>
        GetAsync<PagedResult<WarehouseStockSummary>>("api/stocks", filter, cancellationToken);

    public Task<WarehouseStockSummary> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        GetAsync<WarehouseStockSummary>($"api/stocks/{id}", cancellationToken: cancellationToken);

    public Task<WarehouseStockSummary> CreateAsync(CreateStockCommand command, CancellationToken cancellationToken = default) =>
        PostAsync<CreateStockCommand, WarehouseStockSummary>("api/stocks", command, cancellationToken);

    public Task<WarehouseStockSummary> UpdateAsync(int id, UpdateStockCommand command, CancellationToken cancellationToken = default) =>
        PutAsync<UpdateStockCommand, WarehouseStockSummary>($"api/stocks/{id}", command, cancellationToken);
}
