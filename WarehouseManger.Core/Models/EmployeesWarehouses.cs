using WarehouseManger.Core.Models.Interfaces;

namespace WarehouseManager.Core.Models
{
    public class EmployeesWarehouses : IEntity, IArchivable
    {
        public int Id { get; set;}
        public bool IsArchived { get; set; }
        public DateTime CreationDateTime { get; set; }
        public DateTime UpdateDateTime { get; set; }

        public int WarehouseId { get; set; }
        public int EmployeeId { get; set; }

        public Warehouse Warehouse { get; set; }
        public Employee Employee { get; set; }
    }
}
