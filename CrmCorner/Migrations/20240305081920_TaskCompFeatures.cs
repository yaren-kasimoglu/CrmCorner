using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    /// <inheritdoc />
    public partial class TaskCompFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmployeeCount",
                table: "TaskComps",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFinalDecisionMaker",
                table: "TaskComps",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Outcome",
                table: "TaskComps",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmployeeCount",
                table: "TaskComps");

            migrationBuilder.DropColumn(
                name: "IsFinalDecisionMaker",
                table: "TaskComps");

            migrationBuilder.DropColumn(
                name: "Outcome",
                table: "TaskComps");
        }
    }
}
