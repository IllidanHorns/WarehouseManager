using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManager.Wpf.ViewModels;

namespace WarehouseManager.Wpf.View;

public partial class AnalyticsPage : UserControl
{
    public AnalyticsPage()
    {
        InitializeComponent();
        var viewModel = App.ServiceProvider.GetRequiredService<AnalyticsViewModel>();
        DataContext = viewModel;
        Loaded += AnalyticsPage_Loaded;
    }

    private async void AnalyticsPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is AnalyticsViewModel viewModel)
        {
            await viewModel.LoadAsync();
        }
    }
}

