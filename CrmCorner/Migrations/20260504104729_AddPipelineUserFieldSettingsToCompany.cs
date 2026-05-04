using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    /// <inheritdoc />
    public partial class AddPipelineUserFieldSettingsToCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MeetingUserId",
                table: "PipelineTasks",
                type: "varchar(255)",
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ReporterUserId",
                table: "PipelineTasks",
                type: "varchar(255)",
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "UseAppUser",
                table: "Companies",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UseMeetingUser",
                table: "Companies",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UseReporterUser",
                table: "Companies",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UseResponsibleUser",
                table: "Companies",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_PipelineTasks_MeetingUserId",
                table: "PipelineTasks",
                column: "MeetingUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PipelineTasks_ReporterUserId",
                table: "PipelineTasks",
                column: "ReporterUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PipelineTasks_users_MeetingUserId",
                table: "PipelineTasks",
                column: "MeetingUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PipelineTasks_users_ReporterUserId",
                table: "PipelineTasks",
                column: "ReporterUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PipelineTasks_users_MeetingUserId",
                table: "PipelineTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_PipelineTasks_users_ReporterUserId",
                table: "PipelineTasks");

            migrationBuilder.DropIndex(
                name: "IX_PipelineTasks_MeetingUserId",
                table: "PipelineTasks");

            migrationBuilder.DropIndex(
                name: "IX_PipelineTasks_ReporterUserId",
                table: "PipelineTasks");

            migrationBuilder.DropColumn(
                name: "MeetingUserId",
                table: "PipelineTasks");

            migrationBuilder.DropColumn(
                name: "ReporterUserId",
                table: "PipelineTasks");

            migrationBuilder.DropColumn(
                name: "UseAppUser",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "UseMeetingUser",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "UseReporterUser",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "UseResponsibleUser",
                table: "Companies");
        }
    }
}
