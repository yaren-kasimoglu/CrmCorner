using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneToApolloContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
     name: "Phone",
     table: "ApolloContacts",
     type: "longtext",
     nullable: true)
     .Annotation("MySql:CharSet", "utf8mb4");

        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Phone",
                table: "ApolloContacts");
        }

    }
}
