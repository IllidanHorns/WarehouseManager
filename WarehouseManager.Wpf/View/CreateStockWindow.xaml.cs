using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManager.Wpf.ViewModels;

namespace WarehouseManager.Wpf.View
{
    public partial class CreateStockWindow : Window
    {
        public CreateStockWindow()
        {
            InitializeComponent();
            var viewModel = App.ServiceProvider.GetRequiredService<CreateStockViewModel>();
            DataContext = viewModel;
        }
    }
}

