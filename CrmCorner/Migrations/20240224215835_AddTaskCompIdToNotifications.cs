using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskCompIdToNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TaskCompId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TaskCompId",
                table: "Notifications",
                column: "TaskCompId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_TaskComps_TaskCompId",
                table: "Notifications",
                column: "TaskCompId",
                principalTable: "TaskComps",
                principalColumn: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_TaskComps_TaskCompId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_TaskCompId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "TaskCompId",
                table: "Notifications");
        }
    }
}
