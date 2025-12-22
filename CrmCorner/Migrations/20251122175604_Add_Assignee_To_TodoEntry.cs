using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    /// <inheritdoc />
    public partial class Add_Assignee_To_TodoEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssigneeId",
                table: "TodoEntries",
                type: "varchar(255)",
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TodoEntries_AssigneeId",
                table: "TodoEntries",
                column: "AssigneeId");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoEntries_users_AssigneeId",
                table: "TodoEntries",
                column: "AssigneeId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoEntries_users_AssigneeId",
                table: "TodoEntries");

            migrationBuilder.DropIndex(
                name: "IX_TodoEntries_AssigneeId",
                table: "TodoEntries");

            migrationBuilder.DropColumn(
                name: "AssigneeId",
                table: "TodoEntries");
        }
    }
}
