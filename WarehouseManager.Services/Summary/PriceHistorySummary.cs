using System;
using WarehouseManager.Services.Summary.Interfaces;

namespace WarehouseManager.Services.Summary
{
    public class PriceHistorySummary : ISummary
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public DateTime UpdateDatetime { get; set; }

        public override string ToString()
        {
            return $"{ProductName} | {OldPrice} -> {NewPrice}";
        }
    }
}
