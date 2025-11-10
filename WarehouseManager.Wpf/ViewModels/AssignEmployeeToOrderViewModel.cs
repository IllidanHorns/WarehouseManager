using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WarehouseManager.Services.Exceptions;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Services.Summary;
using WarehouseManager.Wpf.Static;
using WarehouseManagerContracts.DTOs.Order;
using Microsoft.EntityFrameworkCore;
using WarehouseManager.Core.Data;
using WarehouseManager.Core.Models;

namespace WarehouseManager.Wpf.ViewModels
{
    public partial class AssignEmployeeToOrderViewModel : ObservableObject
    {
        private readonly IOrderService _orderService;
        private readonly AppDbContext _context;
        private readonly OrderSummary _order;

        [ObservableProperty]
        private int _selectedEmployeeId;

        [ObservableProperty]
        private ObservableCollection<EmployeeSummary> _availableEmployees = new();

        [ObservableProperty]
        private string _errorMessage = "";

        [ObservableProperty]
        private bool _isBusy;

        public AssignEmployeeToOrderViewModel(IOrderService orderService, AppDbContext context, OrderSummary order)
        {
            _orderService = orderService;
            _context = context;
            _order = order;
            _ = LoadEmployeesAsync();
        }

        [RelayCommand]
        private async Task LoadEmployeesAsync()
        {
            try
            {
                // Загружаем заказ, чтобы узнать склад
                var orderEntity = await _context.Orders
                    .Include(o => o.Warehouse)
                    .FirstOrDefaultAsync(o => o.Id == _order.Id);

                if (orderEntity == null)
                {
                    ErrorMessage = "Заказ не найден";
                    return;
                }

                // Загружаем сотрудников, работающих на складе заказа
                var employeesOnWarehouse = await _context.EmployeesWarehouses
                    .Include(ew => ew.Employee)
                        .ThenInclude(e => e.User)
                    .Where(ew => ew.WarehouseId == orderEntity.WarehouseId && !ew.IsArchived && !ew.Employee.IsArchived)
                    .Select(ew => ew.Employee)
                    .ToListAsync();

                AvailableEmployees.Clear();

                // Преобразуем в EmployeeSummary
                foreach (var employee in employeesOnWarehouse)
                {
                    var user = employee.User;
                    var fullName = user != null
                        ? $"{user.FirstName} {user.MiddleName} {(string.IsNullOrEmpty(user.Patronymic) ? "" : user.Patronymic)}"
                        : "Неизвестен";

                    AvailableEmployees.Add(new EmployeeSummary
                    {
                        Id = employee.Id,
                        FullName = fullName,
                        Email = user?.Email ?? "Нет email"
                    });
                }

                // Если нет сотрудников на складе, загружаем всех активных сотрудников
                if (!AvailableEmployees.Any())
                {
                    var allEmployees = await _context.Employees
                        .Include(e => e.User)
                        .Where(e => !e.IsArchived)
                        .ToListAsync();

                    foreach (var employee in allEmployees)
                    {
                        var user = employee.User;
                        var fullName = user != null
                            ? $"{user.FirstName} {user.MiddleName} {(string.IsNullOrEmpty(user.Patronymic) ? "" : user.Patronymic)}"
                            : "Неизвестен";

                        AvailableEmployees.Add(new EmployeeSummary
                        {
                            Id = employee.Id,
                            FullName = fullName,
                            Email = user?.Email ?? "Нет email"
                        });
                    }
                }

                // Устанавливаем текущего сотрудника, если он есть
                if (!string.IsNullOrEmpty(_order.EmployeeFullName) && _order.EmployeeFullName != "Не назначен")
                {
                    var currentEmployee = AvailableEmployees.FirstOrDefault(e => e.FullName == _order.EmployeeFullName);
                    if (currentEmployee != null)
                    {
                        SelectedEmployeeId = currentEmployee.Id;
                    }
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Ошибка при загрузке сотрудников: " + ex.Message;
            }
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task SaveAsync()
        {
            ErrorMessage = "";
            IsBusy = true;

            try
            {
                var command = new AssignEmployeeToOrderCommand
                {
                    OrderId = _order.Id,
                    EmployeeId = SelectedEmployeeId,
                    UserId = CurrentUser.UserId
                };

                await _orderService.AssignEmployeeAsync(command);

                MessageBox.Show("Сотрудник успешно назначен на заказ!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                var window = System.Windows.Application.Current.Windows.OfType<View.AssignEmployeeToOrderWindow>().FirstOrDefault();
                if (window != null)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (DomainException ex)
            {
                ErrorMessage = ex.Message;
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
            System.Windows.Application.Current.Windows.OfType<View.AssignEmployeeToOrderWindow>().FirstOrDefault()?.Close();
        }

        private bool CanSave()
        {
            return !IsBusy && SelectedEmployeeId > 0;
        }

        partial void OnIsBusyChanged(bool value) => SaveCommand.NotifyCanExecuteChanged();
        partial void OnSelectedEmployeeIdChanged(int value) => SaveCommand.NotifyCanExecuteChanged();
    }
}

