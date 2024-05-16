using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    /// <inheritdoc />
    public partial class postSales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PostSaleInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TaskCompId = table.Column<int>(type: "int", nullable: false),
                    IsFirstPaymentMade = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsThereAProblem = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ProblemDescription = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsContinuationConsidered = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsTrustpilotReviewed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CanUseLogo = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostSaleInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostSaleInfos_TaskComps_TaskCompId",
                        column: x => x.TaskCompId,
                        principalTable: "TaskComps",
                        principalColumn: "TaskId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateIndex(
                name: "IX_PostSaleInfos_TaskCompId",
                table: "PostSaleInfos",
                column: "TaskCompId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostSaleInfos");
        }
    }
}
