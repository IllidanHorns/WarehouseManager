using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManager.Wpf.ViewModels;

namespace WarehouseManager.Wpf.View
{
    public partial class CheckoutWindow : Window
    {
        public CheckoutWindow()
        {
            InitializeComponent();
            var viewModel = App.ServiceProvider.GetRequiredService<CheckoutViewModel>();
            DataContext = viewModel;
        }
    }
}

