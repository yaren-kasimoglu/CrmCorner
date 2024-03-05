using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    /// <inheritdoc />
    public partial class IndustryforCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Industry",
                table: "CustomerNs",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Industry",
                table: "CustomerNs");
        }
    }
}
