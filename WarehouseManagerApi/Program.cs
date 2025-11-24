using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Prometheus;
using System.Reflection;
using WarehouseManager.Application.Services;
using WarehouseManager.Core.Data;
using WarehouseManager.Services.Helpers;
using WarehouseManager.Services.Services;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManagerApi.Json;
using WarehouseManagerApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Warehouse Manager API",
        Version = "v1",
    Description = @"REST API для управления складской инфраструктурой:
- каталоги складов, продуктов и категорий;
- оформление и обработка заказов;
- управление сотрудниками и назначениями;
- аналитические отчёты и показатели.

Все методы защищены в доменной логике и возвращают подробные ответы об ошибках.",
        TermsOfService = new Uri("https://github.com"),
        Contact = new OpenApiContact
        {
            Name = "Warehouse Manager Team",
            Email = "support@warehousemanager.local"
        },
        License = new OpenApiLicense
        {
            Name = "Internal Use",
            Url = new Uri("https://github.com")
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
});

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddMetricServer(option => { option.Port = 9090; });
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IEmployeeWarehouseService, EmployeeWarehouseService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IPriceHistoryService, PriceHistoryService>();
builder.Services.AddScoped<ITransactionManager, TransactionManager>();
builder.Services.AddScoped<IMetricsService, MetricsService>();
builder.Services.AddScoped<CustomMetricsService>();

var app = builder.Build();

app.UseRouting();

app.UseHttpMetrics();

    app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Warehouse Manager API v1");
    options.DisplayRequestDuration();
});

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/swagger");
        return;
    }

    await next();
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapMetrics(); 
});

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var customMetricsService = scope.ServiceProvider.GetRequiredService<CustomMetricsService>();
    await customMetricsService.InitializeMetricsAsync();
}

var serviceProvider = app.Services;
var timer = new System.Timers.Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
timer.Elapsed += async (sender, e) =>
{
    using var updateScope = serviceProvider.CreateScope();
    var metricsService = updateScope.ServiceProvider.GetRequiredService<CustomMetricsService>();
    await metricsService.UpdateActiveUsersCountAsync();
    await metricsService.UpdateTotalProductsInWarehousesAsync();
};
timer.AutoReset = true;
timer.Start();

app.Run();

public partial class Program
{
}
