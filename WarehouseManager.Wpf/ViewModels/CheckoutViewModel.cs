using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using WarehouseManager.Core.Data;
using WarehouseManager.Core.Models;
using WarehouseManager.Services.Exceptions;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Services.Summary;
using WarehouseManager.Wpf.Models;
using WarehouseManager.Wpf.Static;
using WarehouseManagerContracts.DTOs.Order;
using WarehouseManagerContracts.DTOs.OrderProduct;

namespace WarehouseManager.Wpf.ViewModels
{
    public partial class CheckoutViewModel : ObservableObject
    {
        private readonly Cart _cart;
        private readonly IOrderService _orderService;
        private readonly AppDbContext _context;
        private readonly IStockService _stockService;

        [ObservableProperty]
        private int _selectedWarehouseId;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage = "";

        public ObservableCollection<WarehouseSummary> Warehouses { get; } = new();

        public decimal TotalPrice => _cart.TotalPrice;
        public ObservableCollection<CartItem> Items => _cart.Items;

        public CheckoutViewModel(
            Cart cart,
            IOrderService orderService,
            AppDbContext context,
            IStockService stockService)
        {
            _cart = cart;
            _orderService = orderService;
            _context = context;
            _stockService = stockService;
            _ = LoadDataAsync();
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            IsLoading = true;
            ErrorMessage = "";

            try
            {
                // Загружаем склады
                var warehouses = await _context.Warehouses
                    .Where(w => !w.IsArchived)
                    .ToListAsync();
                Warehouses.Clear();
                foreach (var warehouse in warehouses)
                {
                    Warehouses.Add(new WarehouseSummary
                    {
                        Id = warehouse.Id,
                        Address = warehouse.Address,
                        Square = warehouse.Square,
                        IsArchived = warehouse.IsArchived,
                        CreationDatetime = warehouse.CreationDatetime
                    });
                }

                // Если в корзине есть товары, выбираем склад из первого товара
                if (Items.Any() && SelectedWarehouseId == 0)
                {
                    var firstWarehouseId = Items.Select(i => i.WarehouseId).Distinct().FirstOrDefault();
                    if (firstWarehouseId > 0)
                    {
                        SelectedWarehouseId = firstWarehouseId;
                    }
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Ошибка при загрузке данных: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanPlaceOrder))]
        private async Task PlaceOrderAsync()
        {
            if (CurrentUser.UserId == null)
            {
                ErrorMessage = "Пользователь не авторизован";
                MessageBox.Show(ErrorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            IsLoading = true;
            ErrorMessage = "";

            try
            {
                // Проверяем, что все товары в корзине с одного склада
                var warehouseIds = Items.Select(i => i.WarehouseId).Distinct().ToList();
                if (warehouseIds.Count > 1)
                {
                    ErrorMessage = "Все товары в заказе должны быть с одного склада";
                    MessageBox.Show(ErrorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var warehouseId = warehouseIds.First();
                if (SelectedWarehouseId != warehouseId)
                {
                    ErrorMessage = "Выбранный склад не совпадает со складом товаров в корзине";
                    MessageBox.Show(ErrorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Финальная проверка остатков
                foreach (var item in Items)
                {
                    var stockFilter = new WarehouseManager.Services.Filters.StockFilter
                    {
                        Page = 1,
                        PageSize = 1,
                        IncludeArchived = false,
                        ProductId = item.Product.Id,
                        WarehouseId = warehouseId
                    };
                    var stockResult = await _stockService.GetPagedAsync(stockFilter);
                    var availableQuantity = stockResult.Items.FirstOrDefault()?.Quantity ?? 0;

                    if (availableQuantity < item.Quantity)
                    {
                        ErrorMessage = $"Недостаточно товара '{item.Product.Name}' на складе. Доступно: {availableQuantity}, запрошено: {item.Quantity}";
                        MessageBox.Show(ErrorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // Создаём команду заказа
                var command = new CreateOrderCommand
                {
                    WarehouseId = warehouseId,
                    UserId = CurrentUser.UserId.Value,
                    Products = Items.Select(item => new CreateOrderProductDto
                    {
                        ProductId = item.Product.Id,
                        Quantity = item.Quantity,
                        OrderPrice = item.Product.Price
                    }).ToList()
                };

                var order = await _orderService.CreateAsync(command);

                MessageBox.Show(
                    $"Заказ успешно создан!\nНомер заказа: {order.Id}\nСумма: {order.TotalPrice:F2} руб.",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Очищаем корзину
                _cart.Clear();
                OnPropertyChanged(nameof(TotalPrice));
                OnPropertyChanged(nameof(Items));

                // Закрываем окно
                System.Windows.Application.Current.Windows
                    .OfType<View.CheckoutWindow>()
                    .FirstOrDefault()?.Close();
            }
            catch (ConflictException ex)
            {
                ErrorMessage = ex.Message;
                MessageBox.Show(ex.Message, "Ошибка создания заказа", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (DomainException ex)
            {
                ErrorMessage = ex.Message;
                MessageBox.Show(ex.Message, "Ошибка создания заказа", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Ошибка при создании заказа: " + ex.Message;
                if (ex.InnerException != null)
                {
                    ErrorMessage += "\n" + ex.InnerException.Message;
                }
                MessageBox.Show(ErrorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            System.Windows.Application.Current.Windows
                .OfType<View.CheckoutWindow>()
                .FirstOrDefault()?.Close();
        }

        [RelayCommand]
        private void ClearWarehouse()
        {
            SelectedWarehouseId = 0;
        }

        private bool CanPlaceOrder()
        {
            return !IsLoading &&
                   !_cart.IsEmpty &&
                   SelectedWarehouseId > 0 &&
                   CurrentUser.UserId.HasValue;
        }

        partial void OnIsLoadingChanged(bool value) => PlaceOrderCommand.NotifyCanExecuteChanged();
        partial void OnSelectedWarehouseIdChanged(int value) => PlaceOrderCommand.NotifyCanExecuteChanged();
    }
}

