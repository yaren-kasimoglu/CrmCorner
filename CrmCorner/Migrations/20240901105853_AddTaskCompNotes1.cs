using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskCompNotes1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskCompNote_TaskComps_TaskCompId",
                table: "TaskCompNote");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskCompNote",
                table: "TaskCompNote");

            migrationBuilder.RenameTable(
                name: "TaskCompNote",
                newName: "TaskCompNotes");

            migrationBuilder.RenameIndex(
                name: "IX_TaskCompNote_TaskCompId",
                table: "TaskCompNotes",
                newName: "IX_TaskCompNotes_TaskCompId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskCompNotes",
                table: "TaskCompNotes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskCompNotes_TaskComps_TaskCompId",
                table: "TaskCompNotes",
                column: "TaskCompId",
                principalTable: "TaskComps",
                principalColumn: "TaskId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskCompNotes_TaskComps_TaskCompId",
                table: "TaskCompNotes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskCompNotes",
                table: "TaskCompNotes");

            migrationBuilder.RenameTable(
                name: "TaskCompNotes",
                newName: "TaskCompNote");

            migrationBuilder.RenameIndex(
                name: "IX_TaskCompNotes_TaskCompId",
                table: "TaskCompNote",
                newName: "IX_TaskCompNote_TaskCompId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskCompNote",
                table: "TaskCompNote",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskCompNote_TaskComps_TaskCompId",
                table: "TaskCompNote",
                column: "TaskCompId",
                principalTable: "TaskComps",
                principalColumn: "TaskId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
