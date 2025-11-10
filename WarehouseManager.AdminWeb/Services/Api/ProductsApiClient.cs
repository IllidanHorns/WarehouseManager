using WarehouseManager.Services;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Summary;
using WarehouseManagerContracts.DTOs.Product;

namespace WarehouseManager.AdminWeb.Services.Api;

public class ProductsApiClient : ApiClientBase
{
    public ProductsApiClient(HttpClient httpClient) : base(httpClient)
    {
    }

    public Task<PagedResult<ProductSummary>> GetPagedAsync(ProductsFilters filter, CancellationToken cancellationToken = default) =>
        GetAsync<PagedResult<ProductSummary>>("api/products", filter, cancellationToken);

    public Task<ProductSummary> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        GetAsync<ProductSummary>($"api/products/{id}", cancellationToken: cancellationToken);

    public Task<ProductSummary> CreateAsync(CreateProductCommand command, CancellationToken cancellationToken = default) =>
        PostAsync<CreateProductCommand, ProductSummary>("api/products", command, cancellationToken);

    public Task<ProductSummary> UpdateAsync(int id, UpdateProductCommand command, CancellationToken cancellationToken = default) =>
        PutAsync<UpdateProductCommand, ProductSummary>($"api/products/{id}", command, cancellationToken);

    public Task ArchiveAsync(int id, int userId, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/products/{id}", new { userId }, cancellationToken);
}
