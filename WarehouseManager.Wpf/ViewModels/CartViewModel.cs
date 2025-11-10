using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Services.Summary;
using WarehouseManager.Wpf.Models;
using WarehouseManager.Wpf.Static;

namespace WarehouseManager.Wpf.ViewModels
{
    public partial class CartViewModel : ObservableObject
    {
        private readonly Cart _cart;
        private readonly IStockService _stockService;

        public ObservableCollection<CartItem> Items => _cart.Items;

        public decimal TotalPrice => _cart.TotalPrice;

        public int TotalItems => _cart.TotalItems;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage = "";

        public CartViewModel(Cart cart, IStockService stockService)
        {
            _cart = cart;
            _stockService = stockService;
        }

        [RelayCommand]
        public async Task UpdateQuantityAsync(CartItem item)
        {
            if (item == null)
                return;

            try
            {
                // Проверяем доступное количество на складе
                var stockFilter = new WarehouseManager.Services.Filters.StockFilter
                {
                    Page = 1,
                    PageSize = 1,
                    IncludeArchived = false,
                    ProductId = item.Product.Id,
                    WarehouseId = item.WarehouseId
                };
                var stockResult = await _stockService.GetPagedAsync(stockFilter);
                var availableQuantity = stockResult.Items.FirstOrDefault()?.Quantity ?? 0;

                // Проверяем другие товары в корзине с тем же продуктом и складом
                var otherItemsQuantity = Items
                    .Where(i => i != item && i.Product.Id == item.Product.Id && i.WarehouseId == item.WarehouseId)
                    .Sum(i => i.Quantity);

                var maxAvailable = availableQuantity - otherItemsQuantity;

                if (item.Quantity > maxAvailable)
                {
                    MessageBox.Show(
                        $"Недостаточно товара на складе. Максимально доступно: {maxAvailable}",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    item.Quantity = maxAvailable;
                    OnPropertyChanged(nameof(TotalPrice));
                    OnPropertyChanged(nameof(TotalItems));
                    return;
                }

                if (item.Quantity <= 0)
                {
                    _cart.RemoveItem(item);
                }
                else
                {
                    item.AvailableQuantity = availableQuantity;
                }

                OnPropertyChanged(nameof(TotalPrice));
                OnPropertyChanged(nameof(TotalItems));
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Ошибка при обновлении количества: " + ex.Message;
                MessageBox.Show(ErrorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void RemoveItem(CartItem item)
        {
            if (item == null)
                return;

            _cart.RemoveItem(item);
            OnPropertyChanged(nameof(TotalPrice));
            OnPropertyChanged(nameof(TotalItems));
        }

        [RelayCommand]
        private void ClearCart()
        {
            var result = MessageBox.Show(
                "Вы уверены, что хотите очистить корзину?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _cart.Clear();
                OnPropertyChanged(nameof(TotalPrice));
                OnPropertyChanged(nameof(TotalItems));
            }
        }

        [RelayCommand]
        private void OpenCheckout()
        {
            if (_cart.IsEmpty)
            {
                MessageBox.Show("Корзина пуста", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var checkoutWindow = new View.CheckoutWindow();
            checkoutWindow.ShowDialog();
        }

        [RelayCommand]
        private async Task RefreshAvailabilityAsync()
        {
            IsLoading = true;
            ErrorMessage = "";

            try
            {
                foreach (var item in Items.ToList())
                {
                    var stockFilter = new WarehouseManager.Services.Filters.StockFilter
                    {
                        Page = 1,
                        PageSize = 1,
                        IncludeArchived = false,
                        ProductId = item.Product.Id,
                        WarehouseId = item.WarehouseId
                    };
                    var stockResult = await _stockService.GetPagedAsync(stockFilter);
                    var availableQuantity = stockResult.Items.FirstOrDefault()?.Quantity ?? 0;
                    
                    item.AvailableQuantity = availableQuantity;

                    // Если количество в корзине больше доступного, уменьшаем
                    if (item.Quantity > availableQuantity)
                    {
                        item.Quantity = availableQuantity;
                        if (item.Quantity == 0)
                        {
                            _cart.RemoveItem(item);
                        }
                    }
                }

                OnPropertyChanged(nameof(TotalPrice));
                OnPropertyChanged(nameof(TotalItems));
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Ошибка при обновлении остатков: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}

