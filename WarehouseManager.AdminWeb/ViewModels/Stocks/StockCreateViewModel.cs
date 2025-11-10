using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WarehouseManager.AdminWeb.ViewModels.Stocks;

public class StockCreateViewModel
{
    [Required(ErrorMessage = "Выберите продукт.")]
    [Display(Name = "Продукт")]
    public int? ProductId { get; set; }

    [Required(ErrorMessage = "Выберите склад.")]
    [Display(Name = "Склад")]
    public int? WarehouseId { get; set; }

    [Required(ErrorMessage = "Количество обязательно.")]
    [Range(0, int.MaxValue, ErrorMessage = "Количество не может быть отрицательным.")]
    [Display(Name = "Количество")]
    public int Quantity { get; set; }

    public List<SelectListItem> Products { get; set; } = new();

    public List<SelectListItem> Warehouses { get; set; } = new();

    public string? ErrorMessage { get; set; }
}
