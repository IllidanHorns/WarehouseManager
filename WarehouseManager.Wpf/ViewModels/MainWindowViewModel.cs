using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManager.Wpf.Static;
using WarehouseManager.Wpf.View;
using System;

namespace WarehouseManager.Wpf.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<NavigationItem> _navigationItems = new();

        [ObservableProperty]
        private NavigationItem? _selectedNavigationItem;

        [ObservableProperty]
        private bool _isNavigationVisible = true;

        public MainWindowViewModel()
        {
            LoadNavigationItems();
        }

        private void LoadNavigationItems()
        {
            NavigationItems.Clear();

            if (CurrentUser.User == null)
                return;

            var roleName = CurrentUser.User.Role?.RoleName ?? "";

            switch (roleName)
            {
                case "Покупатель":
                    NavigationItems.Add(new NavigationItem { Name = "Каталог товаров", PageType = typeof(ProductCatalogPage) });
                    break;

                case "Администратор":
                    NavigationItems.Add(new NavigationItem { Name = "Пользователи", PageType = typeof(UsersPage) });
                    NavigationItems.Add(new NavigationItem { Name = "Сотрудники", PageType = typeof(EmployeesPage) });
                    NavigationItems.Add(new NavigationItem { Name = "Назначения сотрудников", PageType = typeof(EmployeeWarehousesPage) });
                    NavigationItems.Add(new NavigationItem { Name = "Аналитика", PageType = typeof(AnalyticsPage) });
                    break;

                case "Менеджер склада":
                    NavigationItems.Add(new NavigationItem { Name = "Склады", PageType = typeof(WarehousesPage) });
                    NavigationItems.Add(new NavigationItem { Name = "Продукты", PageType = typeof(ProductsPage) });
                    NavigationItems.Add(new NavigationItem { Name = "Категории", PageType = typeof(CategoriesPage) });
                    NavigationItems.Add(new NavigationItem { Name = "История цен", PageType = typeof(PriceHistoryPage) });
                    NavigationItems.Add(new NavigationItem { Name = "Заказы", PageType = typeof(OrdersPage) });
                    NavigationItems.Add(new NavigationItem { Name = "Остатки на складах", PageType = typeof(StocksPage) });
                    break;
            }

            // Выбираем первый элемент по умолчанию
            if (NavigationItems.Any())
            {
                SelectedNavigationItem = NavigationItems.First();
            }
        }

        [RelayCommand]
        private void ToggleNavigation()
        {
            IsNavigationVisible = !IsNavigationVisible;
        }

        [RelayCommand]
        private void Logout()
        {
            // Очищаем текущего пользователя
            CurrentUser.User = null;

            // Находим главное окно
            var mainWindow = System.Windows.Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            
            // Открываем окно авторизации
            var authViewModel = App.ServiceProvider.GetRequiredService<AuthViewModel>();
            var authWindow = new View.AuthWindow(authViewModel);
            authWindow.Show();

            // Закрываем главное окно после открытия окна авторизации
            if (mainWindow != null)
            {
                mainWindow.Close();
            }
        }

        partial void OnSelectedNavigationItemChanged(NavigationItem? value)
        {
            if (value != null && value.PageType != null)
            {
                // Событие будет обработано в MainWindow.xaml.cs через EventToCommand или через привязку
            }
        }
    }

    public class NavigationItem
    {
        public string Name { get; set; } = "";
        public System.Type? PageType { get; set; }
    }
}

