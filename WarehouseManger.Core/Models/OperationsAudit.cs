
using WarehouseManger.Core.Models.Interfaces;

namespace WarehouseManager.Core.Models
{
    public class OperationsAudit : IEntity
    {
        public int Id { get; set; }
        public DateTime OperationTime { get; set; }
        public string TableName { get; set; }
        public string OperationType { get; set; }
        public string RecordId { get; set; }
        public string OldData { get; set; }
        public string NewData { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }
    }
}
