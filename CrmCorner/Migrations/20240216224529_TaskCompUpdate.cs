using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    /// <inheritdoc />
    public partial class TaskCompUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "TaskComps",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskComps_StatusId",
                table: "TaskComps",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskComps_Statuses_StatusId",
                table: "TaskComps",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskComps_Statuses_StatusId",
                table: "TaskComps");

            migrationBuilder.DropIndex(
                name: "IX_TaskComps_StatusId",
                table: "TaskComps");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "TaskComps");
        }
    }
}
