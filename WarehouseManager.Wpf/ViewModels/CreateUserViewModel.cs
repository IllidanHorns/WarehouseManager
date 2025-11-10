using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WarehouseManager.Services.Exceptions;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Wpf.Static;
using WarehouseManagerContracts.DTOs.User;
using Microsoft.EntityFrameworkCore;
using WarehouseManager.Core.Data;
using WarehouseManager.Core.Models;
using System.Collections.ObjectModel;

namespace WarehouseManager.Wpf.ViewModels
{
    public partial class CreateUserViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private readonly AppDbContext _context;

        [ObservableProperty]
        private string _email = "";

        [ObservableProperty]
        private string _password = "";

        [ObservableProperty]
        private string _firstName = "";

        [ObservableProperty]
        private string _middleName = "";

        [ObservableProperty]
        private string? _patronymic = "";

        [ObservableProperty]
        private string _phoneNumber = "";

        [ObservableProperty]
        private int _selectedRoleId;

        [ObservableProperty]
        private string _errorMessage = "";

        [ObservableProperty]
        private bool _isBusy;

        public ObservableCollection<Role> Roles { get; } = new();

        public CreateUserViewModel(IUserService userService, AppDbContext context)
        {
            _userService = userService;
            _context = context;
            _ = LoadRolesAsync();
        }

        [RelayCommand]
        private async Task LoadRolesAsync()
        {
            try
            {
                var roles = await _context.Roles
                    .Where(r => !r.IsArchived)
                    .OrderBy(r => r.RoleName)
                    .ToListAsync();

                Roles.Clear();
                foreach (var role in roles)
                {
                    Roles.Add(role);
                }

                if (Roles.Any())
                {
                    SelectedRoleId = Roles.First().Id;
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Ошибка при загрузке ролей: " + ex.Message;
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

                var command = new CreateUserCommand
                {
                    UserId = CurrentUser.UserId.Value,
                    Email = Email.Trim(),
                    Password = Password,
                    FirstName = FirstName.Trim(),
                    MiddleName = MiddleName.Trim(),
                    Patronymic = string.IsNullOrWhiteSpace(Patronymic) ? null : Patronymic.Trim(),
                    PhoneNumber = PhoneNumber.Trim(),
                    RoleId = SelectedRoleId
                };

                await _userService.CreateAsync(command);
                
                MessageBox.Show("Пользователь успешно создан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                
                System.Windows.Application.Current.Windows.OfType<View.CreateUserWindow>().FirstOrDefault()?.Close();
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
            System.Windows.Application.Current.Windows.OfType<View.CreateUserWindow>().FirstOrDefault()?.Close();
        }

        private bool CanSave()
        {
            if (IsBusy || 
                string.IsNullOrWhiteSpace(Email) || 
                string.IsNullOrWhiteSpace(Password) || 
                string.IsNullOrWhiteSpace(FirstName) || 
                string.IsNullOrWhiteSpace(MiddleName) || 
                string.IsNullOrWhiteSpace(PhoneNumber) ||
                SelectedRoleId <= 0)
                return false;
            
            return true;
        }

        partial void OnIsBusyChanged(bool value) => SaveCommand.NotifyCanExecuteChanged();
        partial void OnEmailChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
        partial void OnPasswordChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
        partial void OnFirstNameChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
        partial void OnMiddleNameChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
        partial void OnPhoneNumberChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
        partial void OnSelectedRoleIdChanged(int value) => SaveCommand.NotifyCanExecuteChanged();

        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(Email) || 
                e.PropertyName == nameof(Password) || 
                e.PropertyName == nameof(FirstName) || 
                e.PropertyName == nameof(MiddleName) || 
                e.PropertyName == nameof(PhoneNumber) || 
                e.PropertyName == nameof(SelectedRoleId) || 
                e.PropertyName == nameof(IsBusy))
            {
                SaveCommand.NotifyCanExecuteChanged();
            }
        }
    }
}

