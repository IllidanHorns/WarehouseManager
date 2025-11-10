using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WarehouseManager.Services.Exceptions;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Services.Summary;
using WarehouseManager.Wpf.Static;
using WarehouseManager.Contracts.DTOs.Remaining;

namespace WarehouseManager.Wpf.ViewModels
{
    public partial class UpdateStockViewModel : ObservableObject
    {
        private readonly IStockService _stockService;
        private readonly WarehouseStockSummary _stock;

        [ObservableProperty]
        private string _productName = "";

        [ObservableProperty]
        private string _warehouseAddress = "";

        [ObservableProperty]
        private string _quantity = "";

        [ObservableProperty]
        private string _errorMessage = "";

        [ObservableProperty]
        private bool _isBusy;

        public UpdateStockViewModel(IStockService stockService, WarehouseStockSummary stock)
        {
            _stockService = stockService;
            _stock = stock;
            ProductName = stock.ProductName;
            WarehouseAddress = stock.WarehouseAddress;
            Quantity = stock.Quantity.ToString();
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

                if (!int.TryParse(Quantity, out int quantityValue) || quantityValue < 0)
                {
                    ErrorMessage = "Количество должно быть неотрицательным числом";
                    return;
                }

                var command = new UpdateStockCommand
                {
                    UserId = CurrentUser.UserId.Value,
                    RemainingId = _stock.Id,
                    NewQuantity = quantityValue
                };

                await _stockService.UpdateStockAsync(command);

                MessageBox.Show("Остаток успешно обновлён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                System.Windows.Application.Current.Windows
                    .OfType<View.UpdateStockWindow>()
                    .FirstOrDefault()?.Close();
            }
            catch (ModelValidationException ex)
            {
                var errors = ex.Errors;
                foreach (var error in errors)
                {
                    ErrorMessage += $"{error.ErrorMessage}\n";
                }
            }
            catch (DomainException ex)
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
            System.Windows.Application.Current.Windows
                .OfType<View.UpdateStockWindow>()
                .FirstOrDefault()?.Close();
        }

        private bool CanSave()
        {
            if (IsBusy || string.IsNullOrWhiteSpace(Quantity))
                return false;

            if (!int.TryParse(Quantity, out int quantity) || quantity < 0)
                return false;

            return true;
        }

        partial void OnIsBusyChanged(bool value) => SaveCommand.NotifyCanExecuteChanged();
        partial void OnQuantityChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
    }
}

