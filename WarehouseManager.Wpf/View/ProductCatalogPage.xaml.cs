using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManager.Services.Summary;
using WarehouseManager.Wpf.ViewModels;

namespace WarehouseManager.Wpf.View
{
    public partial class ProductCatalogPage : UserControl
    {
        private ProductCatalogViewModel? _viewModel;

        public ProductCatalogPage()
        {
            InitializeComponent();
            _viewModel = App.ServiceProvider.GetRequiredService<ProductCatalogViewModel>();
            DataContext = _viewModel;
            Loaded += async (s, e) =>
            {
                if (_viewModel != null)
                {
                    await _viewModel.LoadCategoriesAndWarehousesAsync();
                    await _viewModel.LoadProductsAsync();
                }
            };
        }

        private async void WarehouseComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.Tag is ProductSummary product && _viewModel != null)
            {
                if (comboBox.SelectedValue is int warehouseId && warehouseId > 0)
                {
                    _viewModel.SelectedWarehouseForProduct = warehouseId;
                    _viewModel.SelectedProduct = product;
                    await _viewModel.CheckAvailabilityAsync(product);
                    UpdateAvailabilityText(comboBox, _viewModel.AvailableQuantity);
                }
            }
        }

        private async void WarehouseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.Tag is ProductSummary product && _viewModel != null)
            {
                if (comboBox.SelectedValue is int warehouseId && warehouseId > 0)
                {
                    UpdateAvailabilityText(comboBox, -1);

                    _viewModel.SelectedWarehouseForProduct = warehouseId;
                    _viewModel.SelectedProduct = product;
                    await _viewModel.CheckAvailabilityAsync(product);

                    UpdateAvailabilityText(comboBox, _viewModel.AvailableQuantity);
                }
                else
                {
                    UpdateAvailabilityText(comboBox, 0);
                }
            }
        }

        private void UpdateAvailabilityText(DependencyObject referenceElement, int availableQuantity)
        {
            var cardGrid = FindAncestor<Grid>(referenceElement);
            if (cardGrid == null)
                return;

            var availableText = FindChild<TextBlock>(cardGrid, "AvailableQuantityText");
            if (availableText != null)
            {
                availableText.Text = availableQuantity == -1
                    ? "Загрузка..."
                    : $"Доступно: {availableQuantity}";
            }
        }

        private async void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ProductSummary product && _viewModel != null)
            {
                var quantityGrid = FindAncestor<Grid>(button);
                var cardGrid = quantityGrid != null ? FindAncestor<Grid>(quantityGrid) : null;
                if (quantityGrid == null || cardGrid == null)
                    return;

                var warehouseCombo = FindChild<ComboBox>(cardGrid, "WarehouseComboBox");
                var quantityBox = FindChild<TextBox>(cardGrid, "QuantityTextBox");

                if (warehouseCombo?.SelectedValue is int warehouseId && warehouseId > 0)
                {
                    _viewModel.SelectedWarehouseForProduct = warehouseId;
                    _viewModel.SelectedProduct = product;

                    if (quantityBox != null && int.TryParse(quantityBox.Text, out int quantity) && quantity > 0)
                    {
                        _viewModel.SelectedQuantity = quantity;
                    }
                    else
                    {
                        MessageBox.Show("Количество должно быть положительным числом", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    await _viewModel.AddToCartAsync(product);
                    UpdateAvailabilityText(warehouseCombo, _viewModel.AvailableQuantity);
                }
                else
                {
                    MessageBox.Show("Выберите склад", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
        {
            while (current != null)
            {
                current = VisualTreeHelper.GetParent(current);
                if (current is T target)
                    return target;
            }

            return null;
        }

        private static T? FindChild<T>(DependencyObject? parent, string childName) where T : FrameworkElement
        {
            if (parent == null)
                return null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T childElement && (string.IsNullOrEmpty(childName) || childElement.Name == childName))
                    return childElement;

                var result = FindChild<T>(child, childName);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}

