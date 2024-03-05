using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    /// <inheritdoc />
    public partial class taskAssignedUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedUserId",
                table: "TaskComps",
                type: "varchar(255)",
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TaskComps_AssignedUserId",
                table: "TaskComps",
                column: "AssignedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskComps_Users_AssignedUserId",
                table: "TaskComps",
                column: "AssignedUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskComps_Users_AssignedUserId",
                table: "TaskComps");

            migrationBuilder.DropIndex(
                name: "IX_TaskComps_AssignedUserId",
                table: "TaskComps");

            migrationBuilder.DropColumn(
                name: "AssignedUserId",
                table: "TaskComps");
        }
    }
}
