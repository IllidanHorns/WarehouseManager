// WarehouseManagerContracts.Validation.Auth.LoginCommandValidator
using FluentValidation;
using WarehouseManagerContracts.DTOs.Auth;

namespace WarehouseManagerContracts.Validation.Auth
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email обязателен.");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Пароль обязателен.");
        }
    }
}