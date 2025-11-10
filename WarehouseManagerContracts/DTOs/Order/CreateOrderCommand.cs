using WarehouseManagerContracts.DTOs.OrderProduct;

namespace WarehouseManagerContracts.DTOs.Order
{
    public record class CreateOrderCommand
    {
        public int WarehouseId { get; set; }
        public int UserId { get; set; }
        public List<CreateOrderProductDto> Products { get; set; } = new();
    }
}

