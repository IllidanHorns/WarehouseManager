using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManager.Contracts.DTOs.Remaining;

namespace WarehouseManager.Contracts.Validation.Remaining
{
    public class UpdateStockCommandValidator : AbstractValidator<UpdateStockCommand>
    {
        public UpdateStockCommandValidator()
        {
            RuleFor(x => x.UserId).GreaterThan(0).WithMessage("Неизвестный пользователь");
            RuleFor(x => x.RemainingId).GreaterThan(0).WithMessage("Неизвестный остаток");
            RuleFor(x => x.NewQuantity).GreaterThanOrEqualTo(0).WithMessage("Количество должно быть больше или равно 0");
            RuleFor(x => x.NewQuantity).LessThanOrEqualTo(100000).WithMessage("Количество должно быть меньше 100 тысяч!");
        }
    }
}
