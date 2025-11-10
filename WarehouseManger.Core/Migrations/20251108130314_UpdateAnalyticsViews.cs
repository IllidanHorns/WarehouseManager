using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagerApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAnalyticsViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
IF OBJECT_ID(N'dbo.WarehouseStockDistribution', N'V') IS NOT NULL
    DROP VIEW dbo.WarehouseStockDistribution;
""");

            migrationBuilder.Sql("""
CREATE VIEW dbo.WarehouseStockDistribution AS
SELECT
    w.Id AS WarehouseId,
    w.Address AS WarehouseName,
    SUM(CAST(r.Quantity AS decimal(18, 2)) * p.Price) AS TotalValue
FROM dbo.Remaining AS r
INNER JOIN dbo.Warehouses AS w ON w.Id = r.WarehouseId
INNER JOIN dbo.Products AS p ON p.Id = r.ProductId
WHERE r.IsArchived = 0
  AND w.IsArchived = 0
  AND p.IsArchived = 0
GROUP BY w.Id, w.Address;
""");

            migrationBuilder.Sql("""
IF OBJECT_ID(N'dbo.OrderStatusDistribution', N'V') IS NOT NULL
    DROP VIEW dbo.OrderStatusDistribution;
""");

            migrationBuilder.Sql("""
CREATE VIEW dbo.OrderStatusDistribution AS
SELECT
    os.Id AS StatusId,
    os.StatusName,
    COUNT(*) AS OrderCount
FROM dbo.Orders AS o
INNER JOIN dbo.OrderStatuses AS os ON os.Id = o.StatusId
WHERE o.IsArchived = 0
GROUP BY os.Id, os.StatusName;
""");

            migrationBuilder.Sql("""
IF OBJECT_ID(N'dbo.CategoryProductCount', N'V') IS NOT NULL
    DROP VIEW dbo.CategoryProductCount;
""");

            migrationBuilder.Sql("""
CREATE VIEW dbo.CategoryProductCount AS
SELECT
    c.Id AS CategoryId,
    c.Name AS CategoryName,
    COUNT(p.Id) AS ProductCount
FROM dbo.Categories AS c
LEFT JOIN dbo.Products AS p
    ON p.CategoryId = c.Id
    AND p.IsArchived = 0
WHERE c.IsArchived = 0
GROUP BY c.Id, c.Name;
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.WarehouseStockDistribution;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.OrderStatusDistribution;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.CategoryProductCount;");
        }
    }
}
