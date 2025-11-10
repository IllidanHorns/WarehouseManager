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
using Microsoft.EntityFrameworkCore;
using WarehouseManager.Core.Data;

namespace WarehouseManager.Wpf.ViewModels
{
    public partial class EmployeesViewModel : ObservableObject
    {
        private readonly IEmployeeService _employeeService;
        private readonly IWarehouseService _warehouseService;
        private readonly AppDbContext _context;

        [ObservableProperty]
        private ObservableCollection<EmployeeSummary> _employees = new();

        [ObservableProperty]
        private EmployeeSummary? _selectedEmployee;

        private WarehouseSummary? _selectedWarehouse;

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

        public bool IsWarehouseSelected => SelectedWarehouse != null && SelectedWarehouse.Id != -1;

        private int? _selectedWarehouseId;

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
        public ObservableCollection<WarehouseSummary> Warehouses { get; } = new();

        public EmployeesViewModel(
            IEmployeeService employeeService,
            IWarehouseService warehouseService,
            AppDbContext context)
        {
            _employeeService = employeeService;
            _warehouseService = warehouseService;
            _context = context;
        }

        [RelayCommand]
        public async Task LoadEmployeesAsync()
        {
            await LoadEmployeesPageAsync(CurrentPage);
        }

        [RelayCommand]
        public async Task LoadWarehousesAsync()
        {
            try
            {
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
                ErrorMessage = "Ошибка при загрузке складов: " + ex.Message;
            }
        }

        [RelayCommand]
        private async Task FilterAsync()
        {
            CurrentPage = 1;
            await LoadEmployeesPageAsync(1);
        }

        [RelayCommand]
        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await LoadEmployeesPageAsync(CurrentPage);
            }
        }

        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                await LoadEmployeesPageAsync(CurrentPage);
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadEmployeesPageAsync(CurrentPage);
        }

        [RelayCommand]
        private void OpenCreateWindow()
        {
            var createWindow = new View.CreateEmployeeWindow();
            createWindow.ShowDialog();
            _ = LoadEmployeesAsync();
        }

        [RelayCommand]
        private void OpenUpdateWindow(EmployeeSummary? employee)
        {
            var employeeToEdit = employee ?? SelectedEmployee;
            if (employeeToEdit == null)
                return;

            SelectedEmployee = employeeToEdit;
            var updateWindow = new View.UpdateEmployeeWindow(employeeToEdit);
            updateWindow.ShowDialog();
            _ = LoadEmployeesAsync();
        }

        [RelayCommand]
        private async Task DeleteEmployeeAsync(EmployeeSummary? employee)
        {
            var employeeToDelete = employee ?? SelectedEmployee;
            if (employeeToDelete == null)
                return;

            // Подтверждение удаления
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить сотрудника?\n\nФИО: {employeeToDelete.FullName}\nEmail: {employeeToDelete.Email}\nЗарплата: {employeeToDelete.Salary}",
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

                await _employeeService.ArchiveAsync(employeeToDelete.Id, CurrentUser.UserId.Value);

                MessageBox.Show(
                    "Сотрудник успешно удалён!",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Перезагружаем список
                await LoadEmployeesAsync();
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
                    errorMessage = "Ошибка при удалении сотрудника: " + ex.Message;
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

        [RelayCommand]
        private void ClearWarehouseFilter()
        {
            SelectedWarehouse = null;
            _ = FilterAsync();
        }

        private async Task LoadEmployeesPageAsync(int page)
        {
            IsLoading = true;
            ErrorMessage = "";

            try
            {
                var filter = new EmployeeFilters
                {
                    Page = page,
                    PageSize = PageSize,
                    IncludeArchived = false,
                    WarehouseId = SelectedWarehouseId
                };

                var result = await _employeeService.GetPagedAsync(filter);

                Employees.Clear();
                foreach (var employee in result.Items)
                {
                    Employees.Add(employee);
                }

                TotalCount = result.TotalCount;
                TotalPages = result.TotalPages;
                CurrentPage = page;
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Ошибка при загрузке сотрудников: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnPageSizeChanged(int value)
        {
            CurrentPage = 1;
            _ = LoadEmployeesPageAsync(1);
        }

        partial void OnSelectedEmployeeChanged(EmployeeSummary? value)
        {
            if (value != null)
            {
                // Можно добавить логику при выборе сотрудника
            }
        }
    }
}

