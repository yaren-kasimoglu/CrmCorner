using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    public partial class FixNullabilityError : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Eğer DropIndex veya DropForeignKey yapılacaksa, veritabanında var olduğundan emin olunmalı. Bu satırlar kaldırıldı.

            // NULL olan Name alanlarını boş string yap (veritabanı hatası almamak için)
            migrationBuilder.Sql("UPDATE `roles` SET `Name` = '' WHERE `Name` IS NULL");

            // Name kolonunu zorunlu hale getiriyoruz
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "roles",
                type: "longtext",
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            // Id alanını int'e çeviriyoruz (eğer daha önce değiştirilmişse)
            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "roles",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

     
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // İlişkiyi kaldır
            migrationBuilder.DropForeignKey(
                name: "FK_AppUserRole_roles_RoleId1",
                table: "AppUserRole");

            migrationBuilder.DropIndex(
                name: "IX_AppUserRole_RoleId1",
                table: "AppUserRole");

            migrationBuilder.DropColumn(
                name: "RoleId1",
                table: "AppUserRole");

            // Name kolonunu tekrar nullable yap
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "roles",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            // Id kolonunu tekrar string yap
            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "roles",
                type: "varchar(255)",
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);
        }
    }
}
