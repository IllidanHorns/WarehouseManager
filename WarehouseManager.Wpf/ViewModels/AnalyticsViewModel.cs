using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Microsoft.EntityFrameworkCore;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using WarehouseManager.Core.Data;
using WarehouseManager.Core.Models;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Services.Summary;
using WarehouseManager.Services.Summary.Analytics;
using WpfPngExporter = OxyPlot.Wpf.PngExporter;

namespace WarehouseManager.Wpf.ViewModels;

public partial class AnalyticsViewModel : ObservableObject
{
    private readonly IAnalyticsService _analyticsService;
    private readonly IWarehouseService _warehouseService;
    private readonly ICategoryService _categoryService;
    private readonly AppDbContext _context;
    private const string CsvDialogFilter = "CSV файлы (*.csv)|*.csv";
    private const string PdfDialogFilter = "PDF файлы (*.pdf)|*.pdf";

    [ObservableProperty]
    private PlotModel? _warehouseStockPlot;

    [ObservableProperty]
    private PlotModel? _orderStatusPlot;

    [ObservableProperty]
    private PlotModel? _categoryProductCountPlot;

    [ObservableProperty]
    private PlotModel? _monthlyRevenuePlot;

    [ObservableProperty]
    private PlotModel? _employeePerformancePlot;

    [ObservableProperty]
    private PlotModel? _topCategoryRevenuePlot;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private DateTime? _startDate;

    [ObservableProperty]
    private DateTime? _endDate;

    [ObservableProperty]
    private int _selectedYear;

    [ObservableProperty]
    private int? _selectedWarehouseId;

    [ObservableProperty]
    private int? _selectedCategoryId;

    [ObservableProperty]
    private int? _selectedOrderStatusId;

    [ObservableProperty]
    private int _topCount = 6;

    public ObservableCollection<int> AvailableYears { get; } = new();
    public ObservableCollection<WarehouseSummary> Warehouses { get; } = new();
    public ObservableCollection<CategorySummary> Categories { get; } = new();
    public ObservableCollection<OrderStatus> OrderStatuses { get; } = new();
    public ObservableCollection<int> AvailableTopCounts { get; } = new() { 3, 5, 6, 8, 10, 15, 20 };

    public ObservableCollection<CategoryRevenueSummary> TopCategoryRevenue { get; } = new();
    public ObservableCollection<WarehouseOrderStatsSummary> WarehouseOrderStats { get; } = new();
    public ObservableCollection<EmployeePerformanceSummary> EmployeePerformanceDetails { get; } = new();
    public ObservableCollection<CategoryPriceStatsSummary> CategoryPriceStats { get; } = new();
    public ObservableCollection<WarehouseStockDetailSummary> WarehouseStockDetails { get; } = new();

    // Буфер данных для экспорта
    private List<WarehouseStockDistributionSummary> _warehouseStockExportData = new();
    private List<OrderStatusDistributionSummary> _orderStatusExportData = new();
    private List<CategoryProductCountSummary> _categoryProductCountExportData = new();
    private List<MonthlyRevenueSummary> _monthlyRevenueExportData = new();
    private List<EmployeePerformanceSummary> _employeePerformanceExportData = new();
    private List<CategoryRevenueSummary> _topCategoryRevenueExportData = new();

    public AnalyticsViewModel(
        IAnalyticsService analyticsService,
        IWarehouseService warehouseService,
        ICategoryService categoryService,
        AppDbContext context)
    {
        _analyticsService = analyticsService;
        _warehouseService = warehouseService;
        _categoryService = categoryService;
        _context = context;
        
        // Инициализация фильтров значениями по умолчанию
        var now = DateTime.UtcNow;
        EndDate = now;
        StartDate = now.AddMonths(-3);
        SelectedYear = now.Year;
        
        // Заполняем список доступных годов (текущий и предыдущие 5 лет)
        for (int i = 0; i <= 5; i++)
        {
            AvailableYears.Add(now.Year - i);
        }
    }

