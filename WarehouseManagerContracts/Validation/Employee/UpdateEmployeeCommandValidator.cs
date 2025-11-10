using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManagerContracts.DTOs.Employee;

namespace WarehouseManager.Contracts.Validation.Employee
{
    public class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
    {
        public UpdateEmployeeCommandValidator()
        {
            RuleFor(x => x.Salary)
                .GreaterThan(0).WithMessage("Salary must be greater than 0.")
                .LessThanOrEqualTo(9_999_999_999.99m).WithMessage("Salary cannot exceed 9,999,999,999.99.");

            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth is required.")
                .Must(BeAtLeast16YearsOld).WithMessage("Employee must be at least 16 years old.");

            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("Valid User ID is required.");
        }

        private bool BeAtLeast16YearsOld(DateOnly dob)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            return dob.AddYears(16) <= today;
        }
    }
}
