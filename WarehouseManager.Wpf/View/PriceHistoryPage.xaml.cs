using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManager.Wpf.ViewModels;

namespace WarehouseManager.Wpf.View
{
    public partial class PriceHistoryPage : UserControl
    {
        private readonly PriceHistoryViewModel _viewModel;

        public PriceHistoryPage()
        {
            InitializeComponent();
            _viewModel = App.ServiceProvider.GetRequiredService<PriceHistoryViewModel>();
            DataContext = _viewModel;

            Loaded += async (s, e) =>
            {
                await _viewModel.LoadCategoriesAsync();
                await _viewModel.LoadPriceHistoriesAsync();
            };
        }
    }
}
