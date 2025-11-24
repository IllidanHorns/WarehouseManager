using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WarehouseManagerContracts.DTOs.Role;
using WarehouseManager.Tests.Infrastructure;

namespace WarehouseManager.Tests.Integration;

/// <summary>
/// Интеграционные проверки справочника ролей.
/// </summary>
[TestClass]
public class RolesEndpointTests
{
    private readonly TestApplicationFactory _factory = new();

    /// <summary>
    /// Проверяет, что GET /api/Roles доступен и возвращает данные.
    /// </summary>
    [TestMethod]
    public async Task GetRoles_ReturnsNonEmptyList()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/Roles");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<List<RoleDto>>();
        Assert.IsNotNull(payload);
        Assert.IsTrue(payload.Count > 0);
    }
}

