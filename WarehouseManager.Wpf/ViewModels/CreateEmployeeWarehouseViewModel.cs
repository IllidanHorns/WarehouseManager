using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WarehouseManager.Services.Exceptions;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Wpf.Static;
using WarehouseManagerContracts.DTOs.EmployeeWarehouse;
using Microsoft.EntityFrameworkCore;
using WarehouseManager.Core.Data;
using System.Collections.ObjectModel;
using WarehouseManager.Services.Summary;

namespace WarehouseManager.Wpf.ViewModels
{
    public partial class CreateEmployeeWarehouseViewModel : ObservableObject
    {
        private readonly IEmployeeWarehouseService _employeeWarehouseService;
        private readonly IEmployeeService _employeeService;
        private readonly IWarehouseService _warehouseService;
        private readonly AppDbContext _context;

        [ObservableProperty]
        private int _selectedEmployeeId;

        [ObservableProperty]
        private int _selectedWarehouseId;

        [ObservableProperty]
        private string _errorMessage = "";

        [ObservableProperty]
        private bool _isBusy;

        public ObservableCollection<EmployeeSummary> AvailableEmployees { get; } = new();
        public ObservableCollection<WarehouseSummary> AvailableWarehouses { get; } = new();

        public CreateEmployeeWarehouseViewModel(
            IEmployeeWarehouseService employeeWarehouseService,
            IEmployeeService employeeService,
            IWarehouseService warehouseService,
            AppDbContext context)
        {
            _employeeWarehouseService = employeeWarehouseService;
            _employeeService = employeeService;
            _warehouseService = warehouseService;
            _context = context;
            _ = LoadDataAsync();
        }

        [RelayCommand]
        private async Task LoadDataAsync()
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
                AvailableEmployees.Clear();
                foreach (var employee in employeesResult.Items)
                {
                    AvailableEmployees.Add(employee);
                }
                if (AvailableEmployees.Any())
                {
                    SelectedEmployeeId = AvailableEmployees.First().Id;
                }

                var warehouseFilter = new WarehouseFilter
                {
                    Page = 1,
                    PageSize = 1000,
                    IncludeArchived = false
                };
                var warehousesResult = await _warehouseService.GetPagedAsync(warehouseFilter);
                AvailableWarehouses.Clear();
                foreach (var warehouse in warehousesResult.Items)
                {
                    AvailableWarehouses.Add(warehouse);
                }
                if (AvailableWarehouses.Any())
                {
                    SelectedWarehouseId = AvailableWarehouses.First().Id;
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Ошибка при загрузке данных: " + ex.Message;
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

                var command = new CreateEmployeeWarehouseCommand
                {
                    UserId = CurrentUser.UserId.Value,
                    EmployeeId = SelectedEmployeeId,
                    WarehouseId = SelectedWarehouseId
                };

                await _employeeWarehouseService.CreateAsync(command);
                
                MessageBox.Show("Назначение успешно создано!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                
                System.Windows.Application.Current.Windows.OfType<View.CreateEmployeeWarehouseWindow>().FirstOrDefault()?.Close();
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
                ErrorMessage = "Произошла ошибка: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            System.Windows.Application.Current.Windows.OfType<View.CreateEmployeeWarehouseWindow>().FirstOrDefault()?.Close();
        }

        private bool CanSave()
        {
            if (IsBusy || SelectedEmployeeId <= 0 || SelectedWarehouseId <= 0)
                return false;
            
            return true;
        }

        partial void OnIsBusyChanged(bool value) => SaveCommand.NotifyCanExecuteChanged();
        partial void OnSelectedEmployeeIdChanged(int value) => SaveCommand.NotifyCanExecuteChanged();
        partial void OnSelectedWarehouseIdChanged(int value) => SaveCommand.NotifyCanExecuteChanged();

        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(SelectedEmployeeId) || 
                e.PropertyName == nameof(SelectedWarehouseId) || 
                e.PropertyName == nameof(IsBusy))
            {
                SaveCommand.NotifyCanExecuteChanged();
            }
        }
    }
}

