using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    public partial class FixIdentityRelations3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 🔹 Buraya veritabanında yapmak istediğin değişiklikleri ekle
            // Örn: kolon ekleme/silme, FK düzeltme vb.

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

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "roles",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 🔹 Burada ise yukarıdaki işlemleri geri alıyoruz
            migrationBuilder.DropColumn(
                name: "Description",
                table: "roles");

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
        }
    }
}
