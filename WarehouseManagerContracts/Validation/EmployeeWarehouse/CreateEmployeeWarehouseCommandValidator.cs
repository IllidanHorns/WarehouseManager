using FluentValidation;
using WarehouseManagerContracts.DTOs.EmployeeWarehouse;

namespace WarehouseManagerContracts.Validation.EmployeeWarehouse
{
    public class CreateEmployeeWarehouseCommandValidator : AbstractValidator<CreateEmployeeWarehouseCommand>
    {
        public CreateEmployeeWarehouseCommandValidator()
        {
            RuleFor(x => x.EmployeeId)
                .GreaterThan(0).WithMessage("Необходимо выбрать сотрудника.");

            RuleFor(x => x.WarehouseId)
                .GreaterThan(0).WithMessage("Необходимо выбрать склад.");

            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("Пользователь не авторизован.");
        }
    }
}

