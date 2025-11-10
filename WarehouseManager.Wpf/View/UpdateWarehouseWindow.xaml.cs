using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManager.Services.Summary;
using WarehouseManager.Wpf.ViewModels;

namespace WarehouseManager.Wpf.View
{
    /// <summary>
    /// Interaction logic for UpdateWarehouseWindow.xaml
    /// </summary>
    public partial class UpdateWarehouseWindow : Window
    {
        public UpdateWarehouseWindow(WarehouseSummary warehouse)
        {
            InitializeComponent();
            var service = App.ServiceProvider.GetRequiredService<WarehouseManager.Services.Services.Interfaces.IWarehouseService>();
            var viewModel = new UpdateWarehouseViewModel(service, warehouse);
            DataContext = viewModel;
        }
    }
}

