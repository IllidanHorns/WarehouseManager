using WarehouseManager.Services.Summary.Interfaces;

namespace WarehouseManager.Services.Summary
{
    public class OrderSummary : ISummary
    {
        public int Id { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreationDatetime { get; set; }
        public string WarehouseAddress { get; set; } = default!;
        public string EmployeeFullName { get; set; } = default!;
        public string UserEmail { get; set; } = default!;
        public string OrderStatusName { get; set; } = default!;
    }
}
