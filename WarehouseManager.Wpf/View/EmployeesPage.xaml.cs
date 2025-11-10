using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManager.Services.Summary;
using WarehouseManager.Wpf.ViewModels;

namespace WarehouseManager.Wpf.View
{
    public partial class EmployeesPage : UserControl
    {
        public EmployeesPage()
        {
            InitializeComponent();
            var viewModel = App.ServiceProvider.GetRequiredService<EmployeesViewModel>();
            DataContext = viewModel;
            Loaded += EmployeesPage_Loaded;
        }

        private async void EmployeesPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is EmployeesViewModel viewModel)
            {
                await viewModel.LoadWarehousesAsync();
                await viewModel.LoadEmployeesAsync();
            }
        }

        private void EmployeesListViewItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item && item.Content is EmployeeSummary summary)
            {
                if (DataContext is EmployeesViewModel viewModel && viewModel.OpenUpdateWindowCommand.CanExecute(summary))
                {
                    viewModel.OpenUpdateWindowCommand.Execute(summary);
                }
            }
        }
    }
}

