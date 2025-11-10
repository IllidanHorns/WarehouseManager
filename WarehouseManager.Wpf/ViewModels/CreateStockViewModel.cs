using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WarehouseManager.Services.Exceptions;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Services.Summary;
using WarehouseManager.Wpf.Static;
using WarehouseManager.Contracts.DTOs.Remaining;

namespace WarehouseManager.Wpf.ViewModels
{
    public partial class CreateStockViewModel : ObservableObject
    {
        private readonly IStockService _stockService;
        private readonly IProductService _productService;
        private readonly IWarehouseService _warehouseService;

        [ObservableProperty]
        private int _selectedProductId;

        [ObservableProperty]
        private int _selectedWarehouseId;

        [ObservableProperty]
        private string _quantity = "";

        [ObservableProperty]
        private string _errorMessage = "";

        [ObservableProperty]
        private bool _isBusy;

        public ObservableCollection<ProductSummary> Products { get; } = new();
        public ObservableCollection<WarehouseSummary> Warehouses { get; } = new();

        public CreateStockViewModel(
            IStockService stockService,
            IProductService productService,
            IWarehouseService warehouseService)
        {
            _stockService = stockService;
            _productService = productService;
            _warehouseService = warehouseService;
            _ = LoadProductsAndWarehousesAsync();
        }

        private async Task LoadProductsAndWarehousesAsync()
        {
            try
            {
                var productFilter = new ProductsFilters
                {
                    Page = 1,
                    PageSize = 1000,
                    IncludeArchived = false
                };
                var productsResult = await _productService.GetPagedAsync(productFilter);
                Products.Clear();
                foreach (var product in productsResult.Items)
                {
                    Products.Add(product);
                }

                var warehouseFilter = new WarehouseFilter
                {
                    Page = 1,
                    PageSize = 1000,
                    IncludeArchived = false
                };
                var warehousesResult = await _warehouseService.GetPagedAsync(warehouseFilter);
                Warehouses.Clear();
                foreach (var warehouse in warehousesResult.Items)
                {
                    Warehouses.Add(warehouse);
                }

                if (Products.Any())
                {
                    SelectedProductId = Products.First().Id;
                }
                if (Warehouses.Any())
                {
                    SelectedWarehouseId = Warehouses.First().Id;
                }
            }
            catch (System.Exception)
            {
                // Игнорируем ошибки загрузки
            }
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task SaveAsync()
        {
            ErrorMessage = "";
            IsBusy = true;

            try
            {
                if (CurrentUser.UserId == null)
                {
                    ErrorMessage = "Пользователь не авторизован";
                    return;
                }

                if (!int.TryParse(Quantity, out int quantityValue) || quantityValue < 0)
                {
                    ErrorMessage = "Количество должно быть неотрицательным числом";
                    return;
                }

                var command = new CreateStockCommand
                {
                    UserId = CurrentUser.UserId.Value,
                    ProductId = SelectedProductId,
                    WarehouseId = SelectedWarehouseId,
                    Quantity = quantityValue
                };

                await _stockService.CreateAsync(command);

                MessageBox.Show("Остаток успешно создан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                System.Windows.Application.Current.Windows
                    .OfType<View.CreateStockWindow>()
                    .FirstOrDefault()?.Close();
            }
            catch (ModelValidationException ex)
            {
                var errors = ex.Errors;
                foreach (var error in errors)
                {
                    ErrorMessage += $"{error.ErrorMessage}\n";
                }
            }
            catch (ConflictException ex)
            {
                ErrorMessage = ex.Message;
            }
            catch (DomainException ex)
            {
                ErrorMessage = ex.Message;
            }
            catch (System.Exception ex)
            {
                if (ex.Message.Contains("saving the entity changes") || ex.Message.Contains("inner exception"))
                {
                    var innerEx = ex.InnerException;
                    if (innerEx != null)
                    {
                        ErrorMessage = $"Ошибка сохранения: {innerEx.Message}";
                    }
                    else
                    {
                        ErrorMessage = "Ошибка сохранения данных. Проверьте корректность введенных данных.";
                    }
                }
                else
                {
                    ErrorMessage = "Произошла ошибка: " + ex.Message;
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            System.Windows.Application.Current.Windows
                .OfType<View.CreateStockWindow>()
                .FirstOrDefault()?.Close();
        }

        private bool CanSave()
        {
            if (IsBusy || SelectedProductId <= 0 || SelectedWarehouseId <= 0 || string.IsNullOrWhiteSpace(Quantity))
                return false;

            if (!int.TryParse(Quantity, out int quantity) || quantity < 0)
                return false;

            return true;
        }

        partial void OnIsBusyChanged(bool value) => SaveCommand.NotifyCanExecuteChanged();
        partial void OnSelectedProductIdChanged(int value) => SaveCommand.NotifyCanExecuteChanged();
        partial void OnSelectedWarehouseIdChanged(int value) => SaveCommand.NotifyCanExecuteChanged();
        partial void OnQuantityChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
    }
}

