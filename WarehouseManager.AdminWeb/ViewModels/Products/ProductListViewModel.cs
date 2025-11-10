using Microsoft.AspNetCore.Mvc.Rendering;
using WarehouseManager.Services;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Summary;

namespace WarehouseManager.AdminWeb.ViewModels.Products;

public class ProductListViewModel
{
    public ProductsFilters Filter { get; set; } = new();

    public PagedResult<ProductSummary>? Result { get; set; }

    public List<SelectListItem> Categories { get; set; } = new();

    public List<SelectListItem> Warehouses { get; set; } = new();

    public string? ErrorMessage { get; set; }
}
