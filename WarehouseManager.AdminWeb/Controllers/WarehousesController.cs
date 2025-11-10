using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManager.AdminWeb.Services.Api;
using WarehouseManager.AdminWeb.ViewModels.Warehouses;
using WarehouseManager.Services.Filters;
using WarehouseManagerContracts.DTOs.Warehouse;

namespace WarehouseManager.AdminWeb.Controllers;

[Authorize(Roles = "Администратор,Менеджер склада")]
public class WarehousesController : Controller
{
    private readonly WarehousesApiClient _warehousesApiClient;
    private readonly ILogger<WarehousesController> _logger;

    public WarehousesController(WarehousesApiClient warehousesApiClient, ILogger<WarehousesController> logger)
    {
        _warehousesApiClient = warehousesApiClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(WarehouseFilter filter, CancellationToken cancellationToken)
    {
        filter.Page = Math.Max(filter.Page, 1);
        filter.PageSize = filter.PageSize <= 0 ? 20 : filter.PageSize;

        var viewModel = new WarehouseListViewModel
        {
            Filter = filter
        };

        try
        {
            viewModel.Result = await _warehousesApiClient.GetPagedAsync(filter, cancellationToken);
        }
        catch (ApiException ex)
        {
            viewModel.ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка получения списка складов");
            viewModel.ErrorMessage = "Не удалось загрузить склады. Попробуйте повторить позднее.";
        }

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new WarehouseFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WarehouseFormViewModel model, CancellationToken cancellationToken)
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

        var command = new CreateWarehouseCommand
        {
            UserId = currentUserId.Value,
            Address = model.Address.Trim(),
            Square = model.Square
        };

        try
        {
            await _warehousesApiClient.CreateAsync(command, cancellationToken);
            TempData["SuccessMessage"] = "Склад успешно создан.";
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
            _logger.LogError(ex, "Ошибка создания склада");
            ModelState.AddModelError(string.Empty, "Не удалось создать склад. Попробуйте позже.");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        try
        {
            var warehouse = await _warehousesApiClient.GetByIdAsync(id, cancellationToken);
            var model = new WarehouseFormViewModel
            {
                Id = warehouse.Id,
                Address = warehouse.Address,
                Square = warehouse.Square
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
            _logger.LogError(ex, "Ошибка загрузки склада {WarehouseId}", id);
            TempData["ErrorMessage"] = "Не удалось загрузить данные склада.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, WarehouseFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            ModelState.AddModelError(string.Empty, "Некорректный идентификатор склада.");
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

        var command = new UpdateWarehouseCommand
        {
            Id = id,
            Address = model.Address.Trim(),
            Square = model.Square,
            UserId = currentUserId.Value
        };

        try
        {
            await _warehousesApiClient.UpdateAsync(id, command, cancellationToken);
            TempData["SuccessMessage"] = "Склад успешно обновлён.";
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
            _logger.LogError(ex, "Ошибка обновления склада {WarehouseId}", id);
            ModelState.AddModelError(string.Empty, "Не удалось обновить склад. Попробуйте позже.");
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
            await _warehousesApiClient.ArchiveAsync(id, currentUserId.Value, cancellationToken);
            TempData["SuccessMessage"] = "Склад архивирован.";
        }
        catch (ApiException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка архивирования склада {WarehouseId}", id);
            TempData["ErrorMessage"] = "Не удалось архивировать склад. Возможно, есть связанные активные сущности.";
        }

        return RedirectToAction(nameof(Index));
    }

    private int? GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(idClaim, out var userId))
        {
            return userId;
        }

        return null;
    }
}

