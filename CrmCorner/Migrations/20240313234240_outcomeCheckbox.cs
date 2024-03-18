using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    /// <inheritdoc />
    public partial class outcomeCheckbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Outcome",
                table: "TaskComps");

            migrationBuilder.AddColumn<bool>(
                name: "IsPositiveOutcome",
                table: "TaskComps",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPositiveOutcome",
                table: "TaskComps");

            migrationBuilder.AddColumn<int>(
                name: "Outcome",
                table: "TaskComps",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
