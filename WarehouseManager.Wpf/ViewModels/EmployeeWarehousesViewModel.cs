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
using WarehouseManager.Core.Models;

namespace WarehouseManager.Wpf.ViewModels
{
    public partial class EmployeeWarehousesViewModel : ObservableObject
    {
        private readonly IEmployeeWarehouseService _employeeWarehouseService;
        private readonly IEmployeeService _employeeService;
        private readonly IWarehouseService _warehouseService;
        private readonly AppDbContext _context;

        [ObservableProperty]
        private ObservableCollection<EmployeeWarehouseSummary> _employeeWarehouses = new();

        [ObservableProperty]
        private EmployeeWarehouseSummary? _selectedEmployeeWarehouse;

        private EmployeeSummary? _selectedEmployee;
        private WarehouseSummary? _selectedWarehouse;

        public EmployeeSummary? SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                _selectedEmployee = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEmployeeSelected));
                if (value != null && value.Id == -1)
                {
                    _selectedEmployee = null;
                    SelectedEmployeeId = null;
                    OnPropertyChanged(nameof(SelectedEmployee));
                }
                else
                {
                    SelectedEmployeeId = value?.Id;
                }
            }
        }

        public bool IsEmployeeSelected => SelectedEmployee != null && SelectedEmployee.Id != -1;

        public WarehouseSummary? SelectedWarehouse
        {
            get => _selectedWarehouse;
            set
            {
                _selectedWarehouse = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsWarehouseSelected));
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

        private int? _selectedEmployeeId;
        private int? _selectedWarehouseId;

        public int? SelectedEmployeeId
        {
            get => _selectedEmployeeId;
            set
            {
                _selectedEmployeeId = value;
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
        public ObservableCollection<EmployeeSummary> Employees { get; } = new();
        public ObservableCollection<WarehouseSummary> Warehouses { get; } = new();

        public EmployeeWarehousesViewModel(
            IEmployeeWarehouseService employeeWarehouseService,
            IEmployeeService employeeService,
            IWarehouseService warehouseService,
            AppDbContext context)
        {
            _employeeWarehouseService = employeeWarehouseService;
            _employeeService = employeeService;
            _warehouseService = warehouseService;
            _context = context;
        }

        [RelayCommand]
        public async Task LoadEmployeeWarehousesAsync()
        {
            await LoadEmployeeWarehousesPageAsync(CurrentPage);
        }

        [RelayCommand]
        public async Task LoadEmployeesAndWarehousesAsync()
        {
            try
            {
                var employeeFilter = new EmployeeFilters
                {
                    Page = 1,
                    PageSize = 1000,
                    IncludeArchived = false
                };
                var employeesResult = await _employeeService.GetPagedAsync(employeeFilter);
                Employees.Clear();
                Employees.Add(new EmployeeSummary { Id = -1, FullName = "Все сотрудники", Email = "" });
                foreach (var employee in employeesResult.Items)
                {
                    Employees.Add(employee);
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
                ErrorMessage = "Ошибка при загрузке данных: " + ex.Message;
            }
        }

        [RelayCommand]
        private async Task FilterAsync()
        {
            CurrentPage = 1;
            await LoadEmployeeWarehousesPageAsync(1);
        }

        [RelayCommand]
        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await LoadEmployeeWarehousesPageAsync(CurrentPage);
            }
        }

        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                await LoadEmployeeWarehousesPageAsync(CurrentPage);
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadEmployeeWarehousesPageAsync(CurrentPage);
        }

        [RelayCommand]
        private void OpenCreateWindow()
        {
            var createWindow = new View.CreateEmployeeWarehouseWindow();
            createWindow.ShowDialog();
            _ = LoadEmployeeWarehousesAsync();
        }

        [RelayCommand]
        private async Task DeleteEmployeeWarehouseAsync(EmployeeWarehouseSummary? employeeWarehouse)
        {
            var employeeWarehouseToDelete = employeeWarehouse ?? SelectedEmployeeWarehouse;
            if (employeeWarehouseToDelete == null)
                return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите снять назначение?\n\nСотрудник: {employeeWarehouseToDelete.EmployeeFullName}\nСклад: {employeeWarehouseToDelete.WarehouseAddress}",
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

                await _employeeWarehouseService.ArchiveAsync(employeeWarehouseToDelete.Id, CurrentUser.UserId.Value);

                MessageBox.Show(
                    "Назначение успешно снято!",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                await LoadEmployeeWarehousesAsync();
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
            catch (System.Exception ex)
            {
                ErrorMessage = "Ошибка при удалении назначения: " + ex.Message;
                MessageBox.Show(
                    "Ошибка при удалении назначения: " + ex.Message,
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
        private void ClearEmployeeFilter()
        {
            SelectedEmployee = null;
            _ = FilterAsync();
        }

        [RelayCommand]
        private void ClearWarehouseFilter()
        {
            SelectedWarehouse = null;
            _ = FilterAsync();
        }

        private async Task LoadEmployeeWarehousesPageAsync(int page)
        {
            IsLoading = true;
            ErrorMessage = "";

            try
            {
                var filter = new EmployeeWarehouseFilter
                {
                    Page = page,
                    PageSize = PageSize,
                    IncludeArchived = false,
                    EmployeeId = SelectedEmployeeId,
                    WarehouseId = SelectedWarehouseId
                };

                var result = await _employeeWarehouseService.GetPagedAsync(filter);

                EmployeeWarehouses.Clear();
                foreach (var item in result.Items)
                {
                    EmployeeWarehouses.Add(item);
                }

                TotalCount = result.TotalCount;
                TotalPages = result.TotalPages;
                CurrentPage = page;
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Ошибка при загрузке назначений: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnPageSizeChanged(int value)
        {
            CurrentPage = 1;
            _ = LoadEmployeeWarehousesPageAsync(1);
        }

    }
}

