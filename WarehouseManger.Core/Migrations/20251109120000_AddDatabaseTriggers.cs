using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WarehouseManager.Core.Data;

#nullable disable

namespace WarehouseManagerApi.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20251109120000_AddDatabaseTriggers")]
    public partial class AddDatabaseTriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
IF OBJECT_ID(N'dbo.TR_Product_InsertPriceHistory', N'TR') IS NOT NULL
    DROP TRIGGER dbo.TR_Product_InsertPriceHistory;
""");

            migrationBuilder.Sql("""
CREATE TRIGGER dbo.TR_Product_InsertPriceHistory
ON dbo.Products
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF UPDATE(Price)
    BEGIN
        INSERT INTO dbo.PriceHistories
            (ProductId, OldPrice, NewPrice, CreationDatetime, UpdateDatetime, IsArchived)
        SELECT
            d.Id,
            d.Price,
            i.Price,
            SYSUTCDATETIME(),
            SYSUTCDATETIME(),
            0
        FROM inserted AS i
        INNER JOIN deleted AS d ON d.Id = i.Id
        WHERE ISNULL(d.Price, 0) <> ISNULL(i.Price, 0);
    END
END;
""");

            migrationBuilder.Sql("""
IF OBJECT_ID(N'dbo.TR_Remaining_CheckQuantity', N'TR') IS NOT NULL
    DROP TRIGGER dbo.TR_Remaining_CheckQuantity;
""");

            migrationBuilder.Sql("""
CREATE TRIGGER dbo.TR_Remaining_CheckQuantity
ON dbo.Remaining
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM inserted WHERE Quantity < 0)
    BEGIN
        THROW 50001, N'Нельзя установить отрицательный остаток товара.', 1;
    END
END;
""");

            migrationBuilder.Sql("""
IF OBJECT_ID(N'dbo.TR_Orders_AuditChanges', N'TR') IS NOT NULL
    DROP TRIGGER dbo.TR_Orders_AuditChanges;
""");

            migrationBuilder.Sql("""
CREATE TRIGGER dbo.TR_Orders_AuditChanges
ON dbo.Orders
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.OperationsAudits
        (OperationTime, TableName, OperationType, RecordId, OldData, NewData, UserId)
    SELECT
        SYSUTCDATETIME(),
        N'Orders',
        CASE WHEN d.Id IS NULL THEN N'INSERT' ELSE N'UPDATE' END,
        CAST(i.Id AS nvarchar(50)),
        COALESCE(
            CASE WHEN d.Id IS NOT NULL THEN (
                SELECT
                    d.StatusId,
                    d.EmployeeId,
                    d.TotalPrice,
                    d.WarehouseId,
                    d.UpdateDatetime
                FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
            ) END,
            N'{}'
        ),
        (
            SELECT
                i.StatusId,
                i.EmployeeId,
                i.TotalPrice,
                i.WarehouseId,
                i.UpdateDatetime
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        ),
        i.UserId
    FROM inserted AS i
    LEFT JOIN deleted AS d ON d.Id = i.Id;
END;
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.TR_Orders_AuditChanges;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.TR_Remaining_CheckQuantity;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.TR_Product_InsertPriceHistory;");
        }
    }
}
