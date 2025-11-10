namespace WarehouseManagerContracts.DTOs.Order
{
    public class UpdateOrderStatusCommand
    {
        public int OrderId { get; set; }
        public int StatusId { get; set; }
        public int? UserId { get; set; }
    }
}

