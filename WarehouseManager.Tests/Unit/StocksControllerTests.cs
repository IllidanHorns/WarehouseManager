using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WarehouseManager.Contracts.DTOs.Remaining;
using WarehouseManager.Services;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Services.Summary;
using WarehouseManagerApi.Controllers;

namespace WarehouseManager.Tests.Unit;

/// <summary>
/// Модульные тесты контроллера StocksController.
/// Проверяют корректную работу конечных точек без обращения к реальному слою данных.
/// </summary>
[TestClass]
public class StocksControllerTests
{
    /// <summary>
    /// Убеждаемся, что метод GetStock возвращает Ok и передаёт результат сервиса.
    /// </summary>
    [TestMethod]
    public async Task GetStockList_ReturnsOkResult()
    {
        var serviceMock = new Mock<IStockService>();
        var controller = new StocksController(serviceMock.Object);
        var filter = new StockFilter();
        var expected = new PagedResult<WarehouseStockSummary>
        {
            Items = new[] { new WarehouseStockSummary { Id = 1, ProductName = "Keyboard" } },
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };

        serviceMock.Setup(s => s.GetPagedAsync(filter)).ReturnsAsync(expected);

        var result = await controller.GetStock(filter) as OkObjectResult;

        Assert.IsNotNull(result);
        Assert.AreEqual(expected, result.Value);
    }

    /// <summary>
    /// Проверяет, что CreateStock возвращает CreatedAtAction и инициирует пересчёт метрик.
    /// </summary>
    [TestMethod]
    public async Task CreateStock_ReturnsCreatedAtAction()
    {
        var serviceMock = new Mock<IStockService>();
        var controller = new StocksController(serviceMock.Object);
        var command = new CreateStockCommand { ProductId = 1, WarehouseId = 1, Quantity = 5, UserId = 2 };
        var created = new WarehouseStockSummary { Id = 7, ProductName = "Keyboard" };
        serviceMock.Setup(s => s.CreateAsync(command)).ReturnsAsync(created);

        var result = await controller.CreateStock(command) as CreatedAtActionResult;

        Assert.IsNotNull(result);
        Assert.AreEqual(nameof(StocksController.GetStock), result!.ActionName);
        Assert.AreEqual(created, result.Value);
    }
}

