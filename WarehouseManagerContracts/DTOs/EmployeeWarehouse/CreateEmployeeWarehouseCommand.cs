namespace WarehouseManagerContracts.DTOs.EmployeeWarehouse
{
    public class CreateEmployeeWarehouseCommand
    {
        public int UserId { get; init; }
        public int EmployeeId { get; init; }
        public int WarehouseId { get; init; }
    }
}

