using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using WarehouseManagerApi.Services.DTOs;

namespace WarehouseManagerApi.Services.Interface
{
    public interface IAuthService
    {
        public Task<LoginResult> CheckUserByEmailAsync(string email, string password);
    }
}
