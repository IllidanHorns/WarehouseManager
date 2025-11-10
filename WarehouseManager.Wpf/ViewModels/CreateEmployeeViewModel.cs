using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WarehouseManager.Services.Exceptions;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Wpf.Static;
using WarehouseManagerContracts.DTOs.Employee;
using Microsoft.EntityFrameworkCore;
using WarehouseManager.Core.Data;
using WarehouseManager.Core.Models;
using System.Collections.ObjectModel;
using System;

namespace WarehouseManager.Wpf.ViewModels
{
    public partial class CreateEmployeeViewModel : ObservableObject
    {
        private readonly IEmployeeService _employeeService;
        private readonly AppDbContext _context;

        [ObservableProperty]
        private string _salary = "";

        [ObservableProperty]
        private DateTime _dateOfBirth = DateTime.Now.AddYears(-25);

        [ObservableProperty]
        private int _selectedUserId;

        [ObservableProperty]
        private string _errorMessage = "";

        [ObservableProperty]
        private bool _isBusy;

        public ObservableCollection<User> AvailableUsers { get; } = new();

        public CreateEmployeeViewModel(IEmployeeService employeeService, AppDbContext context)
        {
            _employeeService = employeeService;
            _context = context;
            _ = LoadUsersAsync();
        }

        [RelayCommand]
        private async Task LoadUsersAsync()
        {
            try
            {
                // Загружаем только пользователей, которые еще не являются сотрудниками (включая архивированных)
                var existingEmployeeUserIds = await _context.Employees
                    .Select(e => e.UserId)
                    .ToListAsync();

                var users = await _context.Users
                    .Where(u => !u.IsArchived && !existingEmployeeUserIds.Contains(u.Id))
                    .OrderBy(u => u.FirstName)
                    .ThenBy(u => u.MiddleName)
                    .ToListAsync();

                AvailableUsers.Clear();
                foreach (var user in users)
                {
                    AvailableUsers.Add(user);
                }

                if (AvailableUsers.Any())
                {
                    SelectedUserId = AvailableUsers.First().Id;
                }
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

                var command = new CreateEmployeeCommand
                {
                    UserId = SelectedUserId,
                    TargetUserId = CurrentUser.UserId.Value,
                    Salary = salaryValue,
                    DateOfBirth = dateOfBirth
                };

                await _employeeService.CreateAsync(command);
                
                MessageBox.Show("Сотрудник успешно создан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                
                System.Windows.Application.Current.Windows.OfType<View.CreateEmployeeWindow>().FirstOrDefault()?.Close();
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
            System.Windows.Application.Current.Windows.OfType<View.CreateEmployeeWindow>().FirstOrDefault()?.Close();
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

