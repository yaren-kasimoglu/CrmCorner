using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    /// <inheritdoc />
    public partial class AddUsdAndContractFieldsToFinanceInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CommissionUsd",
                table: "FinanceInvoices",
                type: "decimal(65,30)",
                nullable: true);

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

            migrationBuilder.AddColumn<decimal>(
                name: "SaleAmountUsd",
                table: "FinanceInvoices",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UsdRateAtSale",
                table: "FinanceInvoices",
                type: "decimal(65,30)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommissionUsd",
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
                name: "SaleAmountUsd",
                table: "FinanceInvoices");

            migrationBuilder.DropColumn(
                name: "UsdRateAtSale",
                table: "FinanceInvoices");
        }
    }
}
