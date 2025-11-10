using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManager.Wpf.ViewModels;

namespace WarehouseManager.Wpf.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = App.ServiceProvider.GetRequiredService<MainWindowViewModel>();
            DataContext = _viewModel;
            
            // Подписываемся на изменение выбранного элемента навигации
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            
            // Загружаем первую страницу
            LoadPage(_viewModel.SelectedNavigationItem);
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainWindowViewModel.SelectedNavigationItem))
            {
                LoadPage(_viewModel.SelectedNavigationItem);
            }
        }

        private void LoadPage(NavigationItem? item)
        {
            if (item == null || item.PageType == null)
                return;

            MainGrid.Children.Clear();

            var page = System.Activator.CreateInstance(item.PageType) as UserControl;
            if (page != null)
            {
                MainGrid.Children.Add(page);
            }
        }
    }
}

