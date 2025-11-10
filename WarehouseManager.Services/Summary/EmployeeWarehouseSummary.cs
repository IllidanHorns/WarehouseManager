using System;
using WarehouseManager.Services.Summary.Interfaces;

namespace WarehouseManager.Services.Summary
{
    public class EmployeeWarehouseSummary : ISummary
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeFullName { get; set; } = default!;
        public string EmployeeEmail { get; set; } = default!;
        public int WarehouseId { get; set; }
        public string WarehouseAddress { get; set; } = default!;
        public DateTime CreationDateTime { get; set; }
        public bool IsArchived { get; set; }
    }
}

