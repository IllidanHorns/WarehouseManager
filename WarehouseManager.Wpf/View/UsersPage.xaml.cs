using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManager.Services.Summary;
using WarehouseManager.Wpf.ViewModels;

namespace WarehouseManager.Wpf.View
{
    public partial class UsersPage : UserControl
    {
        public UsersPage()
        {
            InitializeComponent();
            var viewModel = App.ServiceProvider.GetRequiredService<UsersViewModel>();
            DataContext = viewModel;
            Loaded += UsersPage_Loaded;
        }

        private async void UsersPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is UsersViewModel viewModel)
            {
                await viewModel.LoadRolesAsync();
                await viewModel.LoadUsersAsync();
            }
        }

        private void UsersListViewItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item && item.Content is UserSummary summary)
            {
                if (DataContext is UsersViewModel viewModel && viewModel.OpenUpdateWindowCommand.CanExecute(summary))
                {
                    viewModel.OpenUpdateWindowCommand.Execute(summary);
                }
            }
        }
    }
}

