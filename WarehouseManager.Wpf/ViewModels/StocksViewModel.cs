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

namespace WarehouseManager.Wpf.ViewModels
{
    public partial class StocksViewModel : ObservableObject
    {
        private readonly IStockService _stockService;
        private readonly IProductService _productService;
        private readonly IWarehouseService _warehouseService;

        [ObservableProperty]
        private ObservableCollection<WarehouseStockSummary> _stocks = new();

        [ObservableProperty]
        private WarehouseStockSummary? _selectedStock;

        [ObservableProperty]
        private string _productNameFilter = "";

        [ObservableProperty]
        private string _warehouseAddressFilter = "";

        [ObservableProperty]
        private int? _selectedProductId;

        [ObservableProperty]
        private int? _selectedWarehouseId;

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _totalPages = 1;

        [ObservableProperty]
        private int _totalCount = 0;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage = "";

        [ObservableProperty]
        private int _pageSize = 20;

        public ObservableCollection<int> AvailablePageSizes { get; } = new() { 10, 15, 20, 30 };
        public ObservableCollection<ProductSummary> Products { get; } = new();
        public ObservableCollection<WarehouseSummary> Warehouses { get; } = new();

        public StocksViewModel(
            IStockService stockService,
            IProductService productService,
            IWarehouseService warehouseService)
        {
            _stockService = stockService;
            _productService = productService;
            _warehouseService = warehouseService;
        }

        [RelayCommand]
        public async Task LoadStocksAsync()
        {
            await LoadStocksPageAsync(CurrentPage);
        }

        [RelayCommand]
        public async Task LoadProductsAndWarehousesAsync()
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
                Products.Add(new ProductSummary { Id = -1, Name = "Все продукты" });
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
                Warehouses.Add(new WarehouseSummary { Id = -1, Address = "Все склады" });
                foreach (var warehouse in warehousesResult.Items)
                {
                    Warehouses.Add(warehouse);
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Ошибка при загрузке продуктов и складов: " + ex.Message;
            }
        }

        [RelayCommand]
        private async Task FilterAsync()
        {
            CurrentPage = 1;
            await LoadStocksPageAsync(1);
        }

        [RelayCommand]
        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await LoadStocksPageAsync(CurrentPage);
            }
        }

        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                await LoadStocksPageAsync(CurrentPage);
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadStocksPageAsync(CurrentPage);
        }

        [RelayCommand]
        private void OpenCreateWindow()
        {
            var createWindow = new View.CreateStockWindow();
            createWindow.ShowDialog();
            _ = LoadStocksAsync();
        }

        [RelayCommand]
        private void OpenUpdateWindow(WarehouseStockSummary? stock)
        {
            var stockToEdit = stock ?? SelectedStock;
            if (stockToEdit == null)
                return;

            SelectedStock = stockToEdit;
            var updateWindow = new View.UpdateStockWindow(stockToEdit);
            updateWindow.ShowDialog();
            _ = LoadStocksAsync();
        }

        private async Task LoadStocksPageAsync(int page)
        {
            IsLoading = true;
            ErrorMessage = "";

            try
            {
                var filter = new StockFilter
                {
                    Page = page,
                    PageSize = PageSize,
                    IncludeArchived = false,
                    ProductName = string.IsNullOrWhiteSpace(ProductNameFilter) ? null : ProductNameFilter,
                    WarehouseAddress = string.IsNullOrWhiteSpace(WarehouseAddressFilter) ? null : WarehouseAddressFilter,
                    ProductId = SelectedProductId.HasValue && SelectedProductId.Value > 0 ? SelectedProductId : null,
                    WarehouseId = SelectedWarehouseId.HasValue && SelectedWarehouseId.Value > 0 ? SelectedWarehouseId : null
                };

                var result = await _stockService.GetPagedAsync(filter);

                Stocks.Clear();
                foreach (var stock in result.Items)
                {
                    Stocks.Add(stock);
                }

                TotalCount = result.TotalCount;
                TotalPages = result.TotalPages;
                CurrentPage = page;
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Ошибка при загрузке остатков: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnPageSizeChanged(int value)
        {
            CurrentPage = 1;
            _ = LoadStocksPageAsync(1);
        }
    }
}

