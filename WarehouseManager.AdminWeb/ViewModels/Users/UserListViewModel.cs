using Microsoft.AspNetCore.Mvc.Rendering;
using WarehouseManager.Services;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Summary;

namespace WarehouseManager.AdminWeb.ViewModels.Users;

public class UserListViewModel
{
    public UserFilter Filter { get; set; } = new();

    public PagedResult<UserSummary>? Result { get; set; }

    public List<SelectListItem> Roles { get; set; } = new();

    public string? ErrorMessage { get; set; }
}

