using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManagerContracts.DTOs.Order;
using WarehouseManagerContracts.DTOs.OrderProduct;

namespace WarehouseManagerContracts.Validation.Order
{
    public class CreateOrderDtoValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderDtoValidator()
        {
            RuleFor(x => x.WarehouseId)
                .GreaterThan(0).WithMessage("Необходимо указать склад.");

            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("Необходимо указать пользователя.");

            RuleFor(x => x.Products)
                .NotEmpty().WithMessage("Нужно добавить хотя бы один товар.")
                .Must(HasValidProducts).WithMessage("У каждого товара должны быть валидные данные.");
        }

        private bool HasValidProducts(List<CreateOrderProductDto>? products)
        {
            return products != null && products.Count > 0 &&
                   products.All(p => p.ProductId > 0 && p.Quantity > 0 && p.OrderPrice >= 0);
        }
    }
}
