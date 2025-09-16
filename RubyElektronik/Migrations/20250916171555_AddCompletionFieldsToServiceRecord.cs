using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RubyElektronik.Migrations
{
    /// <inheritdoc />
    public partial class AddCompletionFieldsToServiceRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompletionDescription",
                table: "ServiceRecords",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CompletionPrice",
                table: "ServiceRecords",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 11, 17, 15, 54, 920, DateTimeKind.Utc).AddTicks(2697));

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 14, 17, 15, 54, 920, DateTimeKind.Utc).AddTicks(2705));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 16, 17, 15, 54, 920, DateTimeKind.Utc).AddTicks(2538));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 16, 17, 15, 54, 920, DateTimeKind.Utc).AddTicks(2541));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 16, 17, 15, 54, 920, DateTimeKind.Utc).AddTicks(2543));

            migrationBuilder.UpdateData(
                table: "ServiceRecords",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CompletionDescription", "CompletionPrice" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ServiceRecords",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CompletionDescription", "CompletionPrice" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ServiceRecords",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CompletionDescription", "CompletionPrice" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 16, 17, 15, 54, 920, DateTimeKind.Utc).AddTicks(2673));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 16, 17, 15, 54, 920, DateTimeKind.Utc).AddTicks(2676));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 16, 17, 15, 54, 920, DateTimeKind.Utc).AddTicks(2678));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletionDescription",
                table: "ServiceRecords");

            migrationBuilder.DropColumn(
                name: "CompletionPrice",
                table: "ServiceRecords");

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 7, 7, 53, 14, 767, DateTimeKind.Utc).AddTicks(599));

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 10, 7, 53, 14, 767, DateTimeKind.Utc).AddTicks(607));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 7, 53, 14, 767, DateTimeKind.Utc).AddTicks(427));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 7, 53, 14, 767, DateTimeKind.Utc).AddTicks(430));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 7, 53, 14, 767, DateTimeKind.Utc).AddTicks(432));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 7, 53, 14, 767, DateTimeKind.Utc).AddTicks(573));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 7, 53, 14, 767, DateTimeKind.Utc).AddTicks(575));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 12, 7, 53, 14, 767, DateTimeKind.Utc).AddTicks(577));
        }
    }
}
