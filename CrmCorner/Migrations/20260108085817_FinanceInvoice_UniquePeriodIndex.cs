using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    /// <inheritdoc />
    public partial class FinanceInvoice_UniquePeriodIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_FinanceInvoices_CompanyId_ContractId_PeriodYear_PeriodMonth",
                table: "FinanceInvoices",
                columns: new[] { "CompanyId", "ContractId", "PeriodYear", "PeriodMonth" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FinanceInvoices_CompanyId_ContractId_PeriodYear_PeriodMonth",
                table: "FinanceInvoices");
        }
    }
}
