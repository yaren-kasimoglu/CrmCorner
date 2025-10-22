using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonalBrandingFeedbackTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_PersonalBrandingContents_PersonalBrandingContentId",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_PersonalBrandingContentId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "PersonalBrandingContentId",
                table: "Feedbacks");

            migrationBuilder.CreateTable(
                name: "PersonalBrandingFeedbacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PersonalBrandingContentId = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedById = table.Column<string>(type: "varchar(255)", nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalBrandingFeedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalBrandingFeedbacks_PersonalBrandingContents_PersonalB~",
                        column: x => x.PersonalBrandingContentId,
                        principalTable: "PersonalBrandingContents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonalBrandingFeedbacks_users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalBrandingFeedbacks_CreatedById",
                table: "PersonalBrandingFeedbacks",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalBrandingFeedbacks_PersonalBrandingContentId",
                table: "PersonalBrandingFeedbacks",
                column: "PersonalBrandingContentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonalBrandingFeedbacks");

            migrationBuilder.AddColumn<int>(
                name: "PersonalBrandingContentId",
                table: "Feedbacks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_PersonalBrandingContentId",
                table: "Feedbacks",
                column: "PersonalBrandingContentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_PersonalBrandingContents_PersonalBrandingContentId",
                table: "Feedbacks",
                column: "PersonalBrandingContentId",
                principalTable: "PersonalBrandingContents",
                principalColumn: "Id");
        }
    }
}
