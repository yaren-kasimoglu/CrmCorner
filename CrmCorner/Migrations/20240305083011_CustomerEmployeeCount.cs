using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    /// <inheritdoc />
    public partial class CustomerEmployeeCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmployeeCount",
                table: "TaskComps");

            migrationBuilder.AddColumn<int>(
                name: "EmployeeCount",
                table: "CustomerNs",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmployeeCount",
                table: "CustomerNs");

            migrationBuilder.AddColumn<int>(
                name: "EmployeeCount",
                table: "TaskComps",
                type: "int",
                nullable: true);
        }
    }
}
