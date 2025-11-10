using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WarehouseManager.AdminWeb.Services.Api;
using WarehouseManager.AdminWeb.ViewModels.Stocks;
using WarehouseManager.Contracts.DTOs.Remaining;
using WarehouseManager.Services.Filters;

namespace WarehouseManager.AdminWeb.Controllers;

[Authorize(Roles = "Администратор,Менеджер склада")]
public class StocksController : Controller
{
    private readonly StocksApiClient _stocksApiClient;
    private readonly ProductsApiClient _productsApiClient;
    private readonly WarehousesApiClient _warehousesApiClient;
    private readonly ILogger<StocksController> _logger;

    public StocksController(
        StocksApiClient stocksApiClient,
        ProductsApiClient productsApiClient,
        WarehousesApiClient warehousesApiClient,
        ILogger<StocksController> logger)
    {
        _stocksApiClient = stocksApiClient;
        _productsApiClient = productsApiClient;
        _warehousesApiClient = warehousesApiClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(StockFilter filter, CancellationToken cancellationToken)
    {
        filter.Page = Math.Max(1, filter.Page);
        filter.PageSize = filter.PageSize <= 0 ? 20 : filter.PageSize;

        var viewModel = new StockListViewModel
        {
            Filter = filter
        };

        try
        {
            await PopulateProductsAsync(viewModel, cancellationToken);
            await PopulateWarehousesAsync(viewModel, cancellationToken);
            viewModel.Result = await _stocksApiClient.GetPagedAsync(filter, cancellationToken);
        }
        catch (ApiException ex)
        {
            viewModel.ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки остатков");
            viewModel.ErrorMessage = "Не удалось загрузить остатки. Попробуйте позже.";
        }

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new StockCreateViewModel();
        await PopulateProductsAsync(model, cancellationToken);
        await PopulateWarehousesAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StockCreateViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateProductsAsync(model, cancellationToken);
            await PopulateWarehousesAsync(model, cancellationToken);
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            ModelState.AddModelError(string.Empty, "Не удалось определить текущего пользователя.");
            await PopulateProductsAsync(model, cancellationToken);
            await PopulateWarehousesAsync(model, cancellationToken);
            return View(model);
        }

        var command = new CreateStockCommand
        {
            UserId = currentUserId.Value,
            ProductId = model.ProductId!.Value,
            WarehouseId = model.WarehouseId!.Value,
            Quantity = model.Quantity
        };

        try
        {
            await _stocksApiClient.CreateAsync(command, cancellationToken);
            TempData["SuccessMessage"] = "Остаток создан.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex)
        {
            foreach (var error in ex.GetValidationErrors())
            {
                ModelState.AddModelError(error.Key, error.Value);
            }

            ModelState.AddModelError(string.Empty, ex.Message);
            model.ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка создания остатка");
            ModelState.AddModelError(string.Empty, "Не удалось создать остаток. Попробуйте позже.");
        }

        await PopulateProductsAsync(model, cancellationToken);
        await PopulateWarehousesAsync(model, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        try
        {
            var stock = await _stocksApiClient.GetByIdAsync(id, cancellationToken);
            var model = new StockUpdateViewModel
            {
                Id = stock.Id,
                ProductName = stock.ProductName,
                WarehouseAddress = stock.WarehouseAddress,
                Quantity = stock.Quantity
            };

            return View(model);
        }
        catch (ApiException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки остатка {StockId}", id);
            TempData["ErrorMessage"] = "Не удалось загрузить остаток.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, StockUpdateViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            ModelState.AddModelError(string.Empty, "Некорректный идентификатор остатка.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            ModelState.AddModelError(string.Empty, "Не удалось определить текущего пользователя.");
            return View(model);
        }

        var command = new UpdateStockCommand
        {
            UserId = currentUserId.Value,
            RemainingId = id,
            NewQuantity = model.Quantity
        };

        try
        {
            await _stocksApiClient.UpdateAsync(id, command, cancellationToken);
            TempData["SuccessMessage"] = "Количество обновлено.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex)
        {
            foreach (var error in ex.GetValidationErrors())
            {
                ModelState.AddModelError(error.Key, error.Value);
            }

            ModelState.AddModelError(string.Empty, ex.Message);
            model.ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка обновления остатка {StockId}", id);
            ModelState.AddModelError(string.Empty, "Не удалось обновить остаток. Попробуйте позже.");
        }

        return View(model);
    }

    private async Task PopulateProductsAsync(StockListViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            var products = await _productsApiClient.GetPagedAsync(new ProductsFilters
            {
                Page = 1,
                PageSize = 500,
                IncludeArchived = false
            }, cancellationToken);

            model.Products = products.Items
                .Select(p => new SelectListItem(p.Name, p.Id.ToString(), model.Filter.ProductId == p.Id))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки списка продуктов");
            model.ErrorMessage ??= "Не удалось загрузить список продуктов.";
        }
    }

    private async Task PopulateWarehousesAsync(StockListViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            var warehouses = await _warehousesApiClient.GetPagedAsync(new WarehouseFilter
            {
                Page = 1,
                PageSize = 200,
                IncludeArchived = false
            }, cancellationToken);

            model.Warehouses = warehouses.Items
                .Select(w => new SelectListItem(w.Address, w.Id.ToString(), model.Filter.WarehouseId == w.Id))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки списка складов");
            model.ErrorMessage ??= "Не удалось загрузить список складов.";
        }
    }

    private async Task PopulateProductsAsync(StockCreateViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            var products = await _productsApiClient.GetPagedAsync(new ProductsFilters
            {
                Page = 1,
                PageSize = 500,
                IncludeArchived = false
            }, cancellationToken);

            model.Products = products.Items
                .Select(p => new SelectListItem(p.Name, p.Id.ToString(), model.ProductId == p.Id))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки списка продуктов");
            model.ErrorMessage ??= "Не удалось загрузить список продуктов.";
        }
    }

    private async Task PopulateWarehousesAsync(StockCreateViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            var warehouses = await _warehousesApiClient.GetPagedAsync(new WarehouseFilter
            {
                Page = 1,
                PageSize = 200,
                IncludeArchived = false
            }, cancellationToken);

            model.Warehouses = warehouses.Items
                .Select(w => new SelectListItem(w.Address, w.Id.ToString(), model.WarehouseId == w.Id))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки списка складов");
            model.ErrorMessage ??= "Не удалось загрузить список складов.";
        }
    }

    private int? GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idClaim, out var userId) ? userId : null;
    }
}
