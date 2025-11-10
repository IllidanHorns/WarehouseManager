
using System.ComponentModel;
using WarehouseManger.Core.Models.Interfaces;

namespace WarehouseManager.Core.Models
{
    public class Employee : IEntity, IArchivable
    {
        public int Id { get; set; }
        public decimal Salary { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public DateTime CreationDatetime { get; set; }
        public DateTime UpdateDatetime { get; set; }
        public bool IsArchived { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }
        
        public List<EmployeesWarehouses>? EmployeesWarehouses { get; set; }
        public List<Order> Orders { get; set; }
    }
}
