
using WarehouseManger.Core.Models.Interfaces;

namespace WarehouseManager.Core.Models
{
    public class Product : IEntity, IArchivable
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public decimal Weight { get; set; }
        public DateTime CreationDatetime { get; set; }
        public DateTime UpdateDatetime { get; set; }
        public bool IsArchived { get; set; }

        public int CategoryId { get; set; }

        public Category Category { get; set; }
        public List<PriceHistory>? PriceHistories {get; set;}
        public List<OrdersProducts>? OrdersProducts { get; set; }
        public List<Remaining>? Remainings { get; set; }
    }
}
