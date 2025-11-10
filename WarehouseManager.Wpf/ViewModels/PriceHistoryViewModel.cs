using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Services.Summary;

namespace WarehouseManager.Wpf.ViewModels
{
    public partial class PriceHistoryViewModel : ObservableObject
    {
        private readonly IPriceHistoryService _priceHistoryService;
        private readonly ICategoryService _categoryService;

        [ObservableProperty]
        private ObservableCollection<PriceHistorySummary> _priceHistories = new();

        [ObservableProperty]
        private PriceHistorySummary? _selectedPriceHistory;

        [ObservableProperty]
        private string _productNameFilter = string.Empty;

        private CategorySummary? _selectedCategory;
        private int? _selectedCategoryId;

        public CategorySummary? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                SetProperty(ref _selectedCategory, value);
                OnPropertyChanged(nameof(IsCategorySelected));

                if (value != null && value.Id == -1)
                {
                    _selectedCategory = null;
                    _selectedCategoryId = null;
                    OnPropertyChanged(nameof(SelectedCategory));
                }
                else
                {
                    _selectedCategoryId = value?.Id;
                }
            }
        }

        public bool IsCategorySelected => SelectedCategory != null;

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _totalPages = 1;

        [ObservableProperty]
        private int _totalCount;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private int _pageSize = 20;

        public ObservableCollection<int> AvailablePageSizes { get; } = new() { 10, 15, 20, 30, 50 };
        public ObservableCollection<CategorySummary> Categories { get; } = new();

        public PriceHistoryViewModel(IPriceHistoryService priceHistoryService, ICategoryService categoryService)
        {
            _priceHistoryService = priceHistoryService;
            _categoryService = categoryService;
        }

        [RelayCommand]
        public async Task LoadPriceHistoriesAsync()
        {
            await LoadPageAsync(CurrentPage);
        }

        [RelayCommand]
        public async Task LoadCategoriesAsync()
        {
            try
            {
                var filter = new CategoryFilter
                {
                    Page = 1,
                    PageSize = 1000,
                    IncludeArchived = false
                };

                var categoriesResult = await _categoryService.GetPagedAsync(filter);

                Categories.Clear();
                Categories.Add(new CategorySummary { Id = -1, Name = "Все категории" });
                foreach (var category in categoriesResult.Items.OrderBy(c => c.Name))
                {
                    Categories.Add(category);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Не удалось загрузить категории: " + ex.Message;
            }
        }

        [RelayCommand]
        private void ClearCategoryFilter()
        {
            SelectedCategory = null;
            _ = FilterAsync();
        }

        [RelayCommand]
        private async Task FilterAsync()
        {
            CurrentPage = 1;
            await LoadPageAsync(1);
        }

        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (CurrentPage >= TotalPages)
                return;

            await LoadPageAsync(CurrentPage + 1);
        }

        [RelayCommand]
        private async Task PreviousPageAsync()
        {
            if (CurrentPage <= 1)
                return;

            await LoadPageAsync(CurrentPage - 1);
        }

        partial void OnPageSizeChanged(int value)
        {
            if (value <= 0)
            {
                PageSize = 20;
                return;
            }

            CurrentPage = 1;
            _ = LoadPageAsync(1);
        }

        private async Task LoadPageAsync(int page)
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var filter = new PriceHistoryFilter
                {
                    Page = page,
                    PageSize = PageSize,
                    IncludeArchived = false,
                    ProductName = string.IsNullOrWhiteSpace(ProductNameFilter) ? null : ProductNameFilter.Trim(),
                    CategoryId = _selectedCategoryId
                };

                var result = await _priceHistoryService.GetPagedAsync(filter);

                PriceHistories.Clear();
                foreach (var item in result.Items)
                {
                    PriceHistories.Add(item);
                }

                TotalCount = result.TotalCount;
                CurrentPage = page;
                TotalPages = Math.Max(1, (int)Math.Ceiling((double)result.TotalCount / PageSize));
            }
            catch (Exception ex)
            {
                ErrorMessage = "Ошибка при загрузке истории цен: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
