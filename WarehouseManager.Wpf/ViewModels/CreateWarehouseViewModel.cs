using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WarehouseManager.Services.Exceptions;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Wpf.Static;
using WarehouseManagerContracts.DTOs.Warehouse;

namespace WarehouseManager.Wpf.ViewModels
{
    public partial class CreateWarehouseViewModel : ObservableObject
    {
        private readonly IWarehouseService _warehouseService;

        [ObservableProperty]
        private string _address = "";

        [ObservableProperty]
        private string _square = "";

        [ObservableProperty]
        private string _errorMessage = "";

        [ObservableProperty]
        private bool _isBusy;

        public CreateWarehouseViewModel(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
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

                if (!int.TryParse(Square, out int squareValue) || squareValue <= 0)
                {
                    ErrorMessage = "Площадь должна быть положительным числом";
                    return;
                }

                var command = new CreateWarehouseCommand
                {
                    UserId = CurrentUser.UserId.Value,
                    Address = Address.Trim(),
                    Square = squareValue
                };

                await _warehouseService.CreateAsync(command);
                
                MessageBox.Show("Склад успешно создан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Закрыть окно
                System.Windows.Application.Current.Windows.OfType<View.CreateWarehouseWindow>().FirstOrDefault()?.Close();
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
                // Показываем более понятное сообщение для ошибок БД
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
            System.Windows.Application.Current.Windows.OfType<View.CreateWarehouseWindow>().FirstOrDefault()?.Close();
        }

        private bool CanSave()
        {
            if (IsBusy || string.IsNullOrWhiteSpace(Address) || string.IsNullOrWhiteSpace(Square))
                return false;
            
            if (!int.TryParse(Square, out int sq) || sq <= 0)
                return false;
            
            return true;
        }

        partial void OnIsBusyChanged(bool value) => SaveCommand.NotifyCanExecuteChanged();
        partial void OnAddressChanged(string value) => SaveCommand.NotifyCanExecuteChanged();

        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(Square) || e.PropertyName == nameof(Address) || e.PropertyName == nameof(IsBusy))
            {
                SaveCommand.NotifyCanExecuteChanged();
            }
        }
    }
}

