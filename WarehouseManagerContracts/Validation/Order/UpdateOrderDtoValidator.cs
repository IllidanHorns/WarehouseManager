using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManagerContracts.DTOs.Order;

namespace WarehouseManagerContracts.Validation.Order
{
    public class UpdateOrderDtoValidator : AbstractValidator<UpdateOrderDto>
    {
        public UpdateOrderDtoValidator()
        {
            RuleFor(x => x.TotalPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Total price must be non-negative.");

            RuleFor(x => x.WarehouseId)
                .GreaterThan(0).WithMessage("Valid Warehouse ID is required.");

            RuleFor(x => x.StatusId)
                .GreaterThan(0).WithMessage("Valid Order Status ID is required.");

                RuleFor(x => x.EmployeeId)
                    .GreaterThan(0).WithMessage("Valid Employee ID is required.");
        }
    }
}
