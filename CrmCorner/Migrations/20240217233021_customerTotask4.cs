using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    /// <inheritdoc />
    public partial class customerTotask4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "TaskComps",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskComps_CustomerId",
                table: "TaskComps",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskComps_CustomerNs_CustomerId",
                table: "TaskComps",
                column: "CustomerId",
                principalTable: "CustomerNs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskComps_CustomerNs_CustomerId",
                table: "TaskComps");

            migrationBuilder.DropIndex(
                name: "IX_TaskComps_CustomerId",
                table: "TaskComps");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "TaskComps");
        }
    }
}
