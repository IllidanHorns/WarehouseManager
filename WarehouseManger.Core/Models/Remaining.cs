
using WarehouseManger.Core.Models.Interfaces;

namespace WarehouseManager.Core.Models
{
    public class Remaining : IEntity, IArchivable
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public DateTime CreationDatetime { get; set; }
        public DateTime UpdateDatetime { get; set; }
        public bool IsArchived { get; set; }

        public int WarehouseId { get; set; }
        public int ProductId { get; set; }

        public Product Product { get; set; }
        public Warehouse Warehouse { get; set; }
    }
}
