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
using WarehouseManagerContracts.DTOs.Product;

namespace WarehouseManager.Wpf.ViewModels
{
    public partial class UpdateProductViewModel : ObservableObject
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ProductSummary _product;

        [ObservableProperty]
        private string _productName = "";

        [ObservableProperty]
        private string _price = "";

        [ObservableProperty]
        private string _weight = "";

        [ObservableProperty]
        private int _selectedCategoryId;

        [ObservableProperty]
        private string _errorMessage = "";

        [ObservableProperty]
        private bool _isBusy;

        public ObservableCollection<CategorySummary> Categories { get; } = new();

        public UpdateProductViewModel(IProductService productService, ICategoryService categoryService, ProductSummary product)
        {
            _productService = productService;
            _categoryService = categoryService;
            _product = product;
            ProductName = product.Name;
            Price = product.Price.ToString("F2");
            Weight = product.Weight.ToString("F2");
            _ = LoadCategoriesAsync();
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var filter = new CategoryFilter
                {
                    Page = 1,
                    PageSize = 1000,
                    IncludeArchived = false
                };
                var result = await _categoryService.GetPagedAsync(filter);
                Categories.Clear();
                foreach (var category in result.Items)
                {
                    Categories.Add(category);
                }
                // Устанавливаем выбранную категорию по имени (потому что в ProductSummary только CategoryName)
                var selectedCategory = Categories.FirstOrDefault(c => c.Name == _product.CategoryName);
                if (selectedCategory != null)
                {
                    SelectedCategoryId = selectedCategory.Id;
                }
                else if (Categories.Any())
                {
                    SelectedCategoryId = Categories.First().Id;
                }
            }
            catch (System.Exception)
            {
                // Игнорируем ошибки загрузки категорий
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

                if (!decimal.TryParse(Price, out decimal priceValue) || priceValue <= 0)
                {
                    ErrorMessage = "Цена должна быть положительным числом";
                    return;
                }

                if (!decimal.TryParse(Weight, out decimal weightValue) || weightValue <= 0)
                {
                    ErrorMessage = "Вес должен быть положительным числом";
                    return;
                }

                var command = new UpdateProductCommand
                {
                    UserId = CurrentUser.UserId.Value,
                    Id = _product.Id,
                    ProductName = ProductName.Trim(),
                    Price = priceValue,
                    Weight = weightValue,
                    CategoryId = SelectedCategoryId
                };

                await _productService.UpdateAsync(command);

                MessageBox.Show("Продукт успешно обновлён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Закрыть окно
                System.Windows.Application.Current.Windows.OfType<View.UpdateProductWindow>().FirstOrDefault()?.Close();
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
            catch (System.Exception ex)
            {
                // Показываем более понятное сообщение для ошибок БД
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
            System.Windows.Application.Current.Windows.OfType<View.UpdateProductWindow>().FirstOrDefault()?.Close();
        }

        private bool CanSave()
        {
            if (IsBusy || string.IsNullOrWhiteSpace(ProductName) || string.IsNullOrWhiteSpace(Price) || string.IsNullOrWhiteSpace(Weight))
                return false;

            if (!decimal.TryParse(Price, out decimal price) || price <= 0)
                return false;

            if (!decimal.TryParse(Weight, out decimal weight) || weight <= 0)
                return false;

            if (SelectedCategoryId <= 0)
                return false;

            return true;
        }

        partial void OnIsBusyChanged(bool value) => SaveCommand.NotifyCanExecuteChanged();
        partial void OnProductNameChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
        partial void OnPriceChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
        partial void OnWeightChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
        partial void OnSelectedCategoryIdChanged(int value) => SaveCommand.NotifyCanExecuteChanged();

        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(ProductName) || e.PropertyName == nameof(Price) || 
                e.PropertyName == nameof(Weight) || e.PropertyName == nameof(SelectedCategoryId) || 
                e.PropertyName == nameof(IsBusy))
            {
                SaveCommand.NotifyCanExecuteChanged();
            }
        }
    }
}

