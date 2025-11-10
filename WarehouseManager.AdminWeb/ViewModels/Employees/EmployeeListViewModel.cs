using Microsoft.AspNetCore.Mvc.Rendering;
using WarehouseManager.Services;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Summary;

namespace WarehouseManager.AdminWeb.ViewModels.Employees;

public class EmployeeListViewModel
{
    public EmployeeFilters Filter { get; set; } = new();

    public PagedResult<EmployeeSummary>? Result { get; set; }

    public List<SelectListItem> Warehouses { get; set; } = new();

    public string? ErrorMessage { get; set; }
}

