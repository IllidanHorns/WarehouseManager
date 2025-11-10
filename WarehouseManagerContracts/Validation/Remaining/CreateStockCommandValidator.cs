using FluentValidation;
using WarehouseManager.Contracts.DTOs.Remaining;

namespace WarehouseManager.Contracts.Validation.Remaining
{
    public class CreateStockCommandValidator : AbstractValidator<CreateStockCommand>
    {
        public CreateStockCommandValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("Неизвестный пользователь");

            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Необходимо выбрать продукт");

            RuleFor(x => x.WarehouseId)
                .GreaterThan(0).WithMessage("Необходимо выбрать склад");

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("Количество должно быть больше или равно 0")
                .LessThanOrEqualTo(100000).WithMessage("Количество должно быть меньше 100 тысяч");
        }
    }
}

