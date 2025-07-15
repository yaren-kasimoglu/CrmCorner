using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    /// <inheritdoc />
    public partial class AddContactMethodsToPipelineTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ContactedViaColdCall",
                table: "PipelineTasks",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ContactedViaLinkedIn",
                table: "PipelineTasks",
                type: "tinyint(1)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactedViaColdCall",
                table: "PipelineTasks");

            migrationBuilder.DropColumn(
                name: "ContactedViaLinkedIn",
                table: "PipelineTasks");
        }
    }
}
