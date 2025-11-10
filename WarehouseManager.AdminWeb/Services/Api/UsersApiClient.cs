using WarehouseManager.Services;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Summary;
using WarehouseManagerContracts.DTOs.User;

namespace WarehouseManager.AdminWeb.Services.Api;

public class UsersApiClient : ApiClientBase
{
    public UsersApiClient(HttpClient httpClient) : base(httpClient)
    {
    }

    public Task<PagedResult<UserSummary>> GetPagedAsync(UserFilter filter, CancellationToken cancellationToken = default) =>
        GetAsync<PagedResult<UserSummary>>("api/users", filter, cancellationToken);

    public Task<UserSummary> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        GetAsync<UserSummary>($"api/users/{id}", cancellationToken: cancellationToken);

    public Task<UserSummary> CreateAsync(CreateUserCommand command, CancellationToken cancellationToken = default) =>
        PostAsync<CreateUserCommand, UserSummary>("api/users", command, cancellationToken);

    public Task<UserSummary> UpdateAsync(int id, UpdateUserCommand command, CancellationToken cancellationToken = default)
    {
        var url = $"api/users/{id}";
        return PutAsync<UpdateUserCommand, UserSummary>(url, command, cancellationToken);
    }

    public Task ArchiveAsync(int id, int currentUserId, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/users/{id}", new { currentUserId }, cancellationToken);
}

