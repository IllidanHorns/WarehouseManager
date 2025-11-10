using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WarehouseManager.AdminWeb.ViewModels.Products;

public class ProductFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Название обязательно.")]
    [Display(Name = "Название")]
    public string ProductName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Цена обязательна.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Цена должна быть положительной.")]
    [Display(Name = "Цена")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Вес обязателен.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Вес должен быть положительным.")]
    [Display(Name = "Вес")]
    public decimal Weight { get; set; }

    [Required(ErrorMessage = "Категория обязательна.")]
    [Display(Name = "Категория")]
    public int CategoryId { get; set; }

    public List<SelectListItem> Categories { get; set; } = new();

    public string? ErrorMessage { get; set; }
}
