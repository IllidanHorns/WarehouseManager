using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManager.Wpf.ViewModels;
using WarehouseManager.Services.Summary;

namespace WarehouseManager.Wpf.View
{
    public partial class UpdateEmployeeWindow : Window
    {
        public UpdateEmployeeWindow(EmployeeSummary employee)
        {
            InitializeComponent();
            var employeeService = App.ServiceProvider.GetRequiredService<Services.Services.Interfaces.IEmployeeService>();
            var context = App.ServiceProvider.GetRequiredService<Core.Data.AppDbContext>();
            var viewModel = new UpdateEmployeeViewModel(employeeService, context, employee);
            DataContext = viewModel;
        }
    }
}

