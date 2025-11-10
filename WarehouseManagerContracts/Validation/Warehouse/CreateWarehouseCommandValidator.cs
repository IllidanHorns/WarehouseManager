using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManagerContracts.DTOs.Warehouse;

namespace WarehouseManagerContracts.Validation.Warehouse
{
    public class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
    {
        public CreateWarehouseCommandValidator()
        {
            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required.")
                .MaximumLength(1000).WithMessage("Address must not exceed 500 characters.");

            RuleFor(x => x.Square)
                .GreaterThan(0).WithMessage("Square must be greater than 0.")
                .LessThanOrEqualTo(1000000).WithMessage("Square cannot exceed 1 000 000 square units.");
        }
    }
}
