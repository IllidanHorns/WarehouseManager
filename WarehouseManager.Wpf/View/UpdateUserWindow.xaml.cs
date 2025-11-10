using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManager.Wpf.ViewModels;
using WarehouseManager.Services.Summary;

namespace WarehouseManager.Wpf.View
{
    public partial class UpdateUserWindow : Window
    {
        public UpdateUserWindow(UserSummary user)
        {
            InitializeComponent();
            var userService = App.ServiceProvider.GetRequiredService<Services.Services.Interfaces.IUserService>();
            var context = App.ServiceProvider.GetRequiredService<Core.Data.AppDbContext>();
            var viewModel = new UpdateUserViewModel(userService, context, user);
            DataContext = viewModel;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is UpdateUserViewModel viewModel && sender is PasswordBox passwordBox)
            {
                viewModel.NewPassword = passwordBox.Password;
            }
        }
    }
}

