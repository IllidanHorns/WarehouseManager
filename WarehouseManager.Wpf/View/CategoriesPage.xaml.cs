using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManager.Services.Summary;
using WarehouseManager.Wpf.ViewModels;

namespace WarehouseManager.Wpf.View
{
    public partial class CategoriesPage : UserControl
    {
        public CategoriesPage()
        {
            InitializeComponent();
            var viewModel = App.ServiceProvider.GetRequiredService<CategoriesViewModel>();
            DataContext = viewModel;
            Loaded += CategoriesPage_Loaded;
        }

        private async void CategoriesPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is CategoriesViewModel viewModel)
            {
                await viewModel.LoadCategoriesAsync();
            }
        }

        private void CategoriesListViewItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item && item.Content is CategorySummary summary)
            {
                if (DataContext is CategoriesViewModel viewModel && viewModel.OpenUpdateWindowCommand.CanExecute(summary))
                {
                    viewModel.OpenUpdateWindowCommand.Execute(summary);
                }
            }
        }
    }
}

