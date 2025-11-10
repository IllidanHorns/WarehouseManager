using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WarehouseManager.AdminWeb.ViewModels.Users;

public class UserFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Email обязателен.")]
    [EmailAddress(ErrorMessage = "Неверный формат email.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Имя обязательно.")]
    [Display(Name = "Имя")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Фамилия обязательна.")]
    [Display(Name = "Фамилия")]
    public string MiddleName { get; set; } = string.Empty;

    [Display(Name = "Отчество")]
    public string? Patronymic { get; set; }

    [Required(ErrorMessage = "Телефон обязателен.")]
    [Phone(ErrorMessage = "Неверный формат телефона.")]
    [Display(Name = "Телефон")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "Пароль")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Пароль должен содержать минимум 6 символов.")]
    public string? Password { get; set; }

    [Display(Name = "Новый пароль")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Пароль должен содержать минимум 6 символов.")]
    public string? NewPassword { get; set; }

    [Required(ErrorMessage = "Роль обязательна.")]
    [Display(Name = "Роль")]
    public int RoleId { get; set; }

    public List<SelectListItem> Roles { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public bool ShowPasswordField => !Id.HasValue;
}

