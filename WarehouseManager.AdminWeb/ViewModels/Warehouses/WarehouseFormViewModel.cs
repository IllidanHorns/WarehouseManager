using System.ComponentModel.DataAnnotations;

namespace WarehouseManager.AdminWeb.ViewModels.Warehouses;

public class WarehouseFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Адрес обязателен.")]
    [MaxLength(500, ErrorMessage = "Адрес не может превышать 500 символов.")]
    [Display(Name = "Адрес склада")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Площадь обязательна.")]
    [Range(1, 1_000_000, ErrorMessage = "Площадь должна быть от 1 до 1 000 000.")]
    [Display(Name = "Площадь (м²)")]
    public int Square { get; set; }

    public string? ErrorMessage { get; set; }
}

