using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManagerContracts.DTOs.Auth
{
    public class LoginCommand
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
