using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManager.Wpf.ViewModels;
using WarehouseManager.Services.Summary;
using WarehouseManager.Services.Services.Interfaces;

namespace WarehouseManager.Wpf.View
{
    public partial class UpdateCategoryWindow : Window
    {
        public UpdateCategoryWindow(CategorySummary category)
        {
            InitializeComponent();
            var categoryService = App.ServiceProvider.GetRequiredService<ICategoryService>();
            var viewModel = new UpdateCategoryViewModel(categoryService, category);
            DataContext = viewModel;
        }
    }
}

