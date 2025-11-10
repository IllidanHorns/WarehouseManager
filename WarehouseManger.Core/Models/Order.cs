
using WarehouseManger.Core.Models.Interfaces;

namespace WarehouseManager.Core.Models
{
    public class Order : IEntity, IArchivable
    {
        public int Id { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreationDatetime { get; set; }
        public DateTime UpdateDatetime { get; set; }
        public bool IsArchived { get; set; }

        public int WarehouseId { get; set; }
        public int? EmployeeId { get; set; }
        public int UserId { get; set; }
        public int StatusId { get; set; }

        public Warehouse Warehouse { get; set; }
        public Employee? Employee { get; set; }
        public User User { get; set; }
        public OrderStatus? OrderStatus { get; set; }
        public List<OrdersProducts>? OrdersProducts { get; set; }
    }
}
