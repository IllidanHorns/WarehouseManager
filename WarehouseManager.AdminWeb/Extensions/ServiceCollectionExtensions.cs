using Microsoft.Extensions.Options;
using WarehouseManager.AdminWeb.Configuration;
using WarehouseManager.AdminWeb.Services.Api;

namespace WarehouseManager.AdminWeb.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWarehouseApiClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ApiOptions>(configuration.GetSection(ApiOptions.SectionName));

        services.AddWarehouseHttpClient<AuthApiClient>();
        services.AddWarehouseHttpClient<RolesApiClient>();
        services.AddWarehouseHttpClient<UsersApiClient>();
        services.AddWarehouseHttpClient<EmployeesApiClient>();
        services.AddWarehouseHttpClient<WarehousesApiClient>();
        services.AddWarehouseHttpClient<CategoriesApiClient>();
        services.AddWarehouseHttpClient<ProductsApiClient>();
        services.AddWarehouseHttpClient<StocksApiClient>();
        services.AddWarehouseHttpClient<OrdersApiClient>();
        services.AddWarehouseHttpClient<OrderStatusesApiClient>();

        return services;
    }

    private static IServiceCollection AddWarehouseHttpClient<TClient>(this IServiceCollection services)
        where TClient : class
    {
        services.AddHttpClient<TClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ApiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        });

        return services;
    }
}

