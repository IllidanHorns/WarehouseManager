using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManager.Services.Summary;
using WarehouseManager.Wpf.ViewModels;

namespace WarehouseManager.Wpf.View
{
    /// <summary>
    /// Interaction logic for ProductsPage.xaml
    /// </summary>
    public partial class ProductsPage : UserControl
    {
        public ProductsPage()
        {
            InitializeComponent();
            var viewModel = App.ServiceProvider.GetRequiredService<ProductsViewModel>();
            DataContext = viewModel;
            Loaded += async (s, e) =>
            {
                await viewModel.LoadCategoriesAndWarehousesAsync();
                await viewModel.LoadProductsAsync();
            };
        }

        private void ProductsListViewItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item && item.Content is ProductSummary summary)
            {
                if (DataContext is ProductsViewModel viewModel && viewModel.OpenUpdateWindowCommand.CanExecute(summary))
                {
                    viewModel.OpenUpdateWindowCommand.Execute(summary);
                }
            }
        }
    }
}

