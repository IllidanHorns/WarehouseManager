using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagerApi.Migrations
{
    /// <inheritdoc />
    public partial class AddMetricsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Employees_EmployeeId1",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Remaining_Products_ProductId1",
                table: "Remaining");

            migrationBuilder.DropIndex(
                name: "IX_Remaining_ProductId1",
                table: "Remaining");

            migrationBuilder.DropIndex(
                name: "IX_Orders_EmployeeId1",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ProductId1",
                table: "Remaining");

            migrationBuilder.DropColumn(
                name: "EmployeeId1",
                table: "Orders");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPrice",
                table: "Orders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)",
                oldPrecision: 12,
                oldScale: 2);

            migrationBuilder.CreateTable(
                name: "ApplicationMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MetricName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Value = table.Column<double>(type: "float", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationMetrics", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationMetrics_MetricName",
                table: "ApplicationMetrics",
                column: "MetricName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationMetrics");

            migrationBuilder.AddColumn<int>(
                name: "ProductId1",
                table: "Remaining",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPrice",
                table: "Orders",
                type: "decimal(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<int>(
                name: "EmployeeId1",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Remaining_ProductId1",
                table: "Remaining",
                column: "ProductId1");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_EmployeeId1",
                table: "Orders",
                column: "EmployeeId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Employees_EmployeeId1",
                table: "Orders",
                column: "EmployeeId1",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Remaining_Products_ProductId1",
                table: "Remaining",
                column: "ProductId1",
                principalTable: "Products",
                principalColumn: "Id");
        }
    }
}
