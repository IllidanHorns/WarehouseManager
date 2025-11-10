using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WarehouseManager.AdminWeb.Services.Api;
using WarehouseManager.AdminWeb.ViewModels.Employees;
using WarehouseManager.Services.Filters;
using WarehouseManagerContracts.DTOs.Employee;

namespace WarehouseManager.AdminWeb.Controllers;

[Authorize(Roles = "Администратор")]
public class EmployeesController : Controller
{
    private readonly EmployeesApiClient _employeesApiClient;
    private readonly UsersApiClient _usersApiClient;
    private readonly WarehousesApiClient _warehousesApiClient;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(
        EmployeesApiClient employeesApiClient,
        UsersApiClient usersApiClient,
        WarehousesApiClient warehousesApiClient,
        ILogger<EmployeesController> logger)
    {
        _employeesApiClient = employeesApiClient;
        _usersApiClient = usersApiClient;
        _warehousesApiClient = warehousesApiClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(EmployeeFilters filter, CancellationToken cancellationToken)
    {
        filter.Page = Math.Max(1, filter.Page);
        filter.PageSize = filter.PageSize <= 0 ? 20 : filter.PageSize;

        var viewModel = new EmployeeListViewModel
        {
            Filter = filter
        };

        try
        {
            await PopulateWarehousesAsync(viewModel, cancellationToken);
            viewModel.Result = await _employeesApiClient.GetPagedAsync(filter, cancellationToken);
        }
        catch (ApiException ex)
        {
            viewModel.ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки сотрудников");
            viewModel.ErrorMessage = "Не удалось загрузить список сотрудников. Попробуйте позже.";
        }

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new EmployeeFormViewModel
        {
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-18))
        };

        await PopulateUsersAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateUsersAsync(model, cancellationToken);
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            ModelState.AddModelError(string.Empty, "Не удалось определить текущего пользователя.");
            await PopulateUsersAsync(model, cancellationToken);
            return View(model);
        }

        var command = new CreateEmployeeCommand
        {
            UserId = model.UserId,
            TargetUserId = currentUserId.Value,
            Salary = model.Salary,
            DateOfBirth = model.DateOfBirth
        };

        try
        {
            await _employeesApiClient.CreateAsync(command, cancellationToken);
            TempData["SuccessMessage"] = "Сотрудник создан.";
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
            _logger.LogError(ex, "Ошибка создания сотрудника");
            ModelState.AddModelError(string.Empty, "Не удалось создать сотрудника. Попробуйте позже.");
        }

        await PopulateUsersAsync(model, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        try
        {
            var employee = await _employeesApiClient.GetByIdAsync(id, cancellationToken);
            var model = new EmployeeFormViewModel
            {
                Id = employee.Id,
                UserId = employee.UserId,
                Salary = employee.Salary,
                DateOfBirth = employee.DateOfBirth
            };

            await PopulateUsersAsync(model, cancellationToken, employeeId: employee.Id);
            return View(model);
        }
        catch (ApiException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки сотрудника {EmployeeId}", id);
            TempData["ErrorMessage"] = "Не удалось загрузить данные сотрудника.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EmployeeFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            ModelState.AddModelError(string.Empty, "Некорректный идентификатор сотрудника.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateUsersAsync(model, cancellationToken, employeeId: id);
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            ModelState.AddModelError(string.Empty, "Не удалось определить текущего пользователя.");
            await PopulateUsersAsync(model, cancellationToken, employeeId: id);
            return View(model);
        }

        var command = new UpdateEmployeeCommand
        {
            EmployeeId = id,
            UserId = model.UserId,
            TargetUserId = currentUserId.Value,
            Salary = model.Salary,
            DateOfBirth = model.DateOfBirth
        };

        try
        {
            await _employeesApiClient.UpdateAsync(id, command, cancellationToken);
            TempData["SuccessMessage"] = "Сотрудник обновлён.";
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
            _logger.LogError(ex, "Ошибка обновления сотрудника {EmployeeId}", id);
            ModelState.AddModelError(string.Empty, "Не удалось обновить сотрудника. Попробуйте позже.");
        }

        await PopulateUsersAsync(model, cancellationToken, employeeId: id);
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
            await _employeesApiClient.ArchiveAsync(id, currentUserId.Value, cancellationToken);
            TempData["SuccessMessage"] = "Сотрудник архивирован.";
        }
        catch (ApiException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка архивирования сотрудника {EmployeeId}", id);
            TempData["ErrorMessage"] = "Не удалось архивировать сотрудника. Возможно, есть активные заказы или привязки.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateUsersAsync(EmployeeFormViewModel model, CancellationToken cancellationToken, int? employeeId = null)
    {
        try
        {
            var userFilter = new UserFilter
            {
                Page = 1,
                PageSize = 500,
                IncludeArchived = false
            };

            var users = await _usersApiClient.GetPagedAsync(userFilter, cancellationToken);

            var items = users.Items
                .Where(u => !u.HasEmployeeRecord || (employeeId.HasValue && u.Id == model.UserId))
                .Select(u => new SelectListItem($"{u.FirstName} {u.MiddleName} ({u.Email})", u.Id.ToString(), u.Id == model.UserId))
                .OrderBy(i => i.Text)
                .ToList();

            model.AvailableUsers = items;

            if (model.UserId == 0 && items.Any())
            {
                model.UserId = int.Parse(items.First().Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки доступных пользователей");
            model.ErrorMessage = "Не удалось загрузить список доступных пользователей.";
        }
    }

    private async Task PopulateWarehousesAsync(EmployeeListViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            var filter = new WarehouseFilter
            {
                Page = 1,
                PageSize = 200,
                IncludeArchived = false
            };

            var warehouses = await _warehousesApiClient.GetPagedAsync(filter, cancellationToken);

            model.Warehouses = warehouses.Items
                .Select(w => new SelectListItem(w.Address, w.Id.ToString()))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки складов для фильтра сотрудников");
            model.ErrorMessage = "Не удалось загрузить список складов.";
        }
    }

    private int? GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idClaim, out var userId) ? userId : null;
    }
}

