using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WarehouseManager.AdminWeb.ViewModels.Employees;

public class EmployeeFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Сотрудник должен быть привязан к пользователю.")]
    [Display(Name = "Пользователь")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Укажите оклад.")]
    [Range(0, double.MaxValue, ErrorMessage = "Оклад не может быть отрицательным.")]
    [Display(Name = "Оклад")]
    public decimal Salary { get; set; }

    [Required(ErrorMessage = "Дата рождения обязательна.")]
    [DataType(DataType.Date)]
    [Display(Name = "Дата рождения")]
    public DateOnly DateOfBirth { get; set; }

    public List<SelectListItem> AvailableUsers { get; set; } = new();

    public string? ErrorMessage { get; set; }
}

