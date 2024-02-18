using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    /// <inheritdoc />
    public partial class calendarforUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "Calendar",
                type: "varchar(255)",
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Calendar",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Calendar_AppUserId",
                table: "Calendar",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Calendar_Users_AppUserId",
                table: "Calendar",
                column: "AppUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Calendar_Users_AppUserId",
                table: "Calendar");

            migrationBuilder.DropIndex(
                name: "IX_Calendar_AppUserId",
                table: "Calendar");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "Calendar");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Calendar");
        }
    }
}
