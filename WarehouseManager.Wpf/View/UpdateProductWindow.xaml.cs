using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Services.Summary;
using WarehouseManager.Wpf.ViewModels;

namespace WarehouseManager.Wpf.View
{
    /// <summary>
    /// Interaction logic for UpdateProductWindow.xaml
    /// </summary>
    public partial class UpdateProductWindow : Window
    {
        public UpdateProductWindow(ProductSummary product)
        {
            InitializeComponent();
            var productService = App.ServiceProvider.GetRequiredService<IProductService>();
            var categoryService = App.ServiceProvider.GetRequiredService<ICategoryService>();
            var viewModel = new UpdateProductViewModel(productService, categoryService, product);
            DataContext = viewModel;
        }
    }
}

