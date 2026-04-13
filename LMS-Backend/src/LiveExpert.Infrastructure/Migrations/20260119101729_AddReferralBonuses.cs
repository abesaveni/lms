using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiveExpert.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReferralBonuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "JoiningBonusAmount",
                table: "ReferralPrograms",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "JoiningBonusPaidAt",
                table: "ReferralPrograms",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReferralBonusPaidAt",
                table: "ReferralPrograms",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JoiningBonusAmount",
                table: "ReferralPrograms");

            migrationBuilder.DropColumn(
                name: "JoiningBonusPaidAt",
                table: "ReferralPrograms");

            migrationBuilder.DropColumn(
                name: "ReferralBonusPaidAt",
                table: "ReferralPrograms");
        }
    }
}
