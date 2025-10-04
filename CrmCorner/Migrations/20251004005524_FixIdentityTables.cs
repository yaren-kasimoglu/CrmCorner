using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    public partial class FixIdentityTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 🔹 AppUserRole tablosunu küçük harfe indir
            migrationBuilder.RenameTable(
                name: "AppUserRole",
                newName: "appuserrole");


            // 🔹 appuserrole → PK kolonları
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "appuserrole",
                type: "varchar(255)",
                nullable: false,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "RoleId",
                table: "appuserrole",
                type: "varchar(255)",
                nullable: false,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_appuserrole",
                table: "appuserrole",
                columns: new[] { "UserId", "RoleId" });

            // 🔹 usertokens → composite PK
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "usertokens",
                type: "varchar(255)",
                nullable: false,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "usertokens",
                type: "varchar(255)",
                nullable: false,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "usertokens",
                type: "varchar(255)",
                nullable: false,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_usertokens",
                table: "usertokens",
                columns: new[] { "UserId", "LoginProvider", "Name" });

            // 🔹 userlogins → composite PK
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "userlogins",
                type: "varchar(255)",
                nullable: false,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "userlogins",
                type: "varchar(255)",
                nullable: false,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "userlogins",
                type: "varchar(255)",
                nullable: false,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_userlogins",
                table: "userlogins",
                columns: new[] { "LoginProvider", "ProviderKey" });

            // 🔹 userclaims
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "userclaims",
                type: "varchar(255)",
                nullable: false,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            // 🔹 roleclaims
            migrationBuilder.AlterColumn<string>(
                name: "RoleId",
                table: "roleclaims",
                type: "varchar(255)",
                nullable: false,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            // 🔹 Foreign Keys
            migrationBuilder.AddForeignKey(
                name: "FK_appuserrole_roles_RoleId",
                table: "appuserrole",
                column: "RoleId",
                principalTable: "roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_appuserrole_users_UserId",
                table: "appuserrole",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_appuserrole_roles_RoleId",
                table: "appuserrole");

            migrationBuilder.DropForeignKey(
                name: "FK_appuserrole_users_UserId",
                table: "appuserrole");

            migrationBuilder.DropPrimaryKey(
                name: "PK_appuserrole",
                table: "appuserrole");

            migrationBuilder.RenameTable(
                name: "appuserrole",
                newName: "AppUserRole");


        }
    }
}
