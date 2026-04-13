using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiveExpert.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTrialSubscriptionAndResume : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSubscribed",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscribedUntil",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TrialEndDate",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TrialStartDate",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResumeData",
                table: "StudentProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResumeLastUpdatedAt",
                table: "StudentProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResumeType",
                table: "StudentProfiles",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSubscribed",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SubscribedUntil",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TrialEndDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TrialStartDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ResumeData",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "ResumeLastUpdatedAt",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "ResumeType",
                table: "StudentProfiles");
        }
    }
}
