using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WarehouseManagerContracts.DTOs.OrderStatus;
using WarehouseManager.Tests.Infrastructure;

namespace WarehouseManager.Tests.Integration;

/// <summary>
/// Интеграционные проверки публичного каталога статусов заказов.
/// </summary>
[TestClass]
public class OrderStatusesEndpointTests
{
    private readonly TestApplicationFactory _factory = new();

    /// <summary>
    /// Убеждаемся, что GET /api/OrderStatuses возвращает seeded данные без авторизации.
    /// </summary>
    [TestMethod]
    public async Task GetOrderStatuses_ReturnsNonEmptyList()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/OrderStatuses");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<List<OrderStatusDto>>();
        Assert.IsNotNull(payload);
        Assert.IsTrue(payload.Count > 0);
    }
}

