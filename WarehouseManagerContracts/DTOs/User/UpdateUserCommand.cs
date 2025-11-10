using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManagerContracts.DTOs.User
{
    public class UpdateUserCommand
    {
        public int UserId { get; init; } 
        public int TargetUserId { get; init; } 
        public string Email { get; init; }
        public string? NewPassword { get; init; } 
        public string FirstName { get; init; }
        public string MiddleName { get; init; }
        public string? Patronymic { get; init; }
        public string PhoneNumber { get; init; }
        public int RoleId { get; init; }
    }
}
