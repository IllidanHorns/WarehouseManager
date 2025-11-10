using CommunityToolkit.Mvvm.ComponentModel;
using WarehouseManager.Services.Summary;

namespace WarehouseManager.Wpf.Models
{
    public class CartItem : ObservableObject
    {
        public ProductSummary Product { get; set; } = null!;

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (SetProperty(ref _quantity, value))
                {
                    OnPropertyChanged(nameof(TotalPrice));
                }
            }
        }

        public int WarehouseId { get; set; }
        public string WarehouseAddress { get; set; } = "";
        public decimal UnitPrice => Product.Price;
        public decimal TotalPrice => Quantity * UnitPrice;

        private int _availableQuantity;
        public int AvailableQuantity
        {
            get => _availableQuantity;
            set => SetProperty(ref _availableQuantity, value);
        }
    }
}

