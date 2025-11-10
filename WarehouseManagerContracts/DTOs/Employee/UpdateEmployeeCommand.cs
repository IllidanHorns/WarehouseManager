using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManagerContracts.DTOs.Employee
{
    public class UpdateEmployeeCommand
    {
        public int UserId { get; init; }
        public int TargetUserId { get; set; }
        public int EmployeeId { get; init; }
        public decimal Salary { get; init; }
        public DateOnly DateOfBirth { get; init; }
    }
}