    [RelayCommand]
    public async Task LoadFilterDataAsync()
    {
        try
        {
            // Загружаем склады
            var warehouseFilter = new WarehouseFilter
            {
                Page = 1,
                PageSize = 1000,
                IncludeArchived = false
            };
            var warehousesResult = await _warehouseService.GetPagedAsync(warehouseFilter);
            Warehouses.Clear();
            Warehouses.Add(new WarehouseSummary { Id = -1, Address = "Все склады" });
            foreach (var warehouse in warehousesResult.Items)
            {
                Warehouses.Add(warehouse);
            }

            // Загружаем категории
            var categoryFilter = new CategoryFilter
            {
                Page = 1,
                PageSize = 1000,
                IncludeArchived = false
            };
            var categoriesResult = await _categoryService.GetPagedAsync(categoryFilter);
            Categories.Clear();
            Categories.Add(new CategorySummary { Id = -1, Name = "Все категории" });
            foreach (var category in categoriesResult.Items)
            {
                Categories.Add(category);
            }

            // Загружаем статусы заказов
            var statuses = await _context.OrderStatuses
                .Where(s => !s.IsArchived)
                .OrderBy(s => s.StatusName)
                .ToListAsync();
            OrderStatuses.Clear();
            OrderStatuses.Add(new OrderStatus { Id = -1, StatusName = "Все статусы" });
            foreach (var status in statuses)
            {
                OrderStatuses.Add(status);
            }
        }
        catch (Exception ex)
        {
            var errorMsg = $"Ошибка при загрузке данных фильтров: {ex.Message}";
            if (ex.InnerException != null)
            {
                errorMsg += $"\nДетали: {ex.InnerException.Message}";
            }
            ErrorMessage = errorMsg;
            System.Diagnostics.Debug.WriteLine($"Ошибка в LoadFilterDataAsync: {errorMsg}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            // Убеждаемся, что фильтры загружены перед загрузкой данных
            if (!Warehouses.Any() || !Categories.Any() || !OrderStatuses.Any())
            {
                await LoadFilterDataAsync();
            }
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка при загрузке: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Ошибка в LoadAsync: {ex.Message}\n{ex.StackTrace}");
        }
    }

    [RelayCommand]
    public async Task ApplyFiltersAsync()
    {
        // Валидация дат (только если они указаны)
        if (StartDate.HasValue && EndDate.HasValue)
        {
            if (StartDate.Value > EndDate.Value)
            {
                ErrorMessage = "Дата начала не может быть больше даты окончания";
                return;
            }

            if (EndDate.Value > DateTime.UtcNow)
            {
                ErrorMessage = "Дата окончания не может быть больше текущей даты";
                return;
            }
        }

        await LoadDataAsync();
    }

    [RelayCommand]
    public async Task ResetFiltersAsync()
    {
        var now = DateTime.UtcNow;
        StartDate = now.AddMonths(-3);
        EndDate = now;
        SelectedYear = now.Year;
        SelectedWarehouseId = -1;
        SelectedCategoryId = -1;
        SelectedOrderStatusId = -1;
        TopCount = 6;
        await LoadDataAsync();
    }

    [RelayCommand]
    private void ExportCsv(string? target)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            return;
        }

