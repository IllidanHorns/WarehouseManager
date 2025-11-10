using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WarehouseManager.Core.Data;

#nullable disable

namespace WarehouseManagerApi.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20251109134500_AddAnalyticsProceduresAndFunctions")]
    public partial class AddAnalyticsProceduresAndFunctions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
IF OBJECT_ID(N'dbo.fn_GetMonthlyRevenue', N'IF') IS NOT NULL
    DROP FUNCTION dbo.fn_GetMonthlyRevenue;
""");

            migrationBuilder.Sql("""
CREATE FUNCTION dbo.fn_GetMonthlyRevenue (@Year INT)
RETURNS TABLE
AS
RETURN
(
    SELECT
        MONTH(o.CreationDatetime) AS MonthNumber,
        FORMAT(DATEFROMPARTS(@Year, MONTH(o.CreationDatetime), 1), N'MMMM', 'ru-RU') AS MonthName,
        COUNT(*) AS OrderCount,
        SUM(o.TotalPrice) AS TotalRevenue
    FROM dbo.Orders AS o
    WHERE YEAR(o.CreationDatetime) = @Year
      AND o.IsArchived = 0
    GROUP BY MONTH(o.CreationDatetime)
);
""");

            migrationBuilder.Sql("""
IF OBJECT_ID(N'dbo.fn_GetTopCategoryRevenue', N'IF') IS NOT NULL
    DROP FUNCTION dbo.fn_GetTopCategoryRevenue;
""");

            migrationBuilder.Sql("""
CREATE FUNCTION dbo.fn_GetTopCategoryRevenue (@StartDate DATETIME2, @EndDate DATETIME2, @TopCount INT)
RETURNS TABLE
AS
RETURN
(
    SELECT TOP (@TopCount)
        c.Name AS CategoryName,
        SUM(op.Quantity * op.OrderPrice) AS TotalRevenue,
        SUM(op.Quantity) AS TotalUnits
    FROM dbo.OrdersProducts AS op
    INNER JOIN dbo.Orders AS o ON o.Id = op.OrderId
    INNER JOIN dbo.Products AS p ON p.Id = op.ProductId
    INNER JOIN dbo.Categories AS c ON c.Id = p.CategoryId
    WHERE o.CreationDatetime BETWEEN @StartDate AND @EndDate
      AND o.IsArchived = 0
      AND op.IsArchived = 0
      AND c.IsArchived = 0
    GROUP BY c.Name
    ORDER BY TotalRevenue DESC, TotalUnits DESC
);
""");

            migrationBuilder.Sql("""
