using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManager.Services.Summary;
using WarehouseManager.Wpf.ViewModels;

namespace WarehouseManager.Wpf.View
{
    public partial class OrdersPage : UserControl
    {
        public OrdersPage()
        {
            InitializeComponent();
            var viewModel = App.ServiceProvider.GetRequiredService<OrdersViewModel>();
            DataContext = viewModel;
            Loaded += OrdersPage_Loaded;
        }

        private async void OrdersPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is OrdersViewModel viewModel)
            {
                await viewModel.LoadWarehousesAsync();
                await viewModel.LoadEmployeesAsync();
                await viewModel.LoadOrderStatusesAsync();
                await viewModel.LoadOrdersAsync();
            }
        }

        private void OrdersListViewItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item && item.Content is OrderSummary summary)
            {
                if (DataContext is OrdersViewModel viewModel && viewModel.UpdateStatusCommand.CanExecute(summary))
                {
                    viewModel.UpdateStatusCommand.Execute(summary);
                }
            }
        }
    }
}

