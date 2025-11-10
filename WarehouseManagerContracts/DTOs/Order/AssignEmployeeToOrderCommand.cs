namespace WarehouseManagerContracts.DTOs.Order
{
    public class AssignEmployeeToOrderCommand
    {
        public int OrderId { get; set; }
        public int EmployeeId { get; set; }
        public int? UserId { get; set; }
    }
}

