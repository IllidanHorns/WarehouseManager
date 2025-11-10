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
using WarehouseManagerContracts.DTOs.Order;
using Microsoft.EntityFrameworkCore;
using WarehouseManager.Core.Data;
using WarehouseManager.Core.Models;

namespace WarehouseManager.Wpf.ViewModels
{
    public partial class OrdersViewModel : ObservableObject
    {
        private readonly IOrderService _orderService;
        private readonly IWarehouseService _warehouseService;
        private readonly IEmployeeService _employeeService;
        private readonly AppDbContext _context;

        [ObservableProperty]
        private ObservableCollection<OrderSummary> _orders = new();

        [ObservableProperty]
        private OrderSummary? _selectedOrder;

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

        // Фильтры
        [ObservableProperty]
        private ObservableCollection<WarehouseSummary> _warehouses = new();

        [ObservableProperty]
        private ObservableCollection<EmployeeSummary> _employees = new();

        [ObservableProperty]
        private ObservableCollection<OrderStatus> _orderStatuses = new();

        private WarehouseSummary? _selectedWarehouse;
        private EmployeeSummary? _selectedEmployee;
        private OrderStatus? _selectedStatus;

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

        private int? _selectedEmployeeId;
        public int? SelectedEmployeeId
        {
            get => _selectedEmployeeId;
            set
            {
                _selectedEmployeeId = value;
                OnPropertyChanged();
            }
        }

        public OrderStatus? SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                _selectedStatus = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsStatusSelected));
                if (value != null && value.Id == -1)
                {
                    _selectedStatus = null;
                    SelectedStatusId = null;
                    OnPropertyChanged(nameof(SelectedStatus));
                }
                else
                {
                    SelectedStatusId = value?.Id;
                }
            }
        }

        public bool IsStatusSelected => SelectedStatus != null && SelectedStatus.Id != -1;

        private int? _selectedStatusId;
        public int? SelectedStatusId
        {
            get => _selectedStatusId;
            set
            {
                _selectedStatusId = value;
                OnPropertyChanged();
            }
        }

        public OrdersViewModel(
            IOrderService orderService,
            IWarehouseService warehouseService,
            IEmployeeService employeeService,
            AppDbContext context)
        {
            _orderService = orderService;
            _warehouseService = warehouseService;
            _employeeService = employeeService;
            _context = context;
        }

        [RelayCommand]
        public async Task LoadOrdersAsync()
        {
            await LoadOrdersPageAsync(CurrentPage);
        }

        [RelayCommand]
        public async Task LoadWarehousesAsync()
        {
            try
            {
                var warehouses = await _warehouseService.GetPagedAsync(new WarehouseFilter
                {
                    Page = 1,
                    PageSize = 1000,
                    IncludeArchived = false
                });

                Warehouses.Clear();
                // Добавляем опцию "Все склады"
                Warehouses.Add(new WarehouseSummary { Id = -1, Address = "Все склады" });
                foreach (var warehouse in warehouses.Items)
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
        public async Task LoadEmployeesAsync()
        {
            try
            {
                var employees = await _employeeService.GetPagedAsync(new EmployeeFilters
                {
                    Page = 1,
                    PageSize = 1000,
                    IncludeArchived = false
                });

                Employees.Clear();
                // Добавляем опцию "Все сотрудники"
                Employees.Add(new EmployeeSummary { Id = -1, FullName = "Все сотрудники" });
                foreach (var employee in employees.Items)
                {
                    Employees.Add(employee);
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Ошибка при загрузке сотрудников: " + ex.Message;
            }
        }

        [RelayCommand]
        public async Task LoadOrderStatusesAsync()
        {
            try
            {
                var statuses = await _context.OrderStatuses
                    .Where(s => !s.IsArchived)
                    .OrderBy(s => s.StatusName)
                    .ToListAsync();

                OrderStatuses.Clear();
                // Добавляем опцию "Все статусы"
                OrderStatuses.Add(new OrderStatus { Id = -1, StatusName = "Все статусы" });
                foreach (var status in statuses)
                {
                    OrderStatuses.Add(status);
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Ошибка при загрузке статусов: " + ex.Message;
            }
        }

        [RelayCommand]
        private void ClearWarehouseFilter()
        {
            SelectedWarehouse = null;
            SelectedWarehouseId = null;
            _ = FilterAsync();
        }

        [RelayCommand]
        private void ClearEmployeeFilter()
        {
            SelectedEmployee = null;
            SelectedEmployeeId = null;
            _ = FilterAsync();
        }

        [RelayCommand]
        private void ClearStatusFilter()
        {
            SelectedStatus = null;
            SelectedStatusId = null;
            _ = FilterAsync();
        }

        [RelayCommand]
        private async Task FilterAsync()
        {
            CurrentPage = 1;
            await LoadOrdersPageAsync(1);
        }

        [RelayCommand]
        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await LoadOrdersPageAsync(CurrentPage);
            }
        }

        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                await LoadOrdersPageAsync(CurrentPage);
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadOrdersPageAsync(CurrentPage);
        }

        [RelayCommand]
        private async Task UpdateStatusAsync(OrderSummary? order)
        {
            var orderToUpdate = order ?? SelectedOrder;
            if (orderToUpdate == null)
                return;

            var updateWindow = new View.UpdateOrderStatusWindow(orderToUpdate);
            if (updateWindow.ShowDialog() == true)
            {
                await LoadOrdersAsync();
            }
        }

        [RelayCommand]
        private async Task AssignEmployeeAsync(OrderSummary? order)
        {
            var orderToUpdate = order ?? SelectedOrder;
            if (orderToUpdate == null)
                return;

            var assignWindow = new View.AssignEmployeeToOrderWindow(orderToUpdate);
            if (assignWindow.ShowDialog() == true)
            {
                await LoadOrdersAsync();
            }
        }

        private async Task LoadOrdersPageAsync(int page)
        {
            IsLoading = true;
            ErrorMessage = "";

            try
            {
                var filter = new OrderFilter
                {
                    Page = page,
                    PageSize = PageSize,
                    IncludeArchived = false,
                    WarehouseId = SelectedWarehouseId,
                    EmployeeId = SelectedEmployeeId,
                    StatusId = SelectedStatusId
                };

                var result = await _orderService.GetPagedAsync(filter);

                Orders.Clear();
                foreach (var order in result.Items)
                {
                    Orders.Add(order);
                }

                TotalCount = result.TotalCount;
                TotalPages = result.TotalPages;
                CurrentPage = page;
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Ошибка при загрузке заказов: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnPageSizeChanged(int value)
        {
            CurrentPage = 1;
            _ = LoadOrdersPageAsync(1);
        }
    }
}

