using System.ComponentModel.DataAnnotations;

namespace WarehouseManager.AdminWeb.ViewModels.Stocks;

public class StockUpdateViewModel
{
    public int Id { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string WarehouseAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Количество обязательно.")]
    [Range(0, int.MaxValue, ErrorMessage = "Количество не может быть отрицательным.")]
    [Display(Name = "Количество")]
    public int Quantity { get; set; }

    public string? ErrorMessage { get; set; }
}
