using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using WarehouseManager.Services;
using WarehouseManager.Services.Exceptions;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Services.Summary;
using WarehouseManager.Wpf.Static;

namespace WarehouseManager.Wpf.ViewModels
{
    public partial class WarehousesViewModel : ObservableObject
    {
        private readonly IWarehouseService _warehouseService;

        [ObservableProperty]
        private ObservableCollection<WarehouseSummary> _warehouses = new();

        [ObservableProperty]
        private WarehouseSummary? _selectedWarehouse;

        [ObservableProperty]
        private string _addressFilter = "";

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

        public WarehousesViewModel(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        [RelayCommand]
        public async Task LoadWarehousesAsync()
        {
            await LoadWarehousesPageAsync(CurrentPage);
        }

        [RelayCommand]
        private async Task FilterAsync()
        {
            CurrentPage = 1;
            await LoadWarehousesPageAsync(1);
        }

        [RelayCommand]
        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await LoadWarehousesPageAsync(CurrentPage);
            }
        }

        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                await LoadWarehousesPageAsync(CurrentPage);
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadWarehousesPageAsync(CurrentPage);
        }

        [RelayCommand]
        private void OpenCreateWindow()
        {
            var createWindow = new View.CreateWarehouseWindow();
            createWindow.ShowDialog();
            _ = LoadWarehousesAsync();
        }

        [RelayCommand]
        private void OpenUpdateWindow(WarehouseSummary? warehouse)
        {
            var warehouseToEdit = warehouse ?? SelectedWarehouse;
            if (warehouseToEdit == null)
                return;

            SelectedWarehouse = warehouseToEdit;
            var updateWindow = new View.UpdateWarehouseWindow(warehouseToEdit);
            updateWindow.ShowDialog();
            _ = LoadWarehousesAsync();
        }

        [RelayCommand]
        private async Task DeleteWarehouseAsync(WarehouseSummary? warehouse)
        {
            var warehouseToDelete = warehouse ?? SelectedWarehouse;
            if (warehouseToDelete == null)
                return;

            // Подтверждение удаления
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить склад?\n\nАдрес: {warehouseToDelete.Address}\nПлощадь: {warehouseToDelete.Square} м²",
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

                await _warehouseService.ArchiveAsync(warehouseToDelete.Id, CurrentUser.UserId.Value);

                MessageBox.Show(
                    "Склад успешно удалён!",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Перезагружаем список
                await LoadWarehousesAsync();
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
                    errorMessage = "Ошибка при удалении склада: " + ex.Message;
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

        private async Task LoadWarehousesPageAsync(int page)
        {
            IsLoading = true;
            ErrorMessage = "";

            try
            {
                var filter = new WarehouseFilter
                {
                    Page = page,
                    PageSize = PageSize,
                    IncludeArchived = false,
                    Address = string.IsNullOrWhiteSpace(AddressFilter) ? null : AddressFilter
                };

                var result = await _warehouseService.GetPagedAsync(filter);

                Warehouses.Clear();
                foreach (var warehouse in result.Items)
                {
                    Warehouses.Add(warehouse);
                }

                TotalCount = result.TotalCount;
                TotalPages = result.TotalPages;
                CurrentPage = page; // Используем переданный page, а не result.Page
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Ошибка при загрузке складов: " + ex.Message;
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
            _ = LoadWarehousesPageAsync(1);
        }

        partial void OnSelectedWarehouseChanged(WarehouseSummary? value)
        {
            if (value != null)
            {
                // Можно добавить логику при выборе склада
            }
        }
    }
}

