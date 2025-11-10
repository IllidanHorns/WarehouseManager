using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using WarehouseManager.Services.Exceptions;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Services.Summary;
using WarehouseManager.Wpf.Static;

namespace WarehouseManager.Wpf.ViewModels
{
    public partial class CategoriesViewModel : ObservableObject
    {
        private readonly ICategoryService _categoryService;

        [ObservableProperty]
        private ObservableCollection<CategorySummary> _categories = new();

        [ObservableProperty]
        private CategorySummary? _selectedCategory;

        [ObservableProperty]
        private string _nameFilter = "";

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

        public CategoriesViewModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [RelayCommand]
        public async Task LoadCategoriesAsync()
        {
            await LoadCategoriesPageAsync(CurrentPage);
        }

        [RelayCommand]
        private async Task FilterAsync()
        {
            CurrentPage = 1;
            await LoadCategoriesPageAsync(1);
        }

        [RelayCommand]
        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await LoadCategoriesPageAsync(CurrentPage);
            }
        }

        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                await LoadCategoriesPageAsync(CurrentPage);
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadCategoriesPageAsync(CurrentPage);
        }

        [RelayCommand]
        private void OpenCreateWindow()
        {
            var createWindow = new View.CreateCategoryWindow();
            createWindow.ShowDialog();
            _ = LoadCategoriesAsync();
        }

        [RelayCommand]
        private void OpenUpdateWindow(CategorySummary? category)
        {
            var categoryToEdit = category ?? SelectedCategory;
            if (categoryToEdit == null)
                return;

            SelectedCategory = categoryToEdit;
            var updateWindow = new View.UpdateCategoryWindow(categoryToEdit);
            updateWindow.ShowDialog();
            _ = LoadCategoriesAsync();
        }

        [RelayCommand]
        private async Task DeleteCategoryAsync(CategorySummary? category)
        {
            var categoryToDelete = category ?? SelectedCategory;
            if (categoryToDelete == null)
                return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить категорию?\n\nНазвание: {categoryToDelete.Name}\nОписание: {categoryToDelete.Description ?? "Нет описания"}\nТоваров: {categoryToDelete.ActiveProductCount}",
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

                await _categoryService.ArchiveAsync(categoryToDelete.Id, CurrentUser.UserId.Value);

                MessageBox.Show(
                    "Категория успешно удалена!",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                await LoadCategoriesAsync();
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
                    errorMessage = "Ошибка при удалении категории: " + ex.Message;
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

        private async Task LoadCategoriesPageAsync(int page)
        {
            IsLoading = true;
            ErrorMessage = "";

            try
            {
                var filter = new CategoryFilter
                {
                    Page = page,
                    PageSize = PageSize,
                    IncludeArchived = false,
                    Name = string.IsNullOrWhiteSpace(NameFilter) ? null : NameFilter
                };

                var result = await _categoryService.GetPagedAsync(filter);

                Categories.Clear();
                foreach (var category in result.Items)
                {
                    Categories.Add(category);
                }

                TotalCount = result.TotalCount;
                TotalPages = result.TotalPages;
                CurrentPage = page;
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Ошибка при загрузке категорий: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnPageSizeChanged(int value)
        {
            CurrentPage = 1;
            _ = LoadCategoriesPageAsync(1);
        }

        partial void OnSelectedCategoryChanged(CategorySummary? value)
        {
            if (value != null)
            {
                // Можно добавить логику при выборе категории
            }
        }
    }
}

