using WarehouseManger.Core.Models.Interfaces;

namespace WarehouseManager.Core.Models
{
    public class OrderStatus : IEntity, IArchivable
    {
        public int Id { get; set; }
        public string StatusName { get; set; }
        public DateTime CreationDatetime { get; set; }
        public DateTime UpdateDatetime { get; set; }
        public bool IsArchived { get; set; }

        public List<Order>? Orders { get; set; }
    }
}
