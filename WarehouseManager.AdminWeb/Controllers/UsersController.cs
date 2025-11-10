using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WarehouseManager.AdminWeb.Services.Api;
using WarehouseManager.AdminWeb.ViewModels.Users;
using WarehouseManager.Services.Filters;
using WarehouseManagerContracts.DTOs.User;

namespace WarehouseManager.AdminWeb.Controllers;

[Authorize(Roles = "Администратор")]
public class UsersController : Controller
{
    private readonly UsersApiClient _usersApiClient;
    private readonly RolesApiClient _rolesApiClient;
    private readonly ILogger<UsersController> _logger;

    public UsersController(UsersApiClient usersApiClient, RolesApiClient rolesApiClient, ILogger<UsersController> logger)
    {
        _usersApiClient = usersApiClient;
        _rolesApiClient = rolesApiClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(UserFilter filter, CancellationToken cancellationToken)
    {
        filter.Page = Math.Max(1, filter.Page);
        filter.PageSize = filter.PageSize <= 0 ? 20 : filter.PageSize;

        var viewModel = new UserListViewModel
        {
            Filter = filter
        };

        try
        {
            var roles = await _rolesApiClient.GetRolesAsync(includeArchived: false, cancellationToken);
            viewModel.Roles = roles
                .Select(r => new SelectListItem(r.RoleName, r.RoleId.ToString()))
                .ToList();

            viewModel.Result = await _usersApiClient.GetPagedAsync(filter, cancellationToken);
        }
        catch (ApiException ex)
        {
            viewModel.ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки пользователей");
            viewModel.ErrorMessage = "Не удалось загрузить список пользователей. Попробуйте позже.";
        }

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new UserFormViewModel();
        await PopulateRolesAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateRolesAsync(model, cancellationToken);
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            ModelState.AddModelError(string.Empty, "Не удалось определить текущего пользователя.");
            await PopulateRolesAsync(model, cancellationToken);
            return View(model);
        }

        var command = new CreateUserCommand
        {
            UserId = currentUserId.Value,
            Email = model.Email.Trim(),
            Password = model.Password ?? string.Empty,
            FirstName = model.FirstName.Trim(),
            MiddleName = model.MiddleName.Trim(),
            Patronymic = string.IsNullOrWhiteSpace(model.Patronymic) ? null : model.Patronymic.Trim(),
            PhoneNumber = model.PhoneNumber.Trim(),
            RoleId = model.RoleId
        };

        try
        {
            await _usersApiClient.CreateAsync(command, cancellationToken);
            TempData["SuccessMessage"] = "Пользователь успешно создан.";
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
            _logger.LogError(ex, "Ошибка создания пользователя");
            ModelState.AddModelError(string.Empty, "Не удалось создать пользователя. Попробуйте позже.");
        }

        await PopulateRolesAsync(model, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _usersApiClient.GetByIdAsync(id, cancellationToken);
            var model = new UserFormViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                Patronymic = user.Patronymic,
                PhoneNumber = user.PhoneNumber,
                RoleId = user.RoleId
            };

            await PopulateRolesAsync(model, cancellationToken);
            return View(model);
        }
        catch (ApiException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки пользователя {UserId}", id);
            TempData["ErrorMessage"] = "Не удалось загрузить данные пользователя.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UserFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            ModelState.AddModelError(string.Empty, "Некорректный идентификатор пользователя.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateRolesAsync(model, cancellationToken);
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            ModelState.AddModelError(string.Empty, "Не удалось определить текущего пользователя.");
            await PopulateRolesAsync(model, cancellationToken);
            return View(model);
        }

        var command = new UpdateUserCommand
        {
            UserId = currentUserId.Value,
            TargetUserId = id,
            Email = model.Email.Trim(),
            FirstName = model.FirstName.Trim(),
            MiddleName = model.MiddleName.Trim(),
            Patronymic = string.IsNullOrWhiteSpace(model.Patronymic) ? null : model.Patronymic.Trim(),
            PhoneNumber = model.PhoneNumber.Trim(),
            RoleId = model.RoleId,
            NewPassword = string.IsNullOrWhiteSpace(model.NewPassword) ? null : model.NewPassword
        };

        try
        {
            await _usersApiClient.UpdateAsync(id, command, cancellationToken);
            TempData["SuccessMessage"] = "Пользователь обновлён.";
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
            _logger.LogError(ex, "Ошибка обновления пользователя {UserId}", id);
            ModelState.AddModelError(string.Empty, "Не удалось обновить пользователя. Попробуйте позже.");
        }

        await PopulateRolesAsync(model, cancellationToken);
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
            await _usersApiClient.ArchiveAsync(id, currentUserId.Value, cancellationToken);
            TempData["SuccessMessage"] = "Пользователь архивирован.";
        }
        catch (ApiException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка архивирования пользователя {UserId}", id);
            TempData["ErrorMessage"] = "Не удалось архивировать пользователя. Возможно, есть связанные активные заказы.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateRolesAsync(UserFormViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            var roles = await _rolesApiClient.GetRolesAsync(includeArchived: false, cancellationToken);
            model.Roles = roles
                .Select(r => new SelectListItem(r.RoleName, r.RoleId.ToString(), r.RoleId == model.RoleId))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки списка ролей");
            model.ErrorMessage = "Не удалось загрузить список ролей.";
        }
    }

    private int? GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idClaim, out var userId) ? userId : null;
    }
}

