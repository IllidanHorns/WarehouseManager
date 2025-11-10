using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManager.Wpf.ViewModels;

namespace WarehouseManager.Wpf.View
{
    public partial class EmployeeWarehousesPage : UserControl
    {
        public EmployeeWarehousesPage()
        {
            InitializeComponent();
            var viewModel = App.ServiceProvider.GetRequiredService<EmployeeWarehousesViewModel>();
            DataContext = viewModel;
            Loaded += EmployeeWarehousesPage_Loaded;
        }

        private async void EmployeeWarehousesPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is EmployeeWarehousesViewModel viewModel)
            {
                await viewModel.LoadEmployeesAndWarehousesAsync();
                await viewModel.LoadEmployeeWarehousesAsync();
            }
        }
    }
}

