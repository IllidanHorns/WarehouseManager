using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WarehouseManager.AdminWeb.Services.Api;
using WarehouseManager.AdminWeb.ViewModels.Products;
using WarehouseManager.Services.Filters;
using WarehouseManagerContracts.DTOs.Product;

namespace WarehouseManager.AdminWeb.Controllers;

[Authorize(Roles = "Администратор,Менеджер склада")]
public class ProductsController : Controller
{
    private readonly ProductsApiClient _productsApiClient;
    private readonly CategoriesApiClient _categoriesApiClient;
    private readonly WarehousesApiClient _warehousesApiClient;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        ProductsApiClient productsApiClient,
        CategoriesApiClient categoriesApiClient,
        WarehousesApiClient warehousesApiClient,
        ILogger<ProductsController> logger)
    {
        _productsApiClient = productsApiClient;
        _categoriesApiClient = categoriesApiClient;
        _warehousesApiClient = warehousesApiClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(ProductsFilters filter, CancellationToken cancellationToken)
    {
        filter.Page = Math.Max(1, filter.Page);
        filter.PageSize = filter.PageSize <= 0 ? 20 : filter.PageSize;

        var viewModel = new ProductListViewModel
        {
            Filter = filter
        };

        try
        {
            await PopulateCategoriesAsync(viewModel, cancellationToken);
            await PopulateWarehousesAsync(viewModel, cancellationToken);
            viewModel.Result = await _productsApiClient.GetPagedAsync(filter, cancellationToken);
        }
        catch (ApiException ex)
        {
            viewModel.ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки продуктов");
            viewModel.ErrorMessage = "Не удалось загрузить список продуктов. Попробуйте позже.";
        }

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new ProductFormViewModel();
        await PopulateCategoriesAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync(model, cancellationToken);
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            ModelState.AddModelError(string.Empty, "Не удалось определить текущего пользователя.");
            await PopulateCategoriesAsync(model, cancellationToken);
            return View(model);
        }

        var command = new CreateProductCommand
        {
            UserId = currentUserId.Value,
            ProductName = model.ProductName.Trim(),
            Price = model.Price,
            Weight = model.Weight,
            CategoryId = model.CategoryId
        };

        try
        {
            await _productsApiClient.CreateAsync(command, cancellationToken);
            TempData["SuccessMessage"] = "Продукт создан.";
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
            _logger.LogError(ex, "Ошибка создания продукта");
            ModelState.AddModelError(string.Empty, "Не удалось создать продукт. Попробуйте позже.");
        }

        await PopulateCategoriesAsync(model, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productsApiClient.GetByIdAsync(id, cancellationToken);
            var model = new ProductFormViewModel
            {
                Id = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                Weight = product.Weight,
                CategoryId = product.CategoryId
            };

            await PopulateCategoriesAsync(model, cancellationToken);
            return View(model);
        }
        catch (ApiException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки продукта {ProductId}", id);
            TempData["ErrorMessage"] = "Не удалось загрузить продукт.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            ModelState.AddModelError(string.Empty, "Некорректный идентификатор продукта.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync(model, cancellationToken);
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            ModelState.AddModelError(string.Empty, "Не удалось определить текущего пользователя.");
            await PopulateCategoriesAsync(model, cancellationToken);
            return View(model);
        }

        var command = new UpdateProductCommand
        {
            Id = id,
            ProductName = model.ProductName.Trim(),
            Price = model.Price,
            Weight = model.Weight,
            CategoryId = model.CategoryId,
            UserId = currentUserId.Value
        };

        try
        {
            await _productsApiClient.UpdateAsync(id, command, cancellationToken);
            TempData["SuccessMessage"] = "Продукт обновлён.";
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
            _logger.LogError(ex, "Ошибка обновления продукта {ProductId}", id);
            ModelState.AddModelError(string.Empty, "Не удалось обновить продукт. Попробуйте позже.");
        }

        await PopulateCategoriesAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(int id, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            TempData["ErrorMessage"] = "Не удалось определить текущего пользователя.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _productsApiClient.ArchiveAsync(id, currentUserId.Value, cancellationToken);
            TempData["SuccessMessage"] = "Продукт архивирован.";
        }
        catch (ApiException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка архивирования продукта {ProductId}", id);
            TempData["ErrorMessage"] = "Не удалось архивировать продукт.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateCategoriesAsync(ProductFormViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            var categories = await _categoriesApiClient.GetPagedAsync(new CategoryFilter
            {
                Page = 1,
                PageSize = 200,
                IncludeArchived = false
            }, cancellationToken);

            model.Categories = categories.Items
                .Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == model.CategoryId))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки категорий");
            model.ErrorMessage ??= "Не удалось загрузить список категорий.";
        }
    }

    private async Task PopulateCategoriesAsync(ProductListViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            var categories = await _categoriesApiClient.GetPagedAsync(new CategoryFilter
            {
                Page = 1,
                PageSize = 200,
                IncludeArchived = false
            }, cancellationToken);

            model.Categories = categories.Items
                .Select(c => new SelectListItem(c.Name, c.Id.ToString(), model.Filter.CategoryId == c.Id))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки категорий");
            model.ErrorMessage ??= "Не удалось загрузить список категорий.";
        }
    }

    private async Task PopulateWarehousesAsync(ProductListViewModel model, CancellationToken cancellationToken)
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
            _logger.LogError(ex, "Ошибка загрузки складов");
            model.ErrorMessage ??= "Не удалось загрузить список складов.";
        }
    }

    private int? GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idClaim, out var userId) ? userId : null;
    }
}
