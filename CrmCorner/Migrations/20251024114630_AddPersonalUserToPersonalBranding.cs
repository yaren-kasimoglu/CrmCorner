using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonalUserToPersonalBranding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PersonalUserId",
                table: "PersonalBrandingContents",
                type: "varchar(255)",
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalBrandingContents_PersonalUserId",
                table: "PersonalBrandingContents",
                column: "PersonalUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonalBrandingContents_users_PersonalUserId",
                table: "PersonalBrandingContents",
                column: "PersonalUserId",
                principalTable: "users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonalBrandingContents_users_PersonalUserId",
                table: "PersonalBrandingContents");

            migrationBuilder.DropIndex(
                name: "IX_PersonalBrandingContents_PersonalUserId",
                table: "PersonalBrandingContents");

            migrationBuilder.DropColumn(
                name: "PersonalUserId",
                table: "PersonalBrandingContents");
        }
    }
}
