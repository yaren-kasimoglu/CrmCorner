using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    /// <inheritdoc />
    public partial class CompanyIdToSocialMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "SocialMediaContents",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SocialMediaContents_CompanyId",
                table: "SocialMediaContents",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_SocialMediaContents_Companies_CompanyId",
                table: "SocialMediaContents",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SocialMediaContents_Companies_CompanyId",
                table: "SocialMediaContents");

            migrationBuilder.DropIndex(
                name: "IX_SocialMediaContents_CompanyId",
                table: "SocialMediaContents");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "SocialMediaContents");
        }
    }
}
