using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WarehouseManager.Services.Exceptions;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Services.Summary;
using WarehouseManager.Wpf.Static;
using WarehouseManagerContracts.DTOs.Employee;
using Microsoft.EntityFrameworkCore;
using WarehouseManager.Core.Data;
using WarehouseManager.Core.Models;
using System.Collections.ObjectModel;
using System;

namespace WarehouseManager.Wpf.ViewModels
{
    public partial class UpdateEmployeeViewModel : ObservableObject
    {
        private readonly IEmployeeService _employeeService;
        private readonly AppDbContext _context;
        private readonly EmployeeSummary _employee;

        [ObservableProperty]
        private string _salary = "";

        [ObservableProperty]
        private DateTime _dateOfBirth;

        [ObservableProperty]
        private int _selectedUserId;

        [ObservableProperty]
        private string _errorMessage = "";

        [ObservableProperty]
        private bool _isBusy;

        public ObservableCollection<User> AvailableUsers { get; } = new();

        public UpdateEmployeeViewModel(IEmployeeService employeeService, AppDbContext context, EmployeeSummary employee)
        {
            _employeeService = employeeService;
            _context = context;
            _employee = employee;
            Salary = employee.Salary.ToString("F2");
            DateOfBirth = employee.DateOfBirth.ToDateTime(TimeOnly.MinValue);
            _ = LoadUsersAsync();
        }

        [RelayCommand]
        private async Task LoadUsersAsync()
        {
            try
            {
                // Получаем текущего сотрудника с его UserId
                var employeeEntity = await _context.Employees
                    .FirstOrDefaultAsync(e => e.Id == _employee.Id);
                
                if (employeeEntity == null)
                {
                    ErrorMessage = "Сотрудник не найден";
                    return;
                }

                var currentEmployeeUserId = employeeEntity.UserId;

                // Загружаем всех активных пользователей, включая текущего пользователя сотрудника
                // Исключаем только тех, кто уже является другими сотрудниками
                var existingEmployeeUserIds = await _context.Employees
                    .Where(e => !e.IsArchived && e.Id != _employee.Id)
                    .Select(e => e.UserId)
                    .ToListAsync();

                var users = await _context.Users
                    .Where(u => !u.IsArchived && (!existingEmployeeUserIds.Contains(u.Id) || u.Id == currentEmployeeUserId))
                    .OrderBy(u => u.FirstName)
                    .ThenBy(u => u.MiddleName)
                    .ToListAsync();

                AvailableUsers.Clear();
                foreach (var user in users)
                {
                    AvailableUsers.Add(user);
                }

                // Устанавливаем текущего пользователя сотрудника
                SelectedUserId = currentEmployeeUserId;
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Ошибка при загрузке пользователей: " + ex.Message;
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

                if (!decimal.TryParse(Salary, out decimal salaryValue) || salaryValue < 0)
                {
                    ErrorMessage = "Зарплата должна быть положительным числом";
                    return;
                }

                var dateOfBirth = DateOnly.FromDateTime(DateOfBirth);

                var command = new UpdateEmployeeCommand
                {
                    UserId = SelectedUserId, // ID пользователя сотрудника
                    TargetUserId = CurrentUser.UserId.Value, // ID пользователя, выполняющего операцию
                    EmployeeId = _employee.Id,
                    Salary = salaryValue,
                    DateOfBirth = dateOfBirth
                };

                await _employeeService.UpdateAsync(command);
                
                MessageBox.Show("Сотрудник успешно обновлён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                
                System.Windows.Application.Current.Windows.OfType<View.UpdateEmployeeWindow>().FirstOrDefault()?.Close();
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
            System.Windows.Application.Current.Windows.OfType<View.UpdateEmployeeWindow>().FirstOrDefault()?.Close();
        }

        private bool CanSave()
        {
            if (IsBusy || 
                string.IsNullOrWhiteSpace(Salary) || 
                SelectedUserId <= 0)
                return false;
            
            if (!decimal.TryParse(Salary, out decimal salaryValue) || salaryValue < 0)
                return false;
            
            return true;
        }

        partial void OnIsBusyChanged(bool value) => SaveCommand.NotifyCanExecuteChanged();
        partial void OnSalaryChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
        partial void OnSelectedUserIdChanged(int value) => SaveCommand.NotifyCanExecuteChanged();

        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(Salary) || 
                e.PropertyName == nameof(SelectedUserId) || 
                e.PropertyName == nameof(IsBusy))
            {
                SaveCommand.NotifyCanExecuteChanged();
            }
        }
    }
}

