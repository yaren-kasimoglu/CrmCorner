using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    /// <inheritdoc />
    public partial class FinanceRefactor_Clean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.DropColumn(
                name: "CommissionUsd",
                table: "FinanceInvoices");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "FinanceInvoices");

            migrationBuilder.DropColumn(
                name: "ContractEndDate",
                table: "FinanceInvoices");

            migrationBuilder.DropColumn(
                name: "ContractMonths",
                table: "FinanceInvoices");

            migrationBuilder.DropColumn(
                name: "ContractStartDate",
                table: "FinanceInvoices");

            migrationBuilder.DropColumn(
                name: "KimSattiUserId",
                table: "FinanceInvoices");

            migrationBuilder.DropColumn(
                name: "SaleAmountUsd",
                table: "FinanceInvoices");

            migrationBuilder.DropColumn(
                name: "SdrUserId",
                table: "FinanceInvoices");

            migrationBuilder.DropColumn(
                name: "UsdRateAtSale",
                table: "FinanceInvoices");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CommissionUsd",
                table: "FinanceInvoices",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "FinanceInvoices",
                type: "longtext",
                nullable: false,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractEndDate",
                table: "FinanceInvoices",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContractMonths",
                table: "FinanceInvoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractStartDate",
                table: "FinanceInvoices",
                type: "datetime(6)",
                nullable: true);

            // ✅ nullable olmalı (defaultValue yok!)
            migrationBuilder.AddColumn<string>(
                name: "KimSattiUserId",
                table: "FinanceInvoices",
                type: "varchar(255)",
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "SaleAmountUsd",
                table: "FinanceInvoices",
                type: "decimal(65,30)",
                nullable: true);

            // ✅ nullable olmalı (defaultValue yok!)
            migrationBuilder.AddColumn<string>(
                name: "SdrUserId",
                table: "FinanceInvoices",
                type: "varchar(255)",
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "UsdRateAtSale",
                table: "FinanceInvoices",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinanceInvoices_KimSattiUserId",
                table: "FinanceInvoices",
                column: "KimSattiUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceInvoices_SdrUserId",
                table: "FinanceInvoices",
                column: "SdrUserId");

            // ✅ Nullable FK için SetNull daha doğru
            migrationBuilder.AddForeignKey(
                name: "FK_FinanceInvoices_users_KimSattiUserId",
                table: "FinanceInvoices",
                column: "KimSattiUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FinanceInvoices_users_SdrUserId",
                table: "FinanceInvoices",
                column: "SdrUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
