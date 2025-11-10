using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WarehouseManager.Services.Exceptions;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Wpf.Static;
using WarehouseManagerContracts.DTOs.Category;

namespace WarehouseManager.Wpf.ViewModels
{
    public partial class CreateCategoryViewModel : ObservableObject
    {
        private readonly ICategoryService _categoryService;

        [ObservableProperty]
        private string _name = "";

        [ObservableProperty]
        private string? _description = "";

        [ObservableProperty]
        private string _errorMessage = "";

        [ObservableProperty]
        private bool _isBusy;

        public CreateCategoryViewModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task SaveAsync()
        {
            ErrorMessage = "";
            IsBusy = true;

            try
            {
                var command = new CreateCategoryCommand
                {
                    UserId = CurrentUser.UserId,
                    Name = Name.Trim(),
                    Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim()
                };

                await _categoryService.CreateAsync(command);
                
                MessageBox.Show("Категория успешно создана!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                
                System.Windows.Application.Current.Windows.OfType<View.CreateCategoryWindow>().FirstOrDefault()?.Close();
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
            System.Windows.Application.Current.Windows.OfType<View.CreateCategoryWindow>().FirstOrDefault()?.Close();
        }

        private bool CanSave()
        {
            if (IsBusy || string.IsNullOrWhiteSpace(Name))
                return false;
            
            return true;
        }

        partial void OnIsBusyChanged(bool value) => SaveCommand.NotifyCanExecuteChanged();
        partial void OnNameChanged(string value) => SaveCommand.NotifyCanExecuteChanged();

        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(Name) || e.PropertyName == nameof(IsBusy))
            {
                SaveCommand.NotifyCanExecuteChanged();
            }
        }
    }
}

