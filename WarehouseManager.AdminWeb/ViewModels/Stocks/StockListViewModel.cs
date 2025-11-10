using Microsoft.AspNetCore.Mvc.Rendering;
using WarehouseManager.Services;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Summary;

namespace WarehouseManager.AdminWeb.ViewModels.Stocks;

public class StockListViewModel
{
    public StockFilter Filter { get; set; } = new();

    public PagedResult<WarehouseStockSummary>? Result { get; set; }

    public List<SelectListItem> Products { get; set; } = new();

    public List<SelectListItem> Warehouses { get; set; } = new();

    public string? ErrorMessage { get; set; }
}
