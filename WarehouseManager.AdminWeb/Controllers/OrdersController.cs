using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WarehouseManager.AdminWeb.Services.Api;
using WarehouseManager.AdminWeb.ViewModels.Orders;
using WarehouseManager.Services.Filters;
using WarehouseManagerContracts.DTOs.Order;

namespace WarehouseManager.AdminWeb.Controllers;

[Authorize(Roles = "Администратор,Менеджер склада")]
public class OrdersController : Controller
{
    private readonly OrdersApiClient _ordersApiClient;
    private readonly WarehousesApiClient _warehousesApiClient;
    private readonly EmployeesApiClient _employeesApiClient;
    private readonly OrderStatusesApiClient _orderStatusesApiClient;
    private readonly UsersApiClient _usersApiClient;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        OrdersApiClient ordersApiClient,
        WarehousesApiClient warehousesApiClient,
        EmployeesApiClient employeesApiClient,
        OrderStatusesApiClient orderStatusesApiClient,
        UsersApiClient usersApiClient,
        ILogger<OrdersController> logger)
    {
        _ordersApiClient = ordersApiClient;
        _warehousesApiClient = warehousesApiClient;
        _employeesApiClient = employeesApiClient;
        _orderStatusesApiClient = orderStatusesApiClient;
        _usersApiClient = usersApiClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(OrderFilter filter, CancellationToken cancellationToken)
    {
        filter.Page = Math.Max(1, filter.Page);
        filter.PageSize = filter.PageSize <= 0 ? 20 : filter.PageSize;

        var viewModel = new OrderListViewModel
        {
            Filter = filter
        };

        try
        {
            await PopulateWarehousesAsync(viewModel, cancellationToken);
            await PopulateEmployeesAsync(viewModel, cancellationToken);
            await PopulateStatusesAsync(viewModel, cancellationToken);
            await PopulateUsersAsync(viewModel, cancellationToken);
            viewModel.Result = await _ordersApiClient.GetPagedAsync(filter, cancellationToken);
        }
        catch (ApiException ex)
        {
            viewModel.ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки заказов");
            viewModel.ErrorMessage = "Не удалось загрузить заказы. Попробуйте позже.";
        }

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> EditStatus(int id, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _ordersApiClient.GetByIdAsync(id, cancellationToken);
            var model = new OrderStatusFormViewModel
            {
                OrderId = order.Id,
                Order = order
            };

            await PopulateStatusesAsync(model, cancellationToken);
            return View(model);
        }
        catch (ApiException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки заказа {OrderId}", id);
            TempData["ErrorMessage"] = "Не удалось загрузить заказ.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditStatus(int id, OrderStatusFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.OrderId)
        {
            ModelState.AddModelError(string.Empty, "Некорректный идентификатор заказа.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateStatusesAsync(model, cancellationToken);
            return View(model);
        }

        var currentUserId = GetCurrentUserId();

        var command = new UpdateOrderStatusCommand
        {
            OrderId = id,
            StatusId = model.StatusId,
            UserId = currentUserId
        };

        try
        {
            await _ordersApiClient.UpdateStatusAsync(id, command, cancellationToken);
            TempData["SuccessMessage"] = "Статус заказа обновлён.";
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
            _logger.LogError(ex, "Ошибка обновления статуса заказа {OrderId}", id);
            ModelState.AddModelError(string.Empty, "Не удалось обновить статус. Попробуйте позже.");
        }

        await PopulateStatusesAsync(model, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> AssignEmployee(int id, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _ordersApiClient.GetByIdAsync(id, cancellationToken);
            var model = new OrderAssignEmployeeViewModel
            {
                OrderId = order.Id,
                Order = order
            };

            await PopulateEmployeesAsync(model, cancellationToken); // will set list
            return View(model);
        }
        catch (ApiException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки заказа {OrderId}", id);
            TempData["ErrorMessage"] = "Не удалось загрузить заказ.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignEmployee(int id, OrderAssignEmployeeViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.OrderId)
        {
            ModelState.AddModelError(string.Empty, "Некорректный идентификатор заказа.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateEmployeesAsync(model, cancellationToken);
            return View(model);
        }

        var currentUserId = GetCurrentUserId();

        var command = new AssignEmployeeToOrderCommand
        {
            OrderId = id,
            EmployeeId = model.EmployeeId,
            UserId = currentUserId
        };

        try
        {
            await _ordersApiClient.AssignEmployeeAsync(id, command, cancellationToken);
            TempData["SuccessMessage"] = "Сотрудник назначен.";
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
            _logger.LogError(ex, "Ошибка назначения сотрудника для заказа {OrderId}", id);
            ModelState.AddModelError(string.Empty, "Не удалось назначить сотрудника. Попробуйте позже.");
        }

        await PopulateEmployeesAsync(model, cancellationToken);
        return View(model);
    }

    private async Task PopulateWarehousesAsync(OrderListViewModel model, CancellationToken cancellationToken)
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
            _logger.LogError(ex, "Ошибка загрузки складов для фильтра заказов");
            model.ErrorMessage ??= "Не удалось загрузить список складов.";
        }
    }

    private async Task PopulateEmployeesAsync(OrderListViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            var employees = await _employeesApiClient.GetPagedAsync(new EmployeeFilters
            {
                Page = 1,
                PageSize = 200,
                IncludeArchived = false
            }, cancellationToken);

            model.Employees = employees.Items
                .Select(e => new SelectListItem(e.FullName, e.Id.ToString(), model.Filter.EmployeeId == e.Id))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки сотрудников для фильтра заказов");
            model.ErrorMessage ??= "Не удалось загрузить список сотрудников.";
        }
    }

    private async Task PopulateStatusesAsync(OrderListViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            var statuses = await _orderStatusesApiClient.GetStatusesAsync(false, cancellationToken);
            model.Statuses = statuses
                .Select(s => new SelectListItem(s.StatusName, s.OrderStatusId.ToString(), model.Filter.StatusId == s.OrderStatusId))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки статусов заказов");
            model.ErrorMessage ??= "Не удалось загрузить статусы заказов.";
        }
    }

    private async Task PopulateUsersAsync(OrderListViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            var users = await _usersApiClient.GetPagedAsync(new UserFilter
            {
                Page = 1,
                PageSize = 200,
                IncludeArchived = false
            }, cancellationToken);

            model.Users = users.Items
                .Select(u => new SelectListItem(u.Email, u.Id.ToString(), model.Filter.UserId == u.Id))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки пользователей для фильтра заказов");
            model.ErrorMessage ??= "Не удалось загрузить список пользователей.";
        }
    }

    private async Task PopulateStatusesAsync(OrderStatusFormViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            var statuses = await _orderStatusesApiClient.GetStatusesAsync(false, cancellationToken);
            model.Statuses = statuses
                .Select(s => new SelectListItem(s.StatusName, s.OrderStatusId.ToString(), model.StatusId == s.OrderStatusId))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки статусов заказов");
            model.ErrorMessage ??= "Не удалось загрузить список статусов.";
        }
    }

    private async Task PopulateEmployeesAsync(OrderAssignEmployeeViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            var employees = await _employeesApiClient.GetPagedAsync(new EmployeeFilters
            {
                Page = 1,
                PageSize = 200,
                IncludeArchived = false
            }, cancellationToken);

            model.Employees = employees.Items
                .Select(e => new SelectListItem(e.FullName, e.Id.ToString(), model.EmployeeId == e.Id))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки сотрудников");
            model.ErrorMessage ??= "Не удалось загрузить список сотрудников.";
        }
    }

    private int? GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idClaim, out var userId) ? userId : null;
    }
}
