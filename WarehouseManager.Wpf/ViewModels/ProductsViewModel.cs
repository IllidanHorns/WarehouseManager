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
    public partial class ProductsViewModel : ObservableObject
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IWarehouseService _warehouseService;

        [ObservableProperty]
        private ObservableCollection<ProductSummary> _products = new();

        [ObservableProperty]
        private ProductSummary? _selectedProduct;

        [ObservableProperty]
        private string _nameFilter = "";

        private CategorySummary? _selectedCategory;
        private WarehouseSummary? _selectedWarehouse;

        public CategorySummary? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCategorySelected));
                // Если выбран элемент с Id = -1 ("Все категории"), сбрасываем фильтр
                if (value != null && value.Id == -1)
                {
                    _selectedCategory = null;
                    SelectedCategoryId = null;
                    OnPropertyChanged(nameof(SelectedCategory));
                }
                else
                {
                    SelectedCategoryId = value?.Id;
                }
            }
        }

        public WarehouseSummary? SelectedWarehouse
        {
            get => _selectedWarehouse;
            set
            {
                _selectedWarehouse = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsWarehouseSelected));
                // Если выбран элемент с Id = -1 ("Все склады"), сбрасываем фильтр
                if (value != null && value.Id == -1)
                {
                    _selectedWarehouse = null;
                    SelectedWarehouseId = null;
                    OnPropertyChanged(nameof(SelectedWarehouse));
                }
                else
                {
                    SelectedWarehouseId = value?.Id;
                }
            }
        }

        public bool IsCategorySelected => SelectedCategory != null;
        public bool IsWarehouseSelected => SelectedWarehouse != null;

        private int? _selectedCategoryId;
        private int? _selectedWarehouseId;

        public int? SelectedCategoryId
        {
            get => _selectedCategoryId;
            set
            {
                _selectedCategoryId = value;
                OnPropertyChanged();
            }
        }

        public int? SelectedWarehouseId
        {
            get => _selectedWarehouseId;
            set
            {
                _selectedWarehouseId = value;
                OnPropertyChanged();
            }
        }

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

        public ProductsViewModel(IProductService productService, ICategoryService categoryService, IWarehouseService warehouseService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _warehouseService = warehouseService;
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
                // Загружаем категории для фильтра
                var categoryFilter = new CategoryFilter
                {
                    Page = 1,
                    PageSize = 1000,
                    IncludeArchived = false
                };
                var categoriesResult = await _categoryService.GetPagedAsync(categoryFilter);
                Categories.Clear();
                // Добавляем элемент "Все категории" для сброса фильтра
                Categories.Add(new CategorySummary { Id = -1, Name = "Все категории" });
                foreach (var category in categoriesResult.Items)
                {
                    Categories.Add(category);
                }

                // Загружаем склады для фильтра
                var warehouseFilter = new WarehouseFilter
                {
                    Page = 1,
                    PageSize = 1000,
                    IncludeArchived = false
                };
                var warehousesResult = await _warehouseService.GetPagedAsync(warehouseFilter);
                Warehouses.Clear();
                // Добавляем элемент "Все склады" для сброса фильтра
                Warehouses.Add(new WarehouseSummary { Id = -1, Address = "Все склады" });
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
        private void ClearCategoryFilter()
        {
            SelectedCategory = null;
            _ = FilterAsync();
        }

        [RelayCommand]
        private void ClearWarehouseFilter()
        {
            SelectedWarehouse = null;
            _ = FilterAsync();
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
        private void OpenCreateWindow()
        {
            var createWindow = new View.CreateProductWindow();
            createWindow.ShowDialog();
            _ = LoadProductsAsync();
        }

        [RelayCommand]
        private void OpenUpdateWindow(ProductSummary? product)
        {
            var productToEdit = product ?? SelectedProduct;
            if (productToEdit == null)
                return;

            SelectedProduct = productToEdit;
            var updateWindow = new View.UpdateProductWindow(productToEdit);
            updateWindow.ShowDialog();
            _ = LoadProductsAsync();
        }

        [RelayCommand]
        private async Task DeleteProductAsync(ProductSummary? product)
        {
            var productToDelete = product ?? SelectedProduct;
            if (productToDelete == null)
                return;

            // Подтверждение удаления
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить продукт?\n\nНазвание: {productToDelete.Name}\nЦена: {productToDelete.Price} руб.\nВес: {productToDelete.Weight} кг",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            IsLoading = true;
            ErrorMessage = "";

            try
            {
                if (CurrentUser.UserId == null)
                {
                    ErrorMessage = "Пользователь не авторизован";
                    return;
                }

                await _productService.ArchiveAsync(productToDelete.Id, CurrentUser.UserId.Value);

                MessageBox.Show(
                    "Продукт успешно удалён!",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Перезагружаем список
                await LoadProductsAsync();
            }
            catch (ConflictException ex)
            {
                ErrorMessage = ex.Message;
                MessageBox.Show(
                    ex.Message,
                    "Ошибка удаления",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (DomainException ex)
            {
                ErrorMessage = ex.Message;
                MessageBox.Show(
                    ex.Message,
                    "Ошибка удаления",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (System.Exception ex)
            {
                // Показываем более понятное сообщение для ошибок БД
                string errorMessage;
                if (ex.Message.Contains("saving the entity changes") || ex.Message.Contains("inner exception"))
                {
                    var innerEx = ex.InnerException;
                    if (innerEx != null)
                    {
                        errorMessage = $"Ошибка сохранения: {innerEx.Message}";
                    }
                    else
                    {
                        errorMessage = "Ошибка сохранения данных. Проверьте корректность введенных данных и права доступа.";
                    }
                }
                else
                {
                    errorMessage = "Ошибка при удалении продукта: " + ex.Message;
                }

                ErrorMessage = errorMessage;
                MessageBox.Show(
                    errorMessage,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
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
            // При изменении размера страницы сбрасываем на первую страницу и перезагружаем
            CurrentPage = 1;
            _ = LoadProductsPageAsync(1);
        }

        partial void OnSelectedProductChanged(ProductSummary? value)
        {
            if (value != null)
            {
                // Можно добавить логику при выборе продукта
            }
        }
    }
}

