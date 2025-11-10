using Microsoft.AspNetCore.Mvc.Rendering;
using WarehouseManager.Services;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Summary;

namespace WarehouseManager.AdminWeb.ViewModels.Orders;

public class OrderListViewModel
{
    public OrderFilter Filter { get; set; } = new();

    public PagedResult<OrderSummary>? Result { get; set; }

    public List<SelectListItem> Warehouses { get; set; } = new();

    public List<SelectListItem> Employees { get; set; } = new();

    public List<SelectListItem> Statuses { get; set; } = new();

    public List<SelectListItem> Users { get; set; } = new();

    public string? ErrorMessage { get; set; }
}
