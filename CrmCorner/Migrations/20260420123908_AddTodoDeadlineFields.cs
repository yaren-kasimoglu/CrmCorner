using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    /// <inheritdoc />
    public partial class AddTodoDeadlineFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AlterColumn<string>(
                name: "AssignedById",
                table: "TodoEntries",
                type: "varchar(255)",
                nullable: true,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AddColumn<bool>(
                name: "DeadlineReminder2HoursSent",
                table: "TodoEntries",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DeadlineReminder3DaysSent",
                table: "TodoEntries",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DeadlineReminderLastDaySent",
                table: "TodoEntries",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DeadlineReminderWeekSent",
                table: "TodoEntries",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_TodoEntries_AssignedById",
                table: "TodoEntries",
                column: "AssignedById");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoEntries_users_AssignedById",
                table: "TodoEntries",
                column: "AssignedById",
                principalTable: "users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoEntries_users_AssignedById",
                table: "TodoEntries");

            migrationBuilder.DropIndex(
                name: "IX_TodoEntries_AssignedById",
                table: "TodoEntries");

            migrationBuilder.DropColumn(
                name: "DeadlineReminder2HoursSent",
                table: "TodoEntries");

            migrationBuilder.DropColumn(
                name: "DeadlineReminder3DaysSent",
                table: "TodoEntries");

            migrationBuilder.DropColumn(
                name: "DeadlineReminderLastDaySent",
                table: "TodoEntries");

            migrationBuilder.DropColumn(
                name: "DeadlineReminderWeekSent",
                table: "TodoEntries");

            migrationBuilder.AlterColumn<string>(
                name: "AssignedById",
                table: "TodoEntries",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

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
    }
}
