namespace WarehouseManager.Contracts.DTOs.Remaining
{
    public record class CreateStockCommand
    {
        public int UserId { get; init; }
        public int ProductId { get; init; }
        public int WarehouseId { get; init; }
        public int Quantity { get; init; }
    }
}

