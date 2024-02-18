using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    /// <inheritdoc />
    public partial class updateCust1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "CustomerNs",
                type: "varchar(255)",
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerNs_AppUserId",
                table: "CustomerNs",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerNs_Users_AppUserId",
                table: "CustomerNs",
                column: "AppUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerNs_Users_AppUserId",
                table: "CustomerNs");

            migrationBuilder.DropIndex(
                name: "IX_CustomerNs_AppUserId",
                table: "CustomerNs");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "CustomerNs");
        }
    }
}
