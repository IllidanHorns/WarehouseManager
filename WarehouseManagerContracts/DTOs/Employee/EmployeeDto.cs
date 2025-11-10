using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManagerContracts.DTOs.Employee
{
    public class EmployeeDto
    {
        public int EmployeeId { get; set; }
        public decimal Salary { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public DateTime CreationDatetime { get; set; }
    }
}