IF OBJECT_ID(N'dbo.sp_GetWarehouseOrderStats', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetWarehouseOrderStats;
""");

            migrationBuilder.Sql("""
CREATE PROCEDURE dbo.sp_GetWarehouseOrderStats
    @StartDate DATETIME2,
    @EndDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        w.Id AS WarehouseId,
        w.Address AS WarehouseAddress,
        COUNT(*) AS TotalOrders,
        SUM(o.TotalPrice) AS TotalRevenue,
        SUM(CASE WHEN LOWER(os.StatusName) LIKE N'%актив%' THEN 1 ELSE 0 END) AS ActiveOrders,
        SUM(CASE WHEN LOWER(os.StatusName) LIKE N'%отмен%' THEN 1 ELSE 0 END) AS CancelledOrders,
        SUM(CASE WHEN LOWER(os.StatusName) LIKE N'%выполн%' OR LOWER(os.StatusName) LIKE N'%заверш%' THEN 1 ELSE 0 END) AS CompletedOrders
    FROM dbo.Orders AS o
    INNER JOIN dbo.Warehouses AS w ON w.Id = o.WarehouseId
    INNER JOIN dbo.OrderStatuses AS os ON os.Id = o.StatusId
    WHERE o.CreationDatetime BETWEEN @StartDate AND @EndDate
      AND o.IsArchived = 0
      AND w.IsArchived = 0
    GROUP BY w.Id, w.Address
    ORDER BY TotalRevenue DESC;
END;
""");

            migrationBuilder.Sql("""
IF OBJECT_ID(N'dbo.sp_GetEmployeePerformanceStats', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetEmployeePerformanceStats;
""");

            migrationBuilder.Sql("""
CREATE PROCEDURE dbo.sp_GetEmployeePerformanceStats
    @StartDate DATETIME2,
    @EndDate DATETIME2,
    @TopCount INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@TopCount)
        e.Id AS EmployeeId,
        LTRIM(RTRIM(
            COALESCE(u.FirstName, N'') +
            CASE WHEN u.MiddleName IS NULL OR u.MiddleName = N'' THEN N'' ELSE N' ' + u.MiddleName END +
            CASE WHEN u.Patronymic IS NULL OR u.Patronymic = N'' THEN N'' ELSE N' ' + u.Patronymic END
        )) AS EmployeeName,
        COUNT(*) AS AssignedOrders,
        SUM(o.TotalPrice) AS TotalRevenue,
        AVG(CAST(DATEDIFF(MINUTE, o.CreationDatetime, o.UpdateDatetime) AS DECIMAL(18, 2)) / 60.0) AS AvgProcessingHours
    FROM dbo.Orders AS o
    INNER JOIN dbo.Employees AS e ON e.Id = o.EmployeeId
    INNER JOIN dbo.Users AS u ON u.Id = e.UserId
    WHERE o.EmployeeId IS NOT NULL
      AND o.CreationDatetime BETWEEN @StartDate AND @EndDate
      AND o.IsArchived = 0
      AND e.IsArchived = 0
      AND u.IsArchived = 0
    GROUP BY e.Id, u.FirstName, u.MiddleName, u.Patronymic
    ORDER BY AssignedOrders DESC, TotalRevenue DESC;
END;
""");

            migrationBuilder.Sql("""
IF OBJECT_ID(N'dbo.fn_GetCategoryPriceStats', N'IF') IS NOT NULL
    DROP FUNCTION dbo.fn_GetCategoryPriceStats;
""");

            migrationBuilder.Sql("""
CREATE FUNCTION dbo.fn_GetCategoryPriceStats()
RETURNS TABLE
AS
RETURN
(
    SELECT
        c.Id AS CategoryId,
        c.Name AS CategoryName,
        COUNT(p.Id) AS ProductCount,
        COALESCE(AVG(CAST(p.Price AS decimal(18,2))), 0) AS AveragePrice,
        COALESCE(MIN(p.Price), 0) AS MinPrice,
        COALESCE(MAX(p.Price), 0) AS MaxPrice
    FROM dbo.Categories AS c
    LEFT JOIN dbo.Products AS p
        ON p.CategoryId = c.Id
        AND p.IsArchived = 0
    WHERE c.IsArchived = 0
    GROUP BY c.Id, c.Name
);
""");

            migrationBuilder.Sql("""
IF OBJECT_ID(N'dbo.fn_GetWarehouseStockDetails', N'IF') IS NOT NULL
    DROP FUNCTION dbo.fn_GetWarehouseStockDetails;
""");

            migrationBuilder.Sql("""
CREATE FUNCTION dbo.fn_GetWarehouseStockDetails()
RETURNS TABLE
AS
RETURN
(
    SELECT
        w.Id AS WarehouseId,
        w.Address AS WarehouseAddress,
        w.Square,
        COUNT(DISTINCT r.ProductId) AS DistinctProducts,
        COALESCE(SUM(r.Quantity), 0) AS TotalQuantity,
        COALESCE(SUM(CAST(r.Quantity AS decimal(18,2)) * p.Price), 0) AS TotalValue
    FROM dbo.Warehouses AS w
    LEFT JOIN dbo.Remaining AS r
        ON r.WarehouseId = w.Id
        AND r.IsArchived = 0
    LEFT JOIN dbo.Products AS p
        ON p.Id = r.ProductId
        AND p.IsArchived = 0
    WHERE w.IsArchived = 0
    GROUP BY w.Id, w.Address, w.Square
);
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS dbo.fn_GetWarehouseStockDetails;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS dbo.fn_GetCategoryPriceStats;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.sp_GetEmployeePerformanceStats;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.sp_GetWarehouseOrderStats;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS dbo.fn_GetTopCategoryRevenue;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS dbo.fn_GetMonthlyRevenue;");
        }
    }
}
