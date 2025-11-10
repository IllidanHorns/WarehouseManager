

using WarehouseManger.Core.Models.Interfaces;

namespace WarehouseManager.Core.Models
{
    public class Role : IEntity, IArchivable
    {
        public int Id { get; set; }
        public string RoleName { get; set; }
        public DateTime CreationDatetime { get; set; }
        public DateTime UpdateDatetime { get; set; }
        public bool IsArchived { get; set; }

        public List<User>? Users { get; set; }
    }
}
