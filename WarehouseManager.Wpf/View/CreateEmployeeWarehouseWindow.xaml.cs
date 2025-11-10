using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManager.Wpf.ViewModels;

namespace WarehouseManager.Wpf.View
{
    public partial class CreateEmployeeWarehouseWindow : Window
    {
        public CreateEmployeeWarehouseWindow()
        {
            InitializeComponent();
            var viewModel = App.ServiceProvider.GetRequiredService<CreateEmployeeWarehouseViewModel>();
            DataContext = viewModel;
        }
    }
}

