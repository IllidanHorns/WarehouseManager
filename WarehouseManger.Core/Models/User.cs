
using WarehouseManger.Core.Models.Interfaces;

namespace WarehouseManager.Core.Models
{
    public class User : IEntity, IArchivable
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string? Patronymic { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreationDatetime { get; set; }
        public DateTime UpdateDatetime { get; set; }
        public bool IsArchived { get; set; }

        public int RoleId { get; set; }

        public Role Role { get; set; }
        public List<Order>? Orders {get; set;}
        public Employee? Employee { get; set; }

    }
}