        try
        {
            switch (target)
            {
                case "WarehouseStock":
                    ExportWarehouseStockCsv();
                    break;
                case "OrderStatus":
                    ExportOrderStatusCsv();
                    break;
                case "CategoryProductCount":
                    ExportCategoryProductCountCsv();
                    break;
                case "MonthlyRevenue":
                    ExportMonthlyRevenueCsv();
                    break;
                case "TopCategoryRevenue":
                    ExportTopCategoryRevenueCsv();
                    break;
                case "EmployeePerformance":
                    ExportEmployeePerformanceCsv();
                    break;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Не удалось экспортировать CSV: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ExportPdf(string? target)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            return;
        }

        try
        {
            switch (target)
            {
                case "WarehouseStock":
                    ExportPlotToPdf(WarehouseStockPlot, "warehouse_stock");
                    break;
                case "OrderStatus":
                    ExportPlotToPdf(OrderStatusPlot, "order_status");
                    break;
                case "CategoryProductCount":
                    ExportPlotToPdf(CategoryProductCountPlot, "category_product_count");
                    break;
                case "MonthlyRevenue":
                    ExportPlotToPdf(MonthlyRevenuePlot, "monthly_revenue");
                    break;
                case "TopCategoryRevenue":
                    ExportPlotToPdf(TopCategoryRevenuePlot, "top_category_revenue");
                    break;
                case "EmployeePerformance":
                    ExportPlotToPdf(EmployeePerformancePlot, "employee_performance");
                    break;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Не удалось экспортировать PDF: {ex.Message}";
        }
    }

    partial void OnSelectedWarehouseIdChanged(int? value)
    {
        // Если выбран "Все склады" (-1), сбрасываем фильтр
        if (value == -1)
        {
            SelectedWarehouseId = null;
        }
    }

    partial void OnSelectedCategoryIdChanged(int? value)
    {
        // Если выбрана "Все категории" (-1), сбрасываем фильтр
        if (value == -1)
        {
            SelectedCategoryId = null;
        }
    }

    partial void OnSelectedOrderStatusIdChanged(int? value)
    {
        // Если выбран "Все статусы" (-1), сбрасываем фильтр
        if (value == -1)
        {
            SelectedOrderStatusId = null;
        }
    }

    private async Task LoadDataAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            // Используем фильтры для загрузки данных
            // Если даты не указаны, используем значения по умолчанию
            var now = DateTime.UtcNow;
            var startPeriod = StartDate ?? now.AddMonths(-3);
            var endPeriod = EndDate ?? now;
            var currentYear = SelectedYear > 0 ? SelectedYear : now.Year;

            // Загружаем данные аналитики
            List<WarehouseStockDistributionSummary> warehouseStock;
            List<OrderStatusDistributionSummary> orderStatus;
            List<CategoryProductCountSummary> categoryProductCount;
            List<MonthlyRevenueSummary> monthlyRevenue;
            List<EmployeePerformanceSummary> employeePerformance;
            List<CategoryRevenueSummary> topCategoryRevenue;
            List<WarehouseOrderStatsSummary> warehouseOrderStats;
            List<CategoryPriceStatsSummary> categoryPriceStats;
            List<WarehouseStockDetailSummary> warehouseStockDetails;

            try
            {
                System.Diagnostics.Debug.WriteLine("Начало загрузки данных аналитики...");
                
                // Загружаем данные по одному, чтобы видеть, где именно ошибка
                warehouseStock = (await _analyticsService.GetWarehouseStockDistributionAsync()).ToList();
                System.Diagnostics.Debug.WriteLine($"warehouseStock загружен: {warehouseStock?.Count ?? 0} элементов");
                
                orderStatus = (await _analyticsService.GetOrderStatusDistributionAsync()).ToList();
                System.Diagnostics.Debug.WriteLine($"orderStatus загружен: {orderStatus?.Count ?? 0} элементов");
                
                categoryProductCount = (await _analyticsService.GetCategoryProductCountAsync()).ToList();
                System.Diagnostics.Debug.WriteLine($"categoryProductCount загружен: {categoryProductCount?.Count ?? 0} элементов");
                
                monthlyRevenue = (await _analyticsService.GetMonthlyRevenueTrendAsync(currentYear)).ToList();
                System.Diagnostics.Debug.WriteLine($"monthlyRevenue загружен: {monthlyRevenue?.Count ?? 0} элементов");
                
                employeePerformance = (await _analyticsService.GetEmployeePerformanceStatsAsync(startPeriod, endPeriod, TopCount)).ToList();
                System.Diagnostics.Debug.WriteLine($"employeePerformance загружен: {employeePerformance?.Count ?? 0} элементов");
                
                topCategoryRevenue = (await _analyticsService.GetTopCategoryRevenueAsync(startPeriod, endPeriod, TopCount)).ToList();
                System.Diagnostics.Debug.WriteLine($"topCategoryRevenue загружен: {topCategoryRevenue?.Count ?? 0} элементов");
                
                warehouseOrderStats = (await _analyticsService.GetWarehouseOrderStatsAsync(startPeriod, endPeriod)).ToList();
                System.Diagnostics.Debug.WriteLine($"warehouseOrderStats загружен: {warehouseOrderStats?.Count ?? 0} элементов");
                
                categoryPriceStats = (await _analyticsService.GetCategoryPriceStatsAsync()).ToList();
                System.Diagnostics.Debug.WriteLine($"categoryPriceStats загружен: {categoryPriceStats?.Count ?? 0} элементов");
                
                warehouseStockDetails = (await _analyticsService.GetWarehouseStockDetailsAsync()).ToList();
                System.Diagnostics.Debug.WriteLine($"warehouseStockDetails загружен: {warehouseStockDetails?.Count ?? 0} элементов");
                
                System.Diagnostics.Debug.WriteLine("Все данные успешно загружены");
            }
            catch (Exception loadEx)
            {
                var errorMsg = $"Ошибка при загрузке данных: {loadEx.Message}";
                if (loadEx.InnerException != null)
                {
                    errorMsg += $"\nДетали: {loadEx.InnerException.Message}";
                }
                ErrorMessage = errorMsg;
                System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке данных аналитики: {errorMsg}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {loadEx.StackTrace}");
                
                // Очищаем графики при ошибке
                WarehouseStockPlot = null;
                OrderStatusPlot = null;
                CategoryProductCountPlot = null;
                MonthlyRevenuePlot = null;
                EmployeePerformancePlot = null;
                TopCategoryRevenuePlot = null;
                return; // Прерываем выполнение, если не удалось загрузить данные
            }

            // Применяем фильтры к данным (игнорируем -1, что означает "Все")
            if (SelectedWarehouseId.HasValue && SelectedWarehouseId.Value > 0 && Warehouses.Any())
            {
                var warehouse = Warehouses.FirstOrDefault(w => w.Id == SelectedWarehouseId.Value);
                if (warehouse != null)
                {
                    warehouseStock = warehouseStock.Where(w => w.WarehouseName == warehouse.Address).ToList();
                    warehouseOrderStats = warehouseOrderStats.Where(w => w.WarehouseAddress == warehouse.Address).ToList();
                    warehouseStockDetails = warehouseStockDetails.Where(w => w.WarehouseAddress == warehouse.Address).ToList();
                }
            }

            if (SelectedCategoryId.HasValue && SelectedCategoryId.Value > 0 && Categories.Any())
            {
                var category = Categories.FirstOrDefault(c => c.Id == SelectedCategoryId.Value);
                if (category != null)
                {
                    categoryProductCount = categoryProductCount.Where(c => c.CategoryName == category.Name).ToList();
                    topCategoryRevenue = topCategoryRevenue.Where(c => c.CategoryName == category.Name).ToList();
                    categoryPriceStats = categoryPriceStats.Where(c => c.CategoryName == category.Name).ToList();
                }
            }

            if (SelectedOrderStatusId.HasValue && SelectedOrderStatusId.Value > 0 && OrderStatuses.Any())
            {
                var status = OrderStatuses.FirstOrDefault(s => s.Id == SelectedOrderStatusId.Value);
                if (status != null)
                {
                    orderStatus = orderStatus.Where(s => s.StatusName == status.StatusName).ToList();
                }
            }

            // Подготавливаем данные для экспорта
            _warehouseStockExportData = warehouseStock?.ToList() ?? new List<WarehouseStockDistributionSummary>();
            _orderStatusExportData = orderStatus?.ToList() ?? new List<OrderStatusDistributionSummary>();
            _categoryProductCountExportData = categoryProductCount?.ToList() ?? new List<CategoryProductCountSummary>();
            _monthlyRevenueExportData = monthlyRevenue?.ToList() ?? new List<MonthlyRevenueSummary>();
            _employeePerformanceExportData = employeePerformance?.ToList() ?? new List<EmployeePerformanceSummary>();
            _topCategoryRevenueExportData = topCategoryRevenue?.ToList() ?? new List<CategoryRevenueSummary>();

            // Создаем графики только если есть данные, с обработкой ошибок
            try
            {
                if (warehouseStock != null && warehouseStock.Any())
                {
                    WarehouseStockPlot = CreatePieChart(
                        "Стоимость запасов по складам",
                        warehouseStock.Select(item => (item.WarehouseName, Convert.ToDouble(item.TotalValue))));
                }
                else
                {
                    WarehouseStockPlot = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при создании графика WarehouseStockPlot: {ex.Message}");
                WarehouseStockPlot = null;
            }

            try
            {
                if (orderStatus != null && orderStatus.Any())
                {
                    OrderStatusPlot = CreateColumnChart(
                        "Распределение заказов по статусам",
                        orderStatus.Select(item => (item.StatusName, (double)item.OrderCount)));
                }
                else
                {
                    OrderStatusPlot = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при создании графика OrderStatusPlot: {ex.Message}");
                OrderStatusPlot = null;
            }

            try
            {
                if (categoryProductCount != null && categoryProductCount.Any())
                {
                    CategoryProductCountPlot = CreateLineChart(
                        "Количество товаров по категориям",
                        categoryProductCount.Select(item => (item.CategoryName, (double)item.ProductCount)));
                }
                else
                {
                    CategoryProductCountPlot = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при создании графика CategoryProductCountPlot: {ex.Message}");
                CategoryProductCountPlot = null;
            }

            // Создаем графики с проверкой на null
            try
            {
                if (monthlyRevenue != null)
                {
                    MonthlyRevenuePlot = CreateMonthlyRevenueChart(monthlyRevenue, currentYear);
                }
                else
                {
                    MonthlyRevenuePlot = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при создании графика MonthlyRevenuePlot: {ex.Message}");
                MonthlyRevenuePlot = null;
            }

            try
            {
                if (employeePerformance != null && employeePerformance.Any())
                {
                    EmployeePerformancePlot = CreateEmployeePerformanceChart(employeePerformance);
                }
                else
                {
                    EmployeePerformancePlot = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при создании графика EmployeePerformancePlot: {ex.Message}");
                EmployeePerformancePlot = null;
            }

            try
            {
                if (topCategoryRevenue != null && topCategoryRevenue.Any())
                {
                    TopCategoryRevenuePlot = CreateCategoryRevenueChart(topCategoryRevenue);
                }
                else
                {
                    TopCategoryRevenuePlot = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при создании графика TopCategoryRevenuePlot: {ex.Message}");
                TopCategoryRevenuePlot = null;
            }

            // Заполняем коллекции для таблиц
            TopCategoryRevenue.Clear();
            if (topCategoryRevenue != null)
            {
                foreach (var item in topCategoryRevenue.OrderByDescending(c => c.TotalRevenue))
                {
                    TopCategoryRevenue.Add(item);
                }
            }

            WarehouseOrderStats.Clear();
            if (warehouseOrderStats != null)
            {
                foreach (var item in warehouseOrderStats.OrderByDescending(w => w.TotalRevenue))
                {
                    WarehouseOrderStats.Add(item);
                }
            }

            EmployeePerformanceDetails.Clear();
            if (employeePerformance != null)
            {
                foreach (var item in employeePerformance.OrderByDescending(e => e.OrdersHandled))
                {
                    EmployeePerformanceDetails.Add(item);
                }
            }

            CategoryPriceStats.Clear();
            if (categoryPriceStats != null)
            {
                foreach (var item in categoryPriceStats.OrderByDescending(c => c.AveragePrice))
                {
                    CategoryPriceStats.Add(item);
                }
            }

            WarehouseStockDetails.Clear();
            if (warehouseStockDetails != null)
            {
                foreach (var item in warehouseStockDetails.OrderByDescending(w => w.TotalValue))
                {
                    WarehouseStockDetails.Add(item);
                }
            }

            // Принудительно обновляем UI после создания графиков
            OnPropertyChanged(nameof(WarehouseStockPlot));
            OnPropertyChanged(nameof(OrderStatusPlot));
            OnPropertyChanged(nameof(CategoryProductCountPlot));
            OnPropertyChanged(nameof(MonthlyRevenuePlot));
            OnPropertyChanged(nameof(EmployeePerformancePlot));
            OnPropertyChanged(nameof(TopCategoryRevenuePlot));
            
            System.Diagnostics.Debug.WriteLine("Графики успешно созданы и обновлены");
        }
        catch (Exception ex)
        {
            var errorDetails = $"Не удалось загрузить данные аналитики: {ex.Message}";
            if (ex.InnerException != null)
            {
                errorDetails += $"\nДетали: {ex.InnerException.Message}";
            }
            if (ex.StackTrace != null)
            {
                errorDetails += $"\nСтек вызовов: {ex.StackTrace}";
            }
            ErrorMessage = errorDetails;
            
            // Очищаем графики при ошибке, чтобы не показывать старые данные
            WarehouseStockPlot = null;
            OrderStatusPlot = null;
            CategoryProductCountPlot = null;
            MonthlyRevenuePlot = null;
            EmployeePerformancePlot = null;
            TopCategoryRevenuePlot = null;
            
            TopCategoryRevenue.Clear();
            WarehouseOrderStats.Clear();
            EmployeePerformanceDetails.Clear();
            CategoryPriceStats.Clear();
            WarehouseStockDetails.Clear();
            
            // Логируем ошибку для отладки
            System.Diagnostics.Debug.WriteLine($"Ошибка в LoadDataAsync: {errorDetails}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ExportWarehouseStockCsv()
    {
        if (!EnsureDataAvailable(_warehouseStockExportData, "запасы по складам"))
            return;

        var builder = new StringBuilder();
        builder.AppendLine("Склад;Стоимость");
        foreach (var item in _warehouseStockExportData)
        {
            builder.Append(EscapeCsv(item.WarehouseName));
            builder.Append(';');
            builder.AppendLine(item.TotalValue.ToString(CultureInfo.InvariantCulture));
        }

        SaveCsvContent(builder, "warehouse_stock");
    }

    private void ExportOrderStatusCsv()
    {
        if (!EnsureDataAvailable(_orderStatusExportData, "статусы заказов"))
            return;

        var builder = new StringBuilder();
        builder.AppendLine("Статус;Количество заказов");
        foreach (var item in _orderStatusExportData)
        {
            builder.Append(EscapeCsv(item.StatusName));
            builder.Append(';');
            builder.AppendLine(item.OrderCount.ToString(CultureInfo.InvariantCulture));
        }

        SaveCsvContent(builder, "order_status_distribution");
    }

    private void ExportCategoryProductCountCsv()
    {
        if (!EnsureDataAvailable(_categoryProductCountExportData, "товары по категориям"))
            return;

        var builder = new StringBuilder();
        builder.AppendLine("Категория;Количество товаров");
        foreach (var item in _categoryProductCountExportData)
        {
            builder.Append(EscapeCsv(item.CategoryName));
            builder.Append(';');
            builder.AppendLine(item.ProductCount.ToString(CultureInfo.InvariantCulture));
        }

        SaveCsvContent(builder, "category_product_count");
    }

    private void ExportMonthlyRevenueCsv()
    {
        if (!EnsureDataAvailable(_monthlyRevenueExportData, "помесячная выручка"))
            return;

        var builder = new StringBuilder();
        builder.AppendLine("Месяц;Количество заказов;Выручка");
        foreach (var item in _monthlyRevenueExportData.OrderBy(m => m.MonthNumber))
        {
            builder.Append(EscapeCsv(item.MonthName));
            builder.Append(';');
            builder.Append(item.OrderCount.ToString(CultureInfo.InvariantCulture));
            builder.Append(';');
            builder.AppendLine(item.TotalRevenue.ToString(CultureInfo.InvariantCulture));
        }

        SaveCsvContent(builder, "monthly_revenue");
    }

    private void ExportTopCategoryRevenueCsv()
    {
        if (!EnsureDataAvailable(_topCategoryRevenueExportData, "выручка по категориям"))
            return;

        var builder = new StringBuilder();
        builder.AppendLine("Категория;Выручка;Продано единиц");
        foreach (var item in _topCategoryRevenueExportData)
        {
            builder.Append(EscapeCsv(item.CategoryName));
            builder.Append(';');
            builder.Append(item.TotalRevenue.ToString(CultureInfo.InvariantCulture));
            builder.Append(';');
            builder.AppendLine(item.TotalUnits.ToString(CultureInfo.InvariantCulture));
        }

        SaveCsvContent(builder, "top_category_revenue");
    }

    private void ExportEmployeePerformanceCsv()
    {
        if (!EnsureDataAvailable(_employeePerformanceExportData, "эффективность сотрудников"))
            return;

        var builder = new StringBuilder();
        builder.AppendLine("Сотрудник;Обработано заказов;Выручка;Среднее время (ч)");
        foreach (var item in _employeePerformanceExportData)
        {
            builder.Append(EscapeCsv(item.EmployeeName));
            builder.Append(';');
            builder.Append(item.OrdersHandled.ToString(CultureInfo.InvariantCulture));
            builder.Append(';');
            builder.Append(item.TotalRevenue.ToString(CultureInfo.InvariantCulture));
            builder.Append(';');
            builder.AppendLine(item.AverageProcessingHours?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
        }

        SaveCsvContent(builder, "employee_performance");
    }

    private void SaveCsvContent(StringBuilder content, string defaultName)
    {
        if (!TryPromptFile(CsvDialogFilter, defaultName, ".csv", out var filePath))
            return;

        File.WriteAllText(filePath, content.ToString(), Encoding.UTF8);
    }

    private void ExportPlotToPdf(PlotModel? model, string defaultName)
    {
        if (model == null)
        {
            ErrorMessage = "Нет данных для экспорта графика.";
            return;
        }

        if (!TryPromptFile(PdfDialogFilter, defaultName, ".pdf", out var filePath))
            return;

        const int width = 900;
        const int height = 500;

        using var pngStream = new MemoryStream();
        var pngExporter = new WpfPngExporter
        {
            Width = width,
            Height = height,
            Background = OxyColors.White
        };
        pngExporter.Export(model, pngStream);
        pngStream.Position = 0;

        using var document = new PdfDocument();
        var page = document.AddPage();
        page.Width = width;
        page.Height = height;

        using (var gfx = XGraphics.FromPdfPage(page))
        {
            var imageData = pngStream.ToArray();
            using var image = XImage.FromStream(() => new MemoryStream(imageData));
            gfx.DrawImage(image, 0, 0, page.Width, page.Height);
        }

        document.Save(filePath);
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "\"\"";
        }

        var sanitized = value.Replace("\"", "\"\"");
        return $"\"{sanitized}\"";
    }

    private bool EnsureDataAvailable<T>(IEnumerable<T>? data, string chartName)
    {
        if (data == null || !data.Any())
        {
            ErrorMessage = $"Нет данных для экспорта ({chartName}).";
            return false;
        }

        return true;
    }

    private bool TryPromptFile(string filter, string defaultName, string defaultExtension, out string filePath)
    {
        var dialog = new SaveFileDialog
        {
            Filter = filter,
            DefaultExt = defaultExtension,
            AddExtension = true,
            FileName = $"{defaultName}_{DateTime.Now:yyyyMMdd_HHmm}"
        };

        var result = dialog.ShowDialog();
        if (result == true)
        {
            filePath = dialog.FileName;
            return true;
        }

        filePath = string.Empty;
        return false;
    }

    private static PlotModel? CreatePieChart(string title, IEnumerable<(string Label, double Value)> data)
    {
        var chartData = data.Where(item => item.Value > 0).ToList();
        if (!chartData.Any())
            return null;

        var model = CreateBaseModel(title, includeLegend: true);

        var series = new PieSeries
        {
            StrokeThickness = 1,
            Stroke = OxyColors.White,
            AngleSpan = 360,
            StartAngle = 0,
            TickHorizontalLength = 10,
            TickRadialLength = 6,
            InsideLabelFormat = string.Empty,
            OutsideLabelFormat = "{1}: {0:N0}",
            TextColor = OxyColors.Black,
            LegendFormat = "{1}",
            FontSize = 14,
            AreInsideLabelsAngled = false
        };

        foreach (var (label, value) in chartData)
        {
            var sliceLabel = string.IsNullOrWhiteSpace(label) ? "—" : label;
            var sliceValue = value < 0 ? 0 : value;

            series.Slices.Add(new PieSlice(sliceLabel, sliceValue));
        }

        model.Series.Add(series);
        model.InvalidatePlot(true);
        return model;
    }

    private static PlotModel? CreateColumnChart(string title, IEnumerable<(string Label, double Value)> data)
    {
        var chartData = data.Where(item => item.Value > 0).ToList();
        if (!chartData.Any())
            return null;

        var model = CreateBaseModel(title);

        var categoryAxis = new CategoryAxis
        {
            Position = AxisPosition.Bottom,
            GapWidth = 0.4,
            IsPanEnabled = false,
            IsZoomEnabled = false,
            Angle = -15,
            AxislineThickness = 0
        };

        var valueAxis = new LinearAxis
        {
            Position = AxisPosition.Left,
            MinimumPadding = 0,
            AbsoluteMinimum = 0,
            MajorGridlineStyle = LineStyle.Solid,
            MinorGridlineStyle = LineStyle.None,
            AxislineThickness = 0
        };

        var series = new ColumnSeries
        {
            LabelPlacement = LabelPlacement.Outside,
            LabelFormatString = "{0:N0}",
            StrokeThickness = 1,
            StrokeColor = OxyColors.White,
            FillColor = OxyColor.FromRgb(59, 130, 246),
            TextColor = OxyColors.Black,
            FontSize = 14
        };

        foreach (var (label, value) in chartData)
        {
            categoryAxis.Labels.Add(string.IsNullOrWhiteSpace(label) ? "—" : label);
            series.Items.Add(new ColumnItem(value));
        }

        model.Axes.Add(categoryAxis);
        model.Axes.Add(valueAxis);
        model.Series.Add(series);
        model.InvalidatePlot(true);
        return model;
    }

    private static PlotModel? CreateLineChart(string title, IEnumerable<(string Label, double Value)> data)
    {
        var chartData = data.Where(item => item.Value > 0).ToList();
        if (!chartData.Any())
            return null;

        var model = CreateBaseModel(title);

        var categoryAxis = new CategoryAxis
        {
            Position = AxisPosition.Bottom,
            IsPanEnabled = false,
            IsZoomEnabled = false,
            Angle = -15,
            AxislineThickness = 0
        };

        var valueAxis = new LinearAxis
        {
            Position = AxisPosition.Left,
            MinimumPadding = 0.1,
            AbsoluteMinimum = 0,
            MajorGridlineStyle = LineStyle.Solid,
            MinorGridlineStyle = LineStyle.None,
            AxislineThickness = 0
        };

        var series = new LineSeries
        {
            StrokeThickness = 2,
            Color = OxyColor.FromRgb(34, 197, 94),
            MarkerType = MarkerType.Circle,
            MarkerSize = 6,
            MarkerStroke = OxyColors.White,
            MarkerFill = OxyColor.FromRgb(34, 197, 94),
            TrackerFormatString = "{1}: {2:N0}"
        };

        for (var index = 0; index < chartData.Count; index++)
        {
            var (label, value) = chartData[index];
            categoryAxis.Labels.Add(string.IsNullOrWhiteSpace(label) ? "—" : label);
            series.Points.Add(new DataPoint(index, value));
        }

        model.Axes.Add(categoryAxis);
        model.Axes.Add(valueAxis);
        model.Series.Add(series);
        model.InvalidatePlot(true);
        return model;
    }

    private static PlotModel? CreateMonthlyRevenueChart(IEnumerable<MonthlyRevenueSummary> data, int year)
    {
        var list = data?.OrderBy(item => item.MonthNumber).ToList() ?? new List<MonthlyRevenueSummary>();
        if (!list.Any())
            return null;

        var model = CreateBaseModel($"Выручка по месяцам ({year} год)");

        var categoryAxis = new CategoryAxis
        {
            Position = AxisPosition.Bottom,
            Angle = -25,
            IsPanEnabled = false,
            IsZoomEnabled = false
        };

        var ordersAxis = new LinearAxis
        {
            Position = AxisPosition.Left,
            Title = "Количество заказов",
            Key = "OrdersAxis",
            AbsoluteMinimum = 0,
            MinimumPadding = 0,
            MajorGridlineStyle = LineStyle.Solid,
            MinorGridlineStyle = LineStyle.None
        };

        var revenueAxis = new LinearAxis
        {
            Position = AxisPosition.Right,
            Title = "Выручка, ₽",
            Key = "RevenueAxis",
            AbsoluteMinimum = 0,
            MinimumPadding = 0,
            MajorGridlineStyle = LineStyle.None,
            MinorGridlineStyle = LineStyle.None
        };

        var ordersSeries = new ColumnSeries
        {
            Title = "Заказы",
            YAxisKey = "OrdersAxis",
            LabelPlacement = LabelPlacement.Outside,
            LabelFormatString = "{0}",
            FillColor = OxyColor.FromRgb(59, 130, 246)
        };

        var revenueSeries = new LineSeries
        {
            Title = "Выручка",
            YAxisKey = "RevenueAxis",
            StrokeThickness = 2,
            Color = OxyColor.FromRgb(34, 197, 94),
            MarkerType = MarkerType.Circle,
            MarkerFill = OxyColor.FromRgb(34, 197, 94),
            MarkerStroke = OxyColors.White,
            TrackerFormatString = "{1}: {2:N0} ₽"
        };

        for (var index = 0; index < list.Count; index++)
        {
            var item = list[index];
            categoryAxis.Labels.Add(string.IsNullOrWhiteSpace(item.MonthName) ? item.MonthNumber.ToString() : item.MonthName);
            ordersSeries.Items.Add(new ColumnItem(item.OrderCount));
            revenueSeries.Points.Add(new DataPoint(index, Convert.ToDouble(item.TotalRevenue)));
        }

        model.Axes.Add(categoryAxis);
        model.Axes.Add(ordersAxis);
        model.Axes.Add(revenueAxis);
        model.Series.Add(ordersSeries);
        model.Series.Add(revenueSeries);
        model.IsLegendVisible = true;
        model.LegendPosition = LegendPosition.TopRight;
        model.LegendOrientation = LegendOrientation.Horizontal;
        model.InvalidatePlot(true);
        return model;
    }

    private static PlotModel? CreateCategoryRevenueChart(IEnumerable<CategoryRevenueSummary> data)
    {
        var list = data?.Where(item => item.TotalRevenue > 0).OrderByDescending(item => item.TotalRevenue).ToList() ?? new List<CategoryRevenueSummary>();
        if (!list.Any())
            return null;

        var model = CreateBaseModel("Топ категорий по выручке");

        var categoryAxis = new CategoryAxis
        {
            Position = AxisPosition.Left,
            IsPanEnabled = false,
            IsZoomEnabled = false
        };

        var valueAxis = new LinearAxis
        {
            Position = AxisPosition.Bottom,
            MinimumPadding = 0,
            AbsoluteMinimum = 0,
            MajorGridlineStyle = LineStyle.Solid,
            MinorGridlineStyle = LineStyle.None
        };

        var series = new BarSeries
        {
            LabelFormatString = "{0:N0} ₽",
            FontSize = 14,
            TextColor = OxyColors.White,
            FillColor = OxyColor.FromRgb(79, 70, 229)
        };

        foreach (var item in list)
        {
            categoryAxis.Labels.Add(string.IsNullOrWhiteSpace(item.CategoryName) ? "—" : item.CategoryName);
            series.Items.Add(new BarItem { Value = Convert.ToDouble(item.TotalRevenue) });
        }

        model.Axes.Add(categoryAxis);
        model.Axes.Add(valueAxis);
        model.Series.Add(series);
        model.InvalidatePlot(true);
        return model;
    }

    private static PlotModel? CreateEmployeePerformanceChart(IEnumerable<EmployeePerformanceSummary> data)
    {
        var list = data?.Where(item => item.OrdersHandled > 0).ToList() ?? new List<EmployeePerformanceSummary>();
        if (!list.Any())
            return null;

        var model = CreateBaseModel("Топ сотрудников по количеству заказов");

        var categoryAxis = new CategoryAxis
        {
            Position = AxisPosition.Bottom,
            Angle = -15,
            IsPanEnabled = false,
            IsZoomEnabled = false
        };

        var valueAxis = new LinearAxis
        {
            Position = AxisPosition.Left,
            MinimumPadding = 0,
            AbsoluteMinimum = 0,
            MajorGridlineStyle = LineStyle.Solid,
            MinorGridlineStyle = LineStyle.None
        };

        var series = new ColumnSeries
        {
            LabelPlacement = LabelPlacement.Inside,
            LabelFormatString = "{0}",
            FillColor = OxyColor.FromRgb(244, 114, 182),
            TextColor = OxyColors.White
        };

        foreach (var item in list.OrderByDescending(i => i.OrdersHandled))
        {
            categoryAxis.Labels.Add(string.IsNullOrWhiteSpace(item.EmployeeName) ? "—" : item.EmployeeName);
            series.Items.Add(new ColumnItem(item.OrdersHandled));
        }

        model.Axes.Add(categoryAxis);
        model.Axes.Add(valueAxis);
        model.Series.Add(series);
        model.InvalidatePlot(true);
        return model;
    }

    private static PlotModel CreateBaseModel(string title, bool includeLegend = false)
    {
        var model = new PlotModel
        {
            Title = title,
            Background = OxyColors.Transparent,
            PlotAreaBackground = OxyColors.Transparent,
            TitleFontSize = 20,
            PlotMargins = new OxyThickness(double.NaN)
        };

        if (includeLegend)
        {
            model.IsLegendVisible = true;
            model.LegendPlacement = LegendPlacement.Outside;
            model.LegendPosition = LegendPosition.RightTop;
            model.LegendOrientation = LegendOrientation.Vertical;
            model.LegendBackground = OxyColors.Transparent;
            model.LegendBorderThickness = 0;
            model.LegendTextColor = OxyColors.Black;
            model.LegendFontSize = 13;
        }

        return model;
    }
}
