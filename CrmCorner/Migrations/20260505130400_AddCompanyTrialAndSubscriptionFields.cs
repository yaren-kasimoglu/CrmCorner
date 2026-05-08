using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmCorner.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyTrialAndSubscriptionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPaymentActive",
                table: "Companies",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTrialActive",
                table: "Companies",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "LastPaymentAmount",
                table: "Companies",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPaymentDate",
                table: "Companies",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaidUserCount",
                table: "Companies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PlanName",
                table: "Companies",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionEndDate",
                table: "Companies",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionStartDate",
                table: "Companies",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TrialEndDate",
                table: "Companies",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TrialStartDate",
                table: "Companies",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPaymentActive",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "IsTrialActive",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "LastPaymentAmount",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "LastPaymentDate",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PaidUserCount",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PlanName",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "SubscriptionEndDate",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "SubscriptionStartDate",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "TrialEndDate",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "TrialStartDate",
                table: "Companies");
        }
    }
}
