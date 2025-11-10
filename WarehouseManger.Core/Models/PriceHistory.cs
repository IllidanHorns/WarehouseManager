

using WarehouseManger.Core.Models.Interfaces;

namespace WarehouseManager.Core.Models
{
    public class PriceHistory : IEntity, IArchivable
    {
        public int Id { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public DateTime UpdateDatetime { get; set; }
        public bool IsArchived { get; set; }
        public DateTime CreationDatetime { get; set; }

        public int ProductId { get; set; }

        public Product Product { get; set; }    
    }
}
