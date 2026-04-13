using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiveExpert.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformFeePayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlatformFeePayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    StudentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    GatewayOrderId = table.Column<string>(type: "TEXT", nullable: false),
                    GatewayPaymentId = table.Column<string>(type: "TEXT", nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformFeePayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformFeePayments_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformFeePayments_StudentId",
                table: "PlatformFeePayments",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlatformFeePayments");
        }
    }
}
