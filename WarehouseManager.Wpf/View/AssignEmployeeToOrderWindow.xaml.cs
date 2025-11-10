using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManager.Wpf.ViewModels;
using WarehouseManager.Services.Summary;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Core.Data;

namespace WarehouseManager.Wpf.View
{
    public partial class AssignEmployeeToOrderWindow : Window
    {
        public AssignEmployeeToOrderWindow(OrderSummary order)
        {
            InitializeComponent();
            var orderService = App.ServiceProvider.GetRequiredService<IOrderService>();
            var context = App.ServiceProvider.GetRequiredService<AppDbContext>();
            var viewModel = new AssignEmployeeToOrderViewModel(orderService, context, order);
            DataContext = viewModel;
        }
    }
}

