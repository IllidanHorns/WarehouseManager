using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManager.Wpf.ViewModels;

namespace WarehouseManager.Wpf.View
{
    public partial class CreateCategoryWindow : Window
    {
        public CreateCategoryWindow()
        {
            InitializeComponent();
            var viewModel = App.ServiceProvider.GetRequiredService<CreateCategoryViewModel>();
            DataContext = viewModel;
        }
    }
}

