using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    /// <inheritdoc />
    public partial class fileUpdate1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TaskId",
                table: "FileAttachments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_FileAttachments_TaskId",
                table: "FileAttachments",
                column: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_FileAttachments_TaskComps_TaskId",
                table: "FileAttachments",
                column: "TaskId",
                principalTable: "TaskComps",
                principalColumn: "TaskId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileAttachments_TaskComps_TaskId",
                table: "FileAttachments");

            migrationBuilder.DropIndex(
                name: "IX_FileAttachments_TaskId",
                table: "FileAttachments");

            migrationBuilder.DropColumn(
                name: "TaskId",
                table: "FileAttachments");
        }
    }
}
