using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    /// <inheritdoc />
    public partial class HeardFromEkle1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HeardFrom",
                table: "TaskComps",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeardFrom",
                table: "TaskComps");
        }
    }
}
