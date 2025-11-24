using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using FluentValidation.Results;
using WarehouseManager.Core.Data;
using WarehouseManager.Core.Models;
using WarehouseManager.Services.Exceptions;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManagerContracts.DTOs.Auth;
using WarehouseManagerContracts.Validation.Auth;

namespace WarehouseManager.Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User> AuthenticateAsync(LoginCommand command)
        {
            var validator = new LoginCommandValidator();
            var result = await validator.ValidateAsync(command);
            if (!result.IsValid)
            {
                throw new ModelValidationException(result.Errors.ToList());
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == command.Email && !u.IsArchived);

            if (user == null)
            {
                throw new InvalidCredentialsException("Неправильный email или пароль");
            }

            if (!BCrypt.Net.BCrypt.Verify(command.Password, user.PasswordHash))
            {
                throw new InvalidCredentialsException("Неправильный email или пароль");
            }

            return user;
        }
    }
}