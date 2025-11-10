using WarehouseManagerContracts.DTOs.Role;

namespace WarehouseManager.AdminWeb.Services.Api;

public class RolesApiClient : ApiClientBase
{
    public RolesApiClient(HttpClient httpClient) : base(httpClient)
    {
    }

    public Task<List<RoleDto>> GetRolesAsync(bool includeArchived = false, CancellationToken cancellationToken = default)
        => GetAsync<List<RoleDto>>("api/roles", new { includeArchived }, cancellationToken);
}

