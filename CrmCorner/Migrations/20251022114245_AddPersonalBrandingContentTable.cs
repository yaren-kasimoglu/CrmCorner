using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonalBrandingContentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PersonalBrandingContentId",
                table: "Feedbacks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PersonalBrandingContents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MediaFile = table.Column<byte[]>(type: "longblob", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EstimatedPublishDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalBrandingContents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalBrandingContents_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "CompanyId");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_PersonalBrandingContentId",
                table: "Feedbacks",
                column: "PersonalBrandingContentId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalBrandingContents_CompanyId",
                table: "PersonalBrandingContents",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_PersonalBrandingContents_PersonalBrandingContentId",
                table: "Feedbacks",
                column: "PersonalBrandingContentId",
                principalTable: "PersonalBrandingContents",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_PersonalBrandingContents_PersonalBrandingContentId",
                table: "Feedbacks");

            migrationBuilder.DropTable(
                name: "PersonalBrandingContents");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_PersonalBrandingContentId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "PersonalBrandingContentId",
                table: "Feedbacks");
        }
    }
}
