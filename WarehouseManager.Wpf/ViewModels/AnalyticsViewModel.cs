using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Services.Summary.Analytics;

namespace WarehouseManager.Wpf.ViewModels;

public partial class AnalyticsViewModel : ObservableObject
{
    private readonly IAnalyticsService _analyticsService;

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

    public ObservableCollection<CategoryRevenueSummary> TopCategoryRevenue { get; } = new();
    public ObservableCollection<WarehouseOrderStatsSummary> WarehouseOrderStats { get; } = new();
    public ObservableCollection<EmployeePerformanceSummary> EmployeePerformanceDetails { get; } = new();
    public ObservableCollection<CategoryPriceStatsSummary> CategoryPriceStats { get; } = new();
    public ObservableCollection<WarehouseStockDetailSummary> WarehouseStockDetails { get; } = new();

    public AnalyticsViewModel(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var now = DateTime.UtcNow;
            var startPeriod = now.AddMonths(-3);
            var currentYear = now.Year;

            var warehouseStock = await _analyticsService.GetWarehouseStockDistributionAsync();
            var orderStatus = await _analyticsService.GetOrderStatusDistributionAsync();
            var categoryProductCount = await _analyticsService.GetCategoryProductCountAsync();
            var monthlyRevenue = await _analyticsService.GetMonthlyRevenueTrendAsync(currentYear);
            var employeePerformance = await _analyticsService.GetEmployeePerformanceStatsAsync(startPeriod, now, 8);
            var topCategoryRevenue = await _analyticsService.GetTopCategoryRevenueAsync(startPeriod, now, 6);
            var warehouseOrderStats = await _analyticsService.GetWarehouseOrderStatsAsync(startPeriod, now);
            var categoryPriceStats = await _analyticsService.GetCategoryPriceStatsAsync();
            var warehouseStockDetails = await _analyticsService.GetWarehouseStockDetailsAsync();

            WarehouseStockPlot = CreatePieChart(
                "Стоимость запасов по складам",
                warehouseStock.Select(item => (item.WarehouseName, Convert.ToDouble(item.TotalValue))));

            OrderStatusPlot = CreateColumnChart(
                "Распределение заказов по статусам",
                orderStatus.Select(item => (item.StatusName, (double)item.OrderCount)));

            CategoryProductCountPlot = CreateLineChart(
                "Количество товаров по категориям",
                categoryProductCount.Select(item => (item.CategoryName, (double)item.ProductCount)));

            MonthlyRevenuePlot = CreateMonthlyRevenueChart(monthlyRevenue, currentYear);
            EmployeePerformancePlot = CreateEmployeePerformanceChart(employeePerformance);
            TopCategoryRevenuePlot = CreateCategoryRevenueChart(topCategoryRevenue);

            TopCategoryRevenue.Clear();
            foreach (var item in topCategoryRevenue.OrderByDescending(c => c.TotalRevenue))
            {
                TopCategoryRevenue.Add(item);
            }

            WarehouseOrderStats.Clear();
            foreach (var item in warehouseOrderStats.OrderByDescending(w => w.TotalRevenue))
            {
                WarehouseOrderStats.Add(item);
            }

            EmployeePerformanceDetails.Clear();
            foreach (var item in employeePerformance.OrderByDescending(e => e.OrdersHandled))
            {
                EmployeePerformanceDetails.Add(item);
            }

            CategoryPriceStats.Clear();
            foreach (var item in categoryPriceStats.OrderByDescending(c => c.AveragePrice))
            {
                CategoryPriceStats.Add(item);
            }

            WarehouseStockDetails.Clear();
            foreach (var item in warehouseStockDetails.OrderByDescending(w => w.TotalValue))
            {
                WarehouseStockDetails.Add(item);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Не удалось загрузить данные аналитики: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
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
