using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManager.Services.Summary;
using WarehouseManager.Wpf.ViewModels;

namespace WarehouseManager.Wpf.View
{
    public partial class StocksPage : UserControl
    {
        public StocksPage()
        {
            InitializeComponent();
            var viewModel = App.ServiceProvider.GetRequiredService<StocksViewModel>();
            DataContext = viewModel;
            Loaded += async (s, e) =>
            {
                await viewModel.LoadProductsAndWarehousesAsync();
                await viewModel.LoadStocksAsync();
            };
        }

        private void StocksListViewItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item && item.Content is WarehouseStockSummary summary)
            {
                if (DataContext is StocksViewModel viewModel && viewModel.OpenUpdateWindowCommand.CanExecute(summary))
                {
                    viewModel.OpenUpdateWindowCommand.Execute(summary);
                }
            }
        }
    }
}

