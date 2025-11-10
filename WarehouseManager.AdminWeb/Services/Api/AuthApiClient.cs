using WarehouseManager.AdminWeb.Configuration;
using WarehouseManagerContracts.DTOs.Auth;
using WarehouseManagerContracts.DTOs.User;

namespace WarehouseManager.AdminWeb.Services.Api;

public class AuthApiClient : ApiClientBase
{
    public AuthApiClient(HttpClient httpClient) : base(httpClient)
    {
    }

    public Task<UserDto> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default)
        => PostAsync<LoginCommand, UserDto>("api/auth/login", command, cancellationToken);
}

