using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagerApi.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdersToEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WarehouseId",
                table: "Warehouses",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "Roles",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "RemainingId",
                table: "Remaining",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "Products",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "PriceHistoryId",
                table: "PriceHistories",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "OrderStatusId",
                table: "OrderStatuses",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "OrdersProductsId",
                table: "OrdersProducts",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "Orders",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "OperationsAuditId",
                table: "OperationsAudits",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "EmployeesWarehousesId",
                table: "EmployeesWarehouses",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "Employees",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Categories",
                newName: "Id");

            migrationBuilder.AddColumn<int>(
                name: "ProductId1",
                table: "Remaining",
                type: "int",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Warehouses",
                newName: "WarehouseId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Users",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Roles",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Remaining",
                newName: "RemainingId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Products",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "PriceHistories",
                newName: "PriceHistoryId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "OrderStatuses",
                newName: "OrderStatusId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "OrdersProducts",
                newName: "OrdersProductsId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Orders",
                newName: "OrderId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "OperationsAudits",
                newName: "OperationsAuditId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "EmployeesWarehouses",
                newName: "EmployeesWarehousesId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Employees",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Categories",
                newName: "CategoryId");
        }
    }
}
