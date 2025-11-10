using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;
using WarehouseManager.Application.Services;
using WarehouseManager.Core.Data;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Wpf.View;
using WarehouseManager.Wpf.ViewModels;

namespace WarehouseManager.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer("Server=LAPTOP-J0029HVK\\SQLEXPRESS;Database=WarehouseManager;Trusted_Connection=true;MultipleActiveResultSets=true;Encrypt=True;TrustServerCertificate=True"));

            // Services
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IWarehouseService, WarehouseService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IStockService, StockService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IEmployeeWarehouseService, EmployeeWarehouseService>();
            services.AddScoped<IAnalyticsService, AnalyticsService>();
            services.AddScoped<IPriceHistoryService, PriceHistoryService>();
            services.AddScoped<ITransactionManager, TransactionManager>();

            // Cart (Singleton - одна корзина на всё приложение)
            services.AddSingleton<WarehouseManager.Wpf.Models.Cart>();

            // ViewModels
            services.AddTransient<AuthViewModel>();
            services.AddTransient<WarehousesViewModel>();
            services.AddTransient<CreateWarehouseViewModel>();
            services.AddTransient<UpdateWarehouseViewModel>();
            services.AddTransient<ProductsViewModel>();
            services.AddTransient<CreateProductViewModel>();
            services.AddTransient<ProductCatalogViewModel>();
            services.AddTransient<CartViewModel>();
            services.AddTransient<CheckoutViewModel>();
            services.AddTransient<StocksViewModel>();
            services.AddTransient<CreateStockViewModel>();
            services.AddTransient<UsersViewModel>();
            services.AddTransient<CreateUserViewModel>();
            services.AddTransient<UpdateUserViewModel>();
            services.AddTransient<EmployeesViewModel>();
            services.AddTransient<CreateEmployeeViewModel>();
            services.AddTransient<UpdateEmployeeViewModel>();
            services.AddTransient<EmployeeWarehousesViewModel>();
            services.AddTransient<CreateEmployeeWarehouseViewModel>();
            services.AddTransient<CategoriesViewModel>();
            services.AddTransient<CreateCategoryViewModel>();
            services.AddTransient<UpdateCategoryViewModel>();
            services.AddTransient<OrdersViewModel>();
            services.AddTransient<UpdateOrderStatusViewModel>();
            services.AddTransient<AssignEmployeeToOrderViewModel>();
            services.AddTransient<AnalyticsViewModel>();
            services.AddTransient<PriceHistoryViewModel>();
            services.AddTransient<MainWindowViewModel>();

            ServiceProvider = services.BuildServiceProvider();

            var viewModel = ServiceProvider.GetRequiredService<AuthViewModel>();
            var authWindow = new AuthWindow(viewModel);
            authWindow.Show();
        }
    }

}
