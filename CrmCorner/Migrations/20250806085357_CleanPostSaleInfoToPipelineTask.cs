using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    /// <inheritdoc />
    public partial class CleanPostSaleInfoToPipelineTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_PostSaleInfos_PipelineTasks_PipelineTaskId",
                table: "PostSaleInfos",
                column: "PipelineTaskId",
                principalTable: "PipelineTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostSaleInfos_PipelineTasks_PipelineTaskId",
                table: "PostSaleInfos");

        }
    }
}
