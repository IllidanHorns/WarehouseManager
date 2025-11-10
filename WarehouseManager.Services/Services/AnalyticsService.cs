using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WarehouseManager.Core.Data;
using WarehouseManager.Core.ViewDTOs;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Services.Summary.Analytics;
using WWarehouseManager.Core.ViewDTOs;

namespace WarehouseManager.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly AppDbContext _context;

    public AnalyticsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<WarehouseStockDistributionSummary>> GetWarehouseStockDistributionAsync()
    {
        var data = await _context.Set<WarehouseStockDto>()
            .AsNoTracking()
            .ToListAsync();

        return data.Select(item => new WarehouseStockDistributionSummary
        {
            WarehouseName = item.WarehouseName,
            TotalValue = item.TotalValue
        }).ToList();
    }

    public async Task<List<OrderStatusDistributionSummary>> GetOrderStatusDistributionAsync()
    {
        var data = await _context.Set<OrderStatusDistributionDto>()
            .AsNoTracking()
            .ToListAsync();

        return data.Select(item => new OrderStatusDistributionSummary
        {
            StatusName = item.StatusName,
            OrderCount = item.OrderCount
        }).ToList();
    }

    public async Task<List<CategoryProductCountSummary>> GetCategoryProductCountAsync()
    {
        var data = await _context.Set<CategoryProductCountDto>()
            .AsNoTracking()
            .ToListAsync();

        return data.Select(item => new CategoryProductCountSummary
        {
            CategoryName = item.CategoryName,
            ProductCount = item.ProductCount
        }).ToList();
    }

    public Task<List<MonthlyRevenueSummary>> GetMonthlyRevenueTrendAsync(int year)
    {
        var yearParameter = new SqlParameter("@Year", SqlDbType.Int) { Value = year };
        return ExecuteQueryAsync(
            "SELECT MonthNumber, MonthName, OrderCount, TotalRevenue FROM dbo.fn_GetMonthlyRevenue(@Year)",
            CommandType.Text,
            reader => new MonthlyRevenueSummary
            {
                MonthNumber = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                MonthName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                OrderCount = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                TotalRevenue = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3)
            },
            yearParameter);
    }

    public Task<List<EmployeePerformanceSummary>> GetEmployeePerformanceStatsAsync(DateTime startDate, DateTime endDate, int top = 5)
    {
        var parameters = new[]
        {
            new SqlParameter("@StartDate", SqlDbType.DateTime2) { Value = startDate },
            new SqlParameter("@EndDate", SqlDbType.DateTime2) { Value = endDate },
            new SqlParameter("@TopCount", SqlDbType.Int) { Value = top }
        };

        return ExecuteQueryAsync(
            "dbo.sp_GetEmployeePerformanceStats",
            CommandType.StoredProcedure,
            reader => new EmployeePerformanceSummary
            {
                EmployeeId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                EmployeeName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                OrdersHandled = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                TotalRevenue = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3),
                AverageProcessingHours = reader.IsDBNull(4) ? (decimal?)null : reader.GetDecimal(4)
            },
            parameters);
    }

    public Task<List<CategoryRevenueSummary>> GetTopCategoryRevenueAsync(DateTime startDate, DateTime endDate, int topCount = 5)
    {
        var parameters = new[]
        {
            new SqlParameter("@StartDate", SqlDbType.DateTime2) { Value = startDate },
            new SqlParameter("@EndDate", SqlDbType.DateTime2) { Value = endDate },
            new SqlParameter("@TopCount", SqlDbType.Int) { Value = topCount }
        };

        return ExecuteQueryAsync(
            "SELECT CategoryName, TotalRevenue, TotalUnits FROM dbo.fn_GetTopCategoryRevenue(@StartDate, @EndDate, @TopCount)",
            CommandType.Text,
            reader => new CategoryRevenueSummary
            {
                CategoryName = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                TotalRevenue = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1),
                TotalUnits = reader.IsDBNull(2) ? 0 : reader.GetInt32(2)
            },
            parameters);
    }

    public Task<List<WarehouseOrderStatsSummary>> GetWarehouseOrderStatsAsync(DateTime startDate, DateTime endDate)
    {
        var parameters = new[]
        {
            new SqlParameter("@StartDate", SqlDbType.DateTime2) { Value = startDate },
            new SqlParameter("@EndDate", SqlDbType.DateTime2) { Value = endDate }
        };

        return ExecuteQueryAsync(
            "dbo.sp_GetWarehouseOrderStats",
            CommandType.StoredProcedure,
            reader => new WarehouseOrderStatsSummary
            {
                WarehouseId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                WarehouseAddress = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                TotalOrders = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                TotalRevenue = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3),
                ActiveOrders = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                CancelledOrders = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                CompletedOrders = reader.IsDBNull(6) ? 0 : reader.GetInt32(6)
            },
            parameters);
    }

    public Task<List<CategoryPriceStatsSummary>> GetCategoryPriceStatsAsync()
    {
        return ExecuteQueryAsync(
            "SELECT * FROM dbo.fn_GetCategoryPriceStats()",
            CommandType.Text,
            reader => new CategoryPriceStatsSummary
            {
                CategoryId = reader.GetInt32(0),
                CategoryName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                ProductCount = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                AveragePrice = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3),
                MinPrice = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4),
                MaxPrice = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5)
            });
    }

    public Task<List<WarehouseStockDetailSummary>> GetWarehouseStockDetailsAsync()
    {
        return ExecuteQueryAsync(
            "SELECT * FROM dbo.fn_GetWarehouseStockDetails()",
            CommandType.Text,
            reader => new WarehouseStockDetailSummary
            {
                WarehouseId = reader.GetInt32(0),
                WarehouseAddress = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                Square = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                DistinctProducts = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                TotalQuantity = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                TotalValue = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5)
            });
    }

    private async Task<List<T>> ExecuteQueryAsync<T>(string commandText, CommandType commandType, Func<DbDataReader, T> projector, params SqlParameter[] parameters)
    {
        var connection = _context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;

        if (shouldClose)
        {
            await connection.OpenAsync();
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = commandType;

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
            }

            var result = new List<T>();
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(projector(reader));
            }

            return result;
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }
}

