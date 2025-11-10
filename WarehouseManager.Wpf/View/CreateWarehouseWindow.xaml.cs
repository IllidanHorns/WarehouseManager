using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManager.Wpf.ViewModels;

namespace WarehouseManager.Wpf.View
{
    /// <summary>
    /// Interaction logic for CreateWarehouseWindow.xaml
    /// </summary>
    public partial class CreateWarehouseWindow : Window
    {
        public CreateWarehouseWindow()
        {
            InitializeComponent();
            var viewModel = App.ServiceProvider.GetRequiredService<CreateWarehouseViewModel>();
            DataContext = viewModel;
        }
    }
}

