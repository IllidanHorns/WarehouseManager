using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManager.Wpf.ViewModels;

namespace WarehouseManager.Wpf.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly RoutedCommand CreateItemCommand = new();
        public static readonly RoutedCommand EditItemCommand = new();
        public static readonly RoutedCommand DeleteItemCommand = new();
        public static readonly RoutedCommand RefreshCommand = new();
        public static readonly RoutedCommand FocusFilterCommand = new();
        public static readonly RoutedCommand ToggleNavigationCommand = new();
        public static readonly RoutedCommand ToggleThemeCommand = new();
        public static readonly RoutedCommand LogoutCommand = new();
        public static readonly RoutedCommand NavigatePrevCommand = new();
        public static readonly RoutedCommand NavigateNextCommand = new();

        private readonly MainWindowViewModel _viewModel;
        private UserControl? _currentPage;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = App.ServiceProvider.GetRequiredService<MainWindowViewModel>();
            DataContext = _viewModel;

            // Подписываемся на изменение выбранного элемента навигации
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            
            // Загружаем первую страницу
            LoadPage(_viewModel.SelectedNavigationItem);

            CommandBindings.Add(new CommandBinding(CreateItemCommand, (s, e) => ExecutePageCommand("создание", "OpenCreate", "Create", "Add")));
            CommandBindings.Add(new CommandBinding(EditItemCommand, (s, e) => ExecutePageCommand("редактирование", "OpenUpdate", "Edit", "Update")));
            CommandBindings.Add(new CommandBinding(DeleteItemCommand, (s, e) => ExecutePageCommand("удаление", "Delete")));
            CommandBindings.Add(new CommandBinding(RefreshCommand, (s, e) => ExecutePageCommand("обновление", "Refresh", "Load", "Reload")));
            CommandBindings.Add(new CommandBinding(FocusFilterCommand, (s, e) => FocusFirstFilterControl()));
            CommandBindings.Add(new CommandBinding(ToggleNavigationCommand, (s, e) => _viewModel.ToggleNavigationCommand.Execute(null)));
            CommandBindings.Add(new CommandBinding(ToggleThemeCommand, (s, e) => _viewModel.ToggleThemeCommand.Execute(null)));
            CommandBindings.Add(new CommandBinding(LogoutCommand, (s, e) => _viewModel.LogoutCommand.Execute(null)));
            CommandBindings.Add(new CommandBinding(NavigatePrevCommand, (s, e) => NavigateList(-1)));
            CommandBindings.Add(new CommandBinding(NavigateNextCommand, (s, e) => NavigateList(1)));
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
                _currentPage = page;
                MainGrid.Children.Add(page);
            }
        }

        private void NavigateList(int direction)
        {
            if (_viewModel.NavigationItems == null || !_viewModel.NavigationItems.Any())
                return;

            var currentIndex = _viewModel.NavigationItems.IndexOf(_viewModel.SelectedNavigationItem!);
            var newIndex = currentIndex + direction;

            if (newIndex >= 0 && newIndex < _viewModel.NavigationItems.Count)
            {
                _viewModel.SelectedNavigationItem = _viewModel.NavigationItems[newIndex];
            }
        }

        private void ExecutePageCommand(string actionDescription, params string[] keywords)
        {
            if (_currentPage?.DataContext == null)
            {
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            var success = TryExecuteCommand(_currentPage.DataContext, keywords) ||
                         TryInvokeMethod(_currentPage.DataContext, keywords);
            if (!success)
            {
                System.Media.SystemSounds.Beep.Play();
            }
        }

        private static bool TryExecuteCommand(object dataContext, string[] keywords)
        {
            var type = dataContext.GetType();
            var commands = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p =>
                {
                    object? value = null;
                    try
                    {
                        value = p.GetValue(dataContext);
                    }
                    catch
                    {
                        // ignore errors accessing property getters (e.g., throwing commands)
                    }

                    return new { p.Name, Command = value as ICommand };
                })
                .Where(item => item.Command != null);

            foreach (var keyword in keywords)
            {
                var candidate = commands.FirstOrDefault(c =>
                    c.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
                if (candidate?.Command == null)
                    continue;

                var command = candidate.Command;
                if (command.CanExecute(null))
                {
                    command.Execute(null);
                    return true;
                }
            }

            return false;
        }

        private void FocusFirstFilterControl()
        {
            if (_currentPage == null)
                return;

            var preferred = FindVisualDescendants<FrameworkElement>(_currentPage)
                .Where(e => e.IsVisible && e.IsEnabled)
                .ToList();

            FrameworkElement? target = preferred
                .FirstOrDefault(IsSearchElement)
                ?? preferred.FirstOrDefault(e => e is TextBox or ComboBox);

            if (target != null)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    target.Focus();
                    if (target is TextBox tb)
                    {
                        tb.SelectAll();
                    }
                });
            }
        }

        private static bool IsSearchElement(FrameworkElement element)
        {
            var name = element.Name ?? string.Empty;
            if (element is TextBox or ComboBox)
            {
                return name.Contains("Search", StringComparison.OrdinalIgnoreCase) ||
                       name.Contains("Filter", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        private static IEnumerable<T> FindVisualDescendants<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null)
                yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                {
                    yield return result;
                }

                foreach (var descendant in FindVisualDescendants<T>(child))
                {
                    yield return descendant;
                }
            }
        }

        private static bool TryInvokeMethod(object dataContext, string[] keywords)
        {
            var type = dataContext.GetType();
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var keyword in keywords)
            {
                var method = methods.FirstOrDefault(m =>
                    m.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) &&
                    m.GetParameters().Length <= 1);

                if (method == null)
                    continue;

                try
                {
                    object? result = method.GetParameters().Length switch
                    {
                        0 => method.Invoke(dataContext, null),
                        1 => method.Invoke(dataContext, new object?[] { null }),
                        _ => null
                    };

                    if (result is Task task)
                    {
                        _ = task;
                    }

                    return true;
                }
                catch
                {
                    // ignored
                }
            }

            return false;
        }
    }
}

