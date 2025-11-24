using System;
using WarehouseManagerContracts.DTOs.Role;

namespace WarehouseManagerContracts.DTOs.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public RoleDto Role { get; set; } = new();
    }
}

