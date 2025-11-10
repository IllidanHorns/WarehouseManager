using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManager.Wpf.Models;
using WarehouseManager.Wpf.ViewModels;

namespace WarehouseManager.Wpf.View
{
    public partial class CartWindow : Window
    {
        private CartViewModel? _viewModel;

        public CartWindow()
        {
            InitializeComponent();
            _viewModel = App.ServiceProvider.GetRequiredService<CartViewModel>();
            DataContext = _viewModel;
        }

        private async void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is CartItem item && _viewModel != null)
            {
                var newQuantity = Math.Max(1, item.Quantity - 1);
                item.Quantity = newQuantity;
                await _viewModel.UpdateQuantityAsync(item);
            }
        }

        private async void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is CartItem item && _viewModel != null)
            {
                var newQuantity = item.Quantity + 1;
                item.Quantity = newQuantity;
                await _viewModel.UpdateQuantityAsync(item);
            }
        }

        private async void QuantityTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Tag is CartItem item && _viewModel != null)
            {
                if (int.TryParse(textBox.Text, out int quantity) && quantity > 0)
                {
                    item.Quantity = quantity;
                    await _viewModel.UpdateQuantityAsync(item);
                }
                else
                {
                    textBox.Text = item.Quantity.ToString();
                }
            }
        }
    }
}

