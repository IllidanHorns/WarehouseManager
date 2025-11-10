using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManagerContracts.DTOs.Employee
{
    public class CreateEmployeeCommand
    {
        public decimal Salary { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public int UserId { get; set; }
        public int TargetUserId { get; set; }
    }
}
