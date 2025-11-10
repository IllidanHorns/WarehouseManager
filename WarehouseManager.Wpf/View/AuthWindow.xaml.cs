using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WarehouseManager.Application.Services;
using WarehouseManager.Wpf.View;
using WarehouseManager.Wpf.ViewModels;

namespace WarehouseManager.Wpf.View
{
    /// <summary>
    /// Логика взаимодействия для AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        public AuthWindow(AuthViewModel viewModel)
        {
            InitializeComponent();
            viewModel.OnLoginSuccess += (user) =>
            {
                // Открываем главное окно
                var mainWindow = new MainWindow();
                mainWindow.Show();
                
                // Закрываем окно авторизации
                this.Close();
            };
            DataContext = viewModel;
        }

        private void EmailBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
                textBox.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#123C69"));
        }

        private void EmailBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
                textBox.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BAB2B5"));
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox != null)
                passwordBox.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#123C69"));
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox != null)
                passwordBox.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BAB2B5"));
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is AuthViewModel viewModel)
            {
                viewModel.Password = ((PasswordBox)sender).Password;
            }
        }

    }
}
