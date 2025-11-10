using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManager.AdminWeb.Services.Api;
using WarehouseManager.AdminWeb.ViewModels.Categories;
using WarehouseManager.Services.Filters;
using WarehouseManagerContracts.DTOs.Category;

namespace WarehouseManager.AdminWeb.Controllers;

[Authorize(Roles = "Администратор,Менеджер склада")]
public class CategoriesController : Controller
{
    private readonly CategoriesApiClient _categoriesApiClient;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(CategoriesApiClient categoriesApiClient, ILogger<CategoriesController> logger)
    {
        _categoriesApiClient = categoriesApiClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CategoryFilter filter, CancellationToken cancellationToken)
    {
        filter.Page = Math.Max(1, filter.Page);
        filter.PageSize = filter.PageSize <= 0 ? 20 : filter.PageSize;

        var viewModel = new CategoryListViewModel
        {
            Filter = filter
        };

        try
        {
            viewModel.Result = await _categoriesApiClient.GetPagedAsync(filter, cancellationToken);
        }
        catch (ApiException ex)
        {
            viewModel.ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки категорий");
            viewModel.ErrorMessage = "Не удалось загрузить данные. Попробуйте позже.";
        }

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CategoryFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel model, CancellationToken cancellationToken)
    {
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

        var command = new CreateCategoryCommand
        {
            UserId = currentUserId.Value,
            Name = model.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim()
        };

        try
        {
            await _categoriesApiClient.CreateAsync(command, cancellationToken);
            TempData["SuccessMessage"] = "Категория успешно создана.";
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
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка создания категории");
            ModelState.AddModelError(string.Empty, "Не удалось создать категорию. Попробуйте позже.");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        try
        {
            var category = await _categoriesApiClient.GetByIdAsync(id, cancellationToken);
            var model = new CategoryFormViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            return View(model);
        }
        catch (ApiException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки категории {CategoryId}", id);
            TempData["ErrorMessage"] = "Не удалось загрузить категорию.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            ModelState.AddModelError(string.Empty, "Некорректный идентификатор категории.");
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

        var command = new UpdateCategoryCommand
        {
            Id = id,
            UserId = currentUserId.Value,
            Name = model.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim()
        };

        try
        {
            await _categoriesApiClient.UpdateAsync(id, command, cancellationToken);
            TempData["SuccessMessage"] = "Категория успешно обновлена.";
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
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка обновления категории {CategoryId}", id);
            ModelState.AddModelError(string.Empty, "Не удалось обновить категорию. Попробуйте позже.");
            return View(model);
        }
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
            await _categoriesApiClient.ArchiveAsync(id, currentUserId.Value, cancellationToken);
            TempData["SuccessMessage"] = "Категория архивирована.";
        }
        catch (ApiException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка архивирования категории {CategoryId}", id);
            TempData["ErrorMessage"] = "Не удалось архивировать категорию. Возможно, есть связанные активные продукты.";
        }

        return RedirectToAction(nameof(Index));
    }

    private int? GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idClaim, out var userId) ? userId : null;
    }
}

