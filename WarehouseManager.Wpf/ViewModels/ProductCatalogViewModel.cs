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
using WarehouseManager.Wpf.Models;
using WarehouseManager.Wpf.Static;

namespace WarehouseManager.Wpf.ViewModels
{
    public partial class ProductCatalogViewModel : ObservableObject
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IWarehouseService _warehouseService;
        private readonly IStockService _stockService;
        private readonly Cart _cart;

        [ObservableProperty]
        private ObservableCollection<ProductSummary> _products = new();

        [ObservableProperty]
        private ProductSummary? _selectedProduct;

        [ObservableProperty]
        private string _nameFilter = "";

        [ObservableProperty]
        private int? _selectedCategoryId;

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
        public ObservableCollection<CategorySummary> Categories { get; } = new();
        public ObservableCollection<WarehouseSummary> Warehouses { get; } = new();

        // Для выбранного продукта
        [ObservableProperty]
        private int _selectedQuantity = 1;

        [ObservableProperty]
        private int _availableQuantity = 0;

        [ObservableProperty]
        private int _selectedWarehouseForProduct;

        public ProductCatalogViewModel(
            IProductService productService,
            ICategoryService categoryService,
            IWarehouseService warehouseService,
            IStockService stockService,
            Cart cart)
        {
            _productService = productService;
            _categoryService = categoryService;
            _warehouseService = warehouseService;
            _stockService = stockService;
            _cart = cart;
        }

        [RelayCommand]
        public async Task LoadProductsAsync()
        {
            await LoadProductsPageAsync(CurrentPage);
        }

        [RelayCommand]
        public async Task LoadCategoriesAndWarehousesAsync()
        {
            try
            {
                var categoryFilter = new CategoryFilter
                {
                    Page = 1,
                    PageSize = 1000,
                    IncludeArchived = false
                };
                var categoriesResult = await _categoryService.GetPagedAsync(categoryFilter);
                Categories.Clear();
                Categories.Add(new CategorySummary { Id = -1, Name = "Все категории" });
                foreach (var category in categoriesResult.Items)
                {
                    Categories.Add(category);
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
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Ошибка при загрузке категорий и складов: " + ex.Message;
            }
        }

        [RelayCommand]
        private async Task FilterAsync()
        {
            CurrentPage = 1;
            await LoadProductsPageAsync(1);
        }

        [RelayCommand]
        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await LoadProductsPageAsync(CurrentPage);
            }
        }

        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                await LoadProductsPageAsync(CurrentPage);
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadProductsPageAsync(CurrentPage);
        }

        [RelayCommand]
        public async Task CheckAvailabilityAsync(ProductSummary? product)
        {
            var productToCheck = product ?? SelectedProduct;
            if (productToCheck == null || SelectedWarehouseForProduct <= 0)
                return;

            try
            {
                var stockFilter = new StockFilter
                {
                    Page = 1,
                    PageSize = 1,
                    IncludeArchived = false,
                    ProductId = productToCheck.Id,
                    WarehouseId = SelectedWarehouseForProduct
                };
                var stockResult = await _stockService.GetPagedAsync(stockFilter);
                AvailableQuantity = stockResult.Items.FirstOrDefault()?.Quantity ?? 0;
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Ошибка при проверке остатков: " + ex.Message;
                AvailableQuantity = 0;
            }
        }

        [RelayCommand]
        public async Task AddToCartAsync(ProductSummary? product)
        {
            var productToAdd = product ?? SelectedProduct;
            if (productToAdd == null)
            {
                MessageBox.Show("Выберите продукт", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedWarehouseForProduct <= 0)
            {
                MessageBox.Show("Выберите склад", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedQuantity <= 0)
            {
                MessageBox.Show("Количество должно быть больше 0", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Проверяем остатки
                await CheckAvailabilityAsync(productToAdd);
                
                if (AvailableQuantity < SelectedQuantity)
                {
                    MessageBox.Show(
                        $"Недостаточно товара на складе. Доступно: {AvailableQuantity}, запрошено: {SelectedQuantity}",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Проверяем, не превышает ли общее количество в корзине доступное
                var existingCartItem = _cart.Items.FirstOrDefault(i => 
                    i.Product.Id == productToAdd.Id && 
                    i.WarehouseId == SelectedWarehouseForProduct);
                
                var totalInCart = existingCartItem?.Quantity ?? 0;
                if (totalInCart + SelectedQuantity > AvailableQuantity)
                {
                    MessageBox.Show(
                        $"Недостаточно товара на складе. Доступно: {AvailableQuantity}, в корзине: {totalInCart}, пытаетесь добавить: {SelectedQuantity}",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var warehouse = Warehouses.FirstOrDefault(w => w.Id == SelectedWarehouseForProduct);
                var cartItem = new CartItem
                {
                    Product = productToAdd,
                    Quantity = SelectedQuantity,
                    WarehouseId = SelectedWarehouseForProduct,
                    WarehouseAddress = warehouse?.Address ?? "Неизвестно",
                    AvailableQuantity = AvailableQuantity
                };

                _cart.AddItem(cartItem);

                MessageBox.Show(
                    $"Товар '{productToAdd.Name}' добавлен в корзину",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Обновляем доступное количество
                await CheckAvailabilityAsync(productToAdd);
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Ошибка при добавлении в корзину: " + ex.Message;
                MessageBox.Show(ErrorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadProductsPageAsync(int page)
        {
            IsLoading = true;
            ErrorMessage = "";

            try
            {
                var filter = new ProductsFilters
                {
                    Page = page,
                    PageSize = PageSize,
                    IncludeArchived = false,
                    Name = string.IsNullOrWhiteSpace(NameFilter) ? null : NameFilter,
                    CategoryId = SelectedCategoryId,
                    WarehouseId = SelectedWarehouseId
                };

                var result = await _productService.GetPagedAsync(filter);

                Products.Clear();
                foreach (var product in result.Items)
                {
                    Products.Add(product);
                }

                TotalCount = result.TotalCount;
                TotalPages = result.TotalPages;
                CurrentPage = page;
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Ошибка при загрузке продуктов: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnPageSizeChanged(int value)
        {
            CurrentPage = 1;
            _ = LoadProductsPageAsync(1);
        }

        partial void OnSelectedProductChanged(ProductSummary? value)
        {
            if (value != null && SelectedWarehouseForProduct > 0)
            {
                _ = CheckAvailabilityAsync(value);
            }
        }

        partial void OnSelectedWarehouseForProductChanged(int value)
        {
            if (value > 0 && SelectedProduct != null)
            {
                _ = CheckAvailabilityAsync(SelectedProduct);
            }
        }

        [RelayCommand]
        private void OpenCart()
        {
            var cartWindow = new View.CartWindow();
            cartWindow.ShowDialog();
        }

        [RelayCommand]
        private void ClearWarehouseFilter()
        {
            SelectedWarehouseId = null;
        }
    }
}

