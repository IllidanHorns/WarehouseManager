using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Services.Summary;
using WarehouseManager.Wpf.ViewModels;

namespace WarehouseManager.Wpf.View
{
    public partial class UpdateStockWindow : Window
    {
        public UpdateStockWindow(WarehouseStockSummary stock)
        {
            InitializeComponent();
            var stockService = App.ServiceProvider.GetRequiredService<IStockService>();
            var viewModel = new UpdateStockViewModel(stockService, stock);
            DataContext = viewModel;
        }
    }
}

