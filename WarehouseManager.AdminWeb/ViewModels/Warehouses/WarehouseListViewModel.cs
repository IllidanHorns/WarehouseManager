using WarehouseManager.Services;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Summary;

namespace WarehouseManager.AdminWeb.ViewModels.Warehouses;

public class WarehouseListViewModel
{
    public WarehouseFilter Filter { get; set; } = new();

    public PagedResult<WarehouseSummary>? Result { get; set; }

    public string? ErrorMessage { get; set; }
}

