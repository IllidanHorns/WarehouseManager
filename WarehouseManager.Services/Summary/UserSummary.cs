using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManager.Services.Summary.Interfaces;

namespace WarehouseManager.Services.Summary
{
    public class UserSummary : ISummary
    {
        public int Id { get; set; }
        public string Email { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string MiddleName { get; set; } = default!;
        public string? Patronymic { get; set; }
        public string PhoneNumber { get; set; } = default!;
        public int RoleId { get; set; }
        public string RoleName { get; set; } = default!;
        public bool IsArchived { get; set; }
        public DateTime CreationDatetime { get; set; }
        public bool HasEmployeeRecord { get; set; }
    }
}
