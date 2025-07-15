using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    public partial class FixAppUserRoleRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Eski foreign key varsa kaldır (RoleId1)
            migrationBuilder.DropForeignKey(
                name: "FK_AppUserRole_roles_RoleId1",
                table: "AppUserRole");

            // Eski foreign key varsa kaldır (UserId)
            migrationBuilder.DropForeignKey(
                name: "FK_AppUserRole_users_UserId",
                table: "AppUserRole");

            // RoleId1 alanını kaldır
            migrationBuilder.DropIndex(
                name: "IX_AppUserRole_RoleId1",
                table: "AppUserRole");

            migrationBuilder.DropColumn(
                name: "RoleId1",
                table: "AppUserRole");

            // Tabloyu yeniden adlandır (AppUserRoles)
            migrationBuilder.RenameTable(
                name: "AppUserRole",
                newName: "AppUserRoles");

            // roles tablosundaki Id alanını string yap
            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "roles",
                type: "varchar(255)",
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(int),
                oldType: "int");

            // Yeni primary key
            migrationBuilder.AddPrimaryKey(
                name: "PK_AppUserRoles",
                table: "AppUserRoles",
                columns: new[] { "UserId", "RoleId" });

            // RoleId üzerine index oluştur
            migrationBuilder.CreateIndex(
                name: "IX_AppUserRoles_RoleId",
                table: "AppUserRoles",
                column: "RoleId");

            // Yeni foreign key bağlantılarını kur
            migrationBuilder.AddForeignKey(
                name: "FK_AppUserRoles_roles_RoleId",
                table: "AppUserRoles",
                column: "RoleId",
                principalTable: "roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppUserRoles_users_UserId",
                table: "AppUserRoles",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppUserRoles_roles_RoleId",
                table: "AppUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AppUserRoles_users_UserId",
                table: "AppUserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppUserRoles",
                table: "AppUserRoles");

            migrationBuilder.DropIndex(
                name: "IX_AppUserRoles_RoleId",
                table: "AppUserRoles");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "roles",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.RenameTable(
                name: "AppUserRoles",
                newName: "AppUserRole");

            migrationBuilder.AddColumn<int>(
                name: "RoleId1",
                table: "AppUserRole",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppUserRole",
                table: "AppUserRole",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppUserRole_RoleId1",
                table: "AppUserRole",
                column: "RoleId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AppUserRole_roles_RoleId1",
                table: "AppUserRole",
                column: "RoleId1",
                principalTable: "roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppUserRole_users_UserId",
                table: "AppUserRole",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
