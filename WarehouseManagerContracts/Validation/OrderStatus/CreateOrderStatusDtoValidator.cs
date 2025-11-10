using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManagerContracts.DTOs.OrderStatus;

namespace WarehouseManagerContracts.Validation.OrderStatus
{
    public class CreateOrderStatusDtoValidator : AbstractValidator<CreateOrderStatusDto>
    {
        public CreateOrderStatusDtoValidator()
        {
            RuleFor(x => x.StatusName)
                .NotEmpty().WithMessage("Status name is required.")
                .MaximumLength(255).WithMessage("Status name must not exceed 255 characters.");
        }
    }
}
