using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    public partial class AddCreatedByFieldsToTaskComps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // TaskComps tablosuna CreatedBy alanını ekle
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TaskComps",
                type: "longtext",
                nullable: false,
                defaultValue: "DefaultUser",
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            // TaskComps tablosuna CreatedByCompanyId alanını ekle
            migrationBuilder.AddColumn<int>(
                name: "CreatedByCompanyId",
                table: "TaskComps",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // TaskComps tablosundan CreatedBy alanını kaldır
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TaskComps");

            // TaskComps tablosundan CreatedByCompanyId alanını kaldır
            migrationBuilder.DropColumn(
                name: "CreatedByCompanyId",
                table: "TaskComps");
        }
    }
}
