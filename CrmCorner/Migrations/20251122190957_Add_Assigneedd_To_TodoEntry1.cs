using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    /// <inheritdoc />
    public partial class Add_Assigneedd_To_TodoEntry1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedByUserId",
                table: "TodoEntries",
                type: "varchar(255)",
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TodoEntries_AssignedByUserId",
                table: "TodoEntries",
                column: "AssignedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoEntries_users_AssignedByUserId",
                table: "TodoEntries",
                column: "AssignedByUserId",
                principalTable: "users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoEntries_users_AssignedByUserId",
                table: "TodoEntries");

            migrationBuilder.DropIndex(
                name: "IX_TodoEntries_AssignedByUserId",
                table: "TodoEntries");

            migrationBuilder.DropColumn(
                name: "AssignedByUserId",
                table: "TodoEntries");
        }
    }
}
