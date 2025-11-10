using FluentValidation;
using WarehouseManagerContracts.DTOs.User;

namespace WarehouseManagerContracts.Validation.User;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен для заполнения.")
            .MinimumLength(6).WithMessage("Пароль должен содержать минимум 6 символов.")
            .MaximumLength(100).WithMessage("Пароль не должен превышать 100 символов.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(255).WithMessage("First name must not exceed 255 characters.");

        RuleFor(x => x.MiddleName)
            .NotEmpty().WithMessage("Middle name is required.")
            .MaximumLength(255).WithMessage("Middle name must not exceed 255 characters.");

        RuleFor(x => x.Patronymic)
            .MaximumLength(255).WithMessage("Patronymic must not exceed 255 characters.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .MaximumLength(255).WithMessage("Phone number must not exceed 255 characters.");

        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("Valid Role ID is required.");
    }
}