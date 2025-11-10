using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using WarehouseManagerContracts.DTOs.Auth;
using WarehouseManager.Core.Data;
using WarehouseManager.Core.Models;
using WarehouseManager.Services.Exceptions;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManagerContracts.Validation.Auth;

namespace WarehouseManager.Application.Services;


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
            throw new ModelValidationException(result.Errors.ToList());

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == command.Email);

        if (user == null)
            throw new InvalidCredentialsException("Неверный email или пароль.");

        if (user.IsArchived)
            throw new InvalidCredentialsException("Учётная запись деактивирована.");

        if (!BCrypt.Net.BCrypt.Verify(command.Password, user.PasswordHash))
            throw new InvalidCredentialsException("Неверный email или пароль.");

        return user;
    }
}