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
    public partial class UpdateOrderStatusViewModel : ObservableObject
    {
        private readonly IOrderService _orderService;
        private readonly AppDbContext _context;
        private readonly OrderSummary _order;

        [ObservableProperty]
        private int _selectedStatusId;

        [ObservableProperty]
        private ObservableCollection<OrderStatus> _availableStatuses = new();

        [ObservableProperty]
        private string _errorMessage = "";

        [ObservableProperty]
        private bool _isBusy;

        public UpdateOrderStatusViewModel(IOrderService orderService, AppDbContext context, OrderSummary order)
        {
            _orderService = orderService;
            _context = context;
            _order = order;
            _ = LoadStatusesAsync();
        }

        [RelayCommand]
        private async Task LoadStatusesAsync()
        {
            try
            {
                var statuses = await _context.OrderStatuses
                    .Where(s => !s.IsArchived)
                    .OrderBy(s => s.StatusName)
                    .ToListAsync();

                AvailableStatuses.Clear();
                foreach (var status in statuses)
                {
                    AvailableStatuses.Add(status);
                }

                // Находим текущий статус заказа
                var currentStatus = statuses.FirstOrDefault(s => s.StatusName == _order.OrderStatusName);
                if (currentStatus != null)
                {
                    SelectedStatusId = currentStatus.Id;
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Ошибка при загрузке статусов: " + ex.Message;
            }
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task SaveAsync()
        {
            ErrorMessage = "";
            IsBusy = true;

            try
            {
                var command = new UpdateOrderStatusCommand
                {
                    OrderId = _order.Id,
                    StatusId = SelectedStatusId,
                    UserId = CurrentUser.UserId
                };

                await _orderService.UpdateStatusAsync(command);

                MessageBox.Show("Статус заказа успешно обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                var window = System.Windows.Application.Current.Windows.OfType<View.UpdateOrderStatusWindow>().FirstOrDefault();
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
            System.Windows.Application.Current.Windows.OfType<View.UpdateOrderStatusWindow>().FirstOrDefault()?.Close();
        }

        private bool CanSave()
        {
            return !IsBusy && SelectedStatusId > 0;
        }

        partial void OnIsBusyChanged(bool value) => SaveCommand.NotifyCanExecuteChanged();
        partial void OnSelectedStatusIdChanged(int value) => SaveCommand.NotifyCanExecuteChanged();
    }
}

