using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManagerContracts.DTOs.Product;

namespace WarehouseManagerContracts.Validation.Product
{
    public class CreateProductDtoValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductDtoValidator()
        {
            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("Название продукта обязательно!")
                .MaximumLength(255).WithMessage("Product name must not exceed 255 characters.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.")
                .LessThanOrEqualTo(99_999_999.99m).WithMessage("Price cannot exceed 99,999,999.99.");

            RuleFor(x => x.Weight)
                .GreaterThan(0).WithMessage("Weight must be greater than 0.")
                .LessThanOrEqualTo(9999.99m).WithMessage("Weight cannot exceed 9,999.99 units.");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Valid Category ID is required.");

            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("Неизвестный пользователь!");
        }
    }
}
