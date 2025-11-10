using WarehouseManager.Services;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Summary;
using WarehouseManagerContracts.DTOs.Category;

namespace WarehouseManager.AdminWeb.Services.Api;

public class CategoriesApiClient : ApiClientBase
{
    public CategoriesApiClient(HttpClient httpClient) : base(httpClient)
    {
    }

    public Task<PagedResult<CategorySummary>> GetPagedAsync(CategoryFilter filter, CancellationToken cancellationToken = default) =>
        GetAsync<PagedResult<CategorySummary>>("api/categories", filter, cancellationToken);

    public Task<CategorySummary> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        GetAsync<CategorySummary>($"api/categories/{id}", cancellationToken: cancellationToken);

    public Task<CategorySummary> CreateAsync(CreateCategoryCommand command, CancellationToken cancellationToken = default) =>
        PostAsync<CreateCategoryCommand, CategorySummary>("api/categories", command, cancellationToken);

    public Task<CategorySummary> UpdateAsync(int id, UpdateCategoryCommand command, CancellationToken cancellationToken = default) =>
        PutAsync<UpdateCategoryCommand, CategorySummary>($"api/categories/{id}", command, cancellationToken);

    public Task ArchiveAsync(int id, int userId, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/categories/{id}", new { userId }, cancellationToken);
}

