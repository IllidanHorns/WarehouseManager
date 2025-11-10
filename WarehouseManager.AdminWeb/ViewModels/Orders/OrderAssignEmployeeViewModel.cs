using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using WarehouseManager.Services.Summary;

namespace WarehouseManager.AdminWeb.ViewModels.Orders;

public class OrderAssignEmployeeViewModel
{
    public int OrderId { get; set; }

    public OrderSummary? Order { get; set; }

    [Required(ErrorMessage = "Выберите сотрудника.")]
    [Display(Name = "Сотрудник")]
    public int EmployeeId { get; set; }

    public List<SelectListItem> Employees { get; set; } = new();

    public string? ErrorMessage { get; set; }
}
