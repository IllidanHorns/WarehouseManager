/*using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using WarehouseManagerApi.Data;
using WarehouseManagerApi.Models;
using WarehouseManagerApi.Services.DTOs;
using WarehouseManagerApi.Services.Interface;

namespace WarehouseManagerApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<LoginResult> CheckUserByEmailAsync(string email, string password) 
        {
            User user = await _context.Users.FirstOrDefaultAsync(user => user.Email == email);
            if (user == null) 
            {
                return new LoginResult(false, null, "Неправильный email или пароль");
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) 
            {
                return new LoginResult(false, null, "Неправильный email или пароль");
            }

            return new LoginResult(true, user.UserId, "Успешная авторизация");

            
        }
    }
}
*/