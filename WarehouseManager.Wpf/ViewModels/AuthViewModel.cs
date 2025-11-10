using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WarehouseManager.Services.Exceptions;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManagerContracts.DTOs.Auth;
using WarehouseManager.Wpf.Static;

namespace WarehouseManager.Wpf.ViewModels
{
    public partial class AuthViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        private string _email = "";

        [ObservableProperty]
        private string _password = "";

        [ObservableProperty]
        private string _errorMessage = "";

        [ObservableProperty]
        private bool _isBusy;

        public AuthViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task Login()
        {
            ErrorMessage = "";
            IsBusy = true;

            try
            {
                var command = new LoginCommand { Email = Email, Password = Password };
                var user = await _authService.AuthenticateAsync(command);
                CurrentUser.User = user;
                OnLoginSuccess?.Invoke(user);
            }
            catch (ModelValidationException ex)
            {
                var errors = ex.Errors;
                foreach (var error in errors) 
                {
                    ErrorMessage += $"{error} \n";
                }
            }
            catch (InvalidCredentialsException ex)
            {
                ErrorMessage = ex.Message;
            }
            catch (Exception ex) 
            {
                ErrorMessage = "Произошла ошибка: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanLogin() => !IsBusy;

        partial void OnIsBusyChanged(bool value) => LoginCommand.NotifyCanExecuteChanged();

        public event Action<object>? OnLoginSuccess;
    }
}




