using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManager.AdminWeb.Services.Api;
using WarehouseManager.AdminWeb.ViewModels.Account;
using WarehouseManagerContracts.DTOs.Auth;

namespace WarehouseManager.AdminWeb.Controllers;

public class AccountController : Controller
{
    private readonly AuthApiClient _authApiClient;

    public AccountController(AuthApiClient authApiClient)
    {
        _authApiClient = authApiClient;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        var command = new LoginCommand
        {
            Email = model.Email.Trim(),
            Password = model.Password
        };

        try
        {
            var user = await _authApiClient.LoginAsync(command, cancellationToken);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new(ClaimTypes.Name, BuildFullName(user)),
                new(ClaimTypes.Role, user.RoleName ?? string.Empty)
            };

            if (!string.IsNullOrWhiteSpace(user.RoleName))
            {
                claims.Add(new Claim("roleName", user.RoleName));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
        catch (ApiException ex)
        {
            foreach (var error in ex.GetValidationErrors())
            {
                ModelState.AddModelError(error.Key, error.Value);
            }

            ModelState.AddModelError(string.Empty, ex.Message);
            model.ErrorMessage = ex.Message;
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private static string BuildFullName(WarehouseManagerContracts.DTOs.User.UserDto user)
    {
        var parts = new[]
        {
            user.FirstName,
            user.MiddleName,
            user.Patronymic
        };

        var name = string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p))).Trim();
        return string.IsNullOrWhiteSpace(name) ? user.Email ?? string.Empty : name;
    }
}

