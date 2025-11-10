using WarehouseManager.Services;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Summary;

namespace WarehouseManager.AdminWeb.ViewModels.Categories;

public class CategoryListViewModel
{
    public CategoryFilter Filter { get; set; } = new();

    public PagedResult<CategorySummary>? Result { get; set; }

    public string? ErrorMessage { get; set; }
}

