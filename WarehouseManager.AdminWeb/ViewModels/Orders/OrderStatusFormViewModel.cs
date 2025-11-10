using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using WarehouseManager.Services.Summary;

namespace WarehouseManager.AdminWeb.ViewModels.Orders;

public class OrderStatusFormViewModel
{
    public int OrderId { get; set; }

    public OrderSummary? Order { get; set; }

    [Required(ErrorMessage = "Выберите статус.")]
    [Display(Name = "Статус заказа")]
    public int StatusId { get; set; }

    public List<SelectListItem> Statuses { get; set; } = new();

    public string? ErrorMessage { get; set; }
}
