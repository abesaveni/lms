using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiveExpert.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFlashSaleCouponsBackgroundCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Session: Flash Sale + Instant Booking + No-Show Protection ────────
            migrationBuilder.AddColumn<decimal>(
                name: "FlashSalePrice",
                table: "Sessions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FlashSaleEndsAt",
                table: "Sessions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "InstantBooking",
                table: "Sessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NoShowProtection",
                table: "Sessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            // ── TutorProfile: Background Check Badge ──────────────────────────────
            migrationBuilder.AddColumn<bool>(
                name: "HasBackgroundCheck",
                table: "TutorProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "BackgroundCheckDate",
                table: "TutorProfiles",
                type: "TEXT",
                nullable: true);

            // ── CouponCodes table ──────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "CouponCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    DiscountType = table.Column<int>(type: "INTEGER", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "TEXT", nullable: false),
                    MaxDiscountAmount = table.Column<decimal>(type: "TEXT", nullable: true),
                    MinOrderAmount = table.Column<decimal>(type: "TEXT", nullable: true),
                    MaxUses = table.Column<int>(type: "INTEGER", nullable: true),
                    UsedCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedByAdminId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponCodes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CouponCodes_Code",
                table: "CouponCodes",
                column: "Code",
                unique: true);

            // ── CouponUsages table ─────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "CouponUsages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CouponId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StudentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BookingId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DiscountApplied = table.Column<decimal>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponUsages_CouponCodes_CouponId",
                        column: x => x.CouponId,
                        principalTable: "CouponCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CouponUsages_CouponId",
                table: "CouponUsages",
                column: "CouponId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CouponUsages");
            migrationBuilder.DropTable(name: "CouponCodes");
            migrationBuilder.DropColumn(name: "FlashSalePrice", table: "Sessions");
            migrationBuilder.DropColumn(name: "FlashSaleEndsAt", table: "Sessions");
            migrationBuilder.DropColumn(name: "InstantBooking", table: "Sessions");
            migrationBuilder.DropColumn(name: "NoShowProtection", table: "Sessions");
            migrationBuilder.DropColumn(name: "HasBackgroundCheck", table: "TutorProfiles");
            migrationBuilder.DropColumn(name: "BackgroundCheckDate", table: "TutorProfiles");
        }
    }
}
