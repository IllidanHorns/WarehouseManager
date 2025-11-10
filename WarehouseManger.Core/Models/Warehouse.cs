using WarehouseManger.Core.Models.Interfaces;

namespace WarehouseManager.Core.Models
{
    public class Warehouse : IEntity, IArchivable
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public int Square { get; set; }
        public DateTime CreationDatetime { get; set; }
        public DateTime UpdateDatetime { get; set; }
        public bool IsArchived { get; set; }

        public List<Order>? Orders { get; set; }
        public List<Remaining>? Remaining { get; set; }
        public List<EmployeesWarehouses>? EmployeesWarehouses{get; set;}
    }
}
