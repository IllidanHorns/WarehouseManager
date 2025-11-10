using System.ComponentModel.DataAnnotations;

namespace WarehouseManager.AdminWeb.ViewModels.Categories;

public class CategoryFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Название обязательно.")]
    [MaxLength(200, ErrorMessage = "Название не может превышать 200 символов.")]
    [Display(Name = "Название категории")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Описание")]
    [MaxLength(1000, ErrorMessage = "Описание не может превышать 1000 символов.")]
    public string? Description { get; set; }

    public string? ErrorMessage { get; set; }
}

