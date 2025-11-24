using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WarehouseManager.Services;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Services.Summary;
using WarehouseManagerApi.Controllers;
using WarehouseManagerContracts.DTOs.Product;

namespace WarehouseManager.Tests.Unit;

/// <summary>
/// Набор модульных тестов для <see cref="ProductsController"/>.
/// Проверяет, что контроллер корректно взаимодействует с сервисом и возвращает ожидаемые ответы.
/// </summary>
[TestClass]
public class ProductsControllerTests
{
    private Mock<IProductService> _productServiceMock = null!;
    private ProductsController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _productServiceMock = new Mock<IProductService>();
        _controller = new ProductsController(_productServiceMock.Object);
    }

    /// <summary>
    /// Проверяет, что метод GetProducts возвращает Ok с результатом сервиса.
    /// </summary>
    [TestMethod]
    public async Task GetProducts_ReturnsOkResult()
    {
        var filter = new ProductsFilters();
        var expected = new PagedResult<ProductSummary>
        {
            Items = new[] { new ProductSummary { Id = 1, Name = "Test" } },
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };

        _productServiceMock.Setup(s => s.GetPagedAsync(filter)).ReturnsAsync(expected);

        var result = await _controller.GetProducts(filter) as OkObjectResult;

        Assert.IsNotNull(result);
        Assert.AreEqual(expected, result.Value);
    }

    /// <summary>
    /// Проверяет, что метод GetProduct возвращает данные товара по идентификатору.
    /// </summary>
    [TestMethod]
    public async Task GetProduct_ReturnsOk()
    {
        var summary = new ProductSummary { Id = 5, Name = "Keyboard" };
        _productServiceMock.Setup(s => s.GetByIdAsync(5)).ReturnsAsync(summary);

        var result = await _controller.GetProduct(5) as OkObjectResult;

        Assert.IsNotNull(result);
        Assert.AreEqual(summary, result.Value);
    }

    /// <summary>
    /// Убеждаемся, что метод CreateProduct возвращает CreatedAtAction с созданным товаром.
    /// </summary>
    [TestMethod]
    public async Task CreateProduct_ReturnsCreatedAtAction()
    {
        var command = new CreateProductCommand { ProductName = "Mouse", Price = 10, Weight = 1, CategoryId = 1, UserId = 1 };
        var created = new ProductSummary { Id = 10, Name = "Mouse" };
        _productServiceMock.Setup(s => s.CreateAsync(command)).ReturnsAsync(created);

        var result = await _controller.CreateProduct(command) as CreatedAtActionResult;

        Assert.IsNotNull(result);
        Assert.AreEqual(nameof(ProductsController.GetProduct), result!.ActionName);
        Assert.AreEqual(created, result.Value);
    }

    /// <summary>
    /// Проверяет, что метод UpdateProduct возвращает обновлённое представление товара.
    /// </summary>
    [TestMethod]
    public async Task UpdateProduct_ReturnsOk()
    {
        var command = new UpdateProductCommand { ProductName = "Updated", Price = 20, Weight = 2, CategoryId = 1, UserId = 1 };
        var updated = new ProductSummary { Id = 3, Name = "Updated" };
        _productServiceMock.Setup(s => s.UpdateAsync(It.IsAny<UpdateProductCommand>())).ReturnsAsync(updated);

        var result = await _controller.UpdateProduct(3, command) as OkObjectResult;

        Assert.IsNotNull(result);
        Assert.AreEqual(updated, result.Value);
    }

    /// <summary>
    /// Убеждаемся, что метод ArchiveProduct возвращает NoContent и вызывает сервис архивирования.
    /// </summary>
    [TestMethod]
    public async Task ArchiveProduct_ReturnsNoContent()
    {
        _productServiceMock.Setup(s => s.ArchiveAsync(2, 99)).ReturnsAsync(true);

        var result = await _controller.ArchiveProduct(2, 99) as NoContentResult;

        Assert.IsNotNull(result);
        _productServiceMock.Verify(s => s.ArchiveAsync(2, 99), Times.Once);
    }
}

