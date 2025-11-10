

using WarehouseManger.Core.Models.Interfaces;

namespace WarehouseManager.Core.Models
{
    public class OrdersProducts : IEntity
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal OrderPrice { get; set; }
        public bool IsArchived { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime UpdateDatetime { get; set; }
        public DateTime CreationDatetime { get; set; }

        public int OrderId { get; set; }
        public int ProductId { get; set; }

        public Order Order { get; set; }
        public Product Product { get; set; }
    }
}
