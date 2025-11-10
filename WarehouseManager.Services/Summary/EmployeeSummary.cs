using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManager.Services.Summary.Interfaces;

namespace WarehouseManager.Services.Summary
{
    public class EmployeeSummary : ISummary
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public decimal Salary { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public bool IsArchived { get; set; }
        public DateTime CreationDatetime { get; set; }
        public int AssignedWarehouseCount { get; set; }
        public int AssignedOrderCount { get; set; }
    }
}
