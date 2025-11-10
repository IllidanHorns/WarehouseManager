using WarehouseManger.Core.Models.Interfaces;

namespace WarehouseManager.Core.Models
{
    public class Category : IEntity, IArchivable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreationDatetime { get; set; }
        public DateTime UpdateDatetime { get; set; }
        public bool IsArchived { get; set; }

        public List<Product>? Products { get; set; }
    }
}
    