using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManager.Services.Summary;
using WarehouseManager.Wpf.ViewModels;

namespace WarehouseManager.Wpf.View
{
    /// <summary>
    /// Interaction logic for WarehousesPage.xaml
    /// </summary>
    public partial class WarehousesPage : UserControl
    {
        public WarehousesPage()
        {
            InitializeComponent();
            var viewModel = App.ServiceProvider.GetRequiredService<WarehousesViewModel>();
            DataContext = viewModel;
            Loaded += async (s, e) => await viewModel.LoadWarehousesAsync();
        }

        private void WarehousesListViewItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item && item.Content is WarehouseSummary summary)
            {
                if (DataContext is WarehousesViewModel viewModel && viewModel.OpenUpdateWindowCommand.CanExecute(summary))
                {
                    viewModel.OpenUpdateWindowCommand.Execute(summary);
                }
            }
        }
    }
}

