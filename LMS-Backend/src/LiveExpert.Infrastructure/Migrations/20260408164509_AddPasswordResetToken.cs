using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiveExpert.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_ReceiverId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_SenderId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Users_StudentId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Users_TutorId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Users_UserId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Users_UserId1",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_SessionBookings_Users_StudentId",
                table: "SessionBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Users_TutorId",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_WhatsAppCampaigns_Users_CreatorId",
                table: "WhatsAppCampaigns");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_UserId1",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Reviews");

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetTokenExpiresAt",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CategoryId",
                table: "Subjects",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddColumn<bool>(
                name: "IsReminderSent",
                table: "Sessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AIResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Prompt = table.Column<string>(type: "TEXT", nullable: false),
                    Response = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIResponses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppCampaigns_CreatedAt",
                table: "WhatsAppCampaigns",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppCampaigns_CreatedBy",
                table: "WhatsAppCampaigns",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppCampaigns_Status",
                table: "WhatsAppCampaigns",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_ReceiverId",
                table: "Messages",
                column: "ReceiverId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_SenderId",
                table: "Messages",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Users_StudentId",
                table: "Reviews",
                column: "StudentId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Users_TutorId",
                table: "Reviews",
                column: "TutorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SessionBookings_Users_StudentId",
                table: "SessionBookings",
                column: "StudentId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Users_TutorId",
                table: "Sessions",
                column: "TutorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WhatsAppCampaigns_Users_CreatorId",
                table: "WhatsAppCampaigns",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_ReceiverId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_SenderId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Users_StudentId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Users_TutorId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_SessionBookings_Users_StudentId",
                table: "SessionBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Users_TutorId",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_WhatsAppCampaigns_Users_CreatorId",
                table: "WhatsAppCampaigns");

            migrationBuilder.DropTable(
                name: "AIResponses");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppCampaigns_CreatedAt",
                table: "WhatsAppCampaigns");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppCampaigns_CreatedBy",
                table: "WhatsAppCampaigns");

            migrationBuilder.DropIndex(
                name: "IX_WhatsAppCampaigns_Status",
                table: "WhatsAppCampaigns");

            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordResetTokenExpiresAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsReminderSent",
                table: "Sessions");

            migrationBuilder.AlterColumn<Guid>(
                name: "CategoryId",
                table: "Subjects",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Reviews",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "Reviews",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId1",
                table: "Reviews",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_ReceiverId",
                table: "Messages",
                column: "ReceiverId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_SenderId",
                table: "Messages",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Users_StudentId",
                table: "Reviews",
                column: "StudentId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Users_TutorId",
                table: "Reviews",
                column: "TutorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Users_UserId",
                table: "Reviews",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Users_UserId1",
                table: "Reviews",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SessionBookings_Users_StudentId",
                table: "SessionBookings",
                column: "StudentId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Users_TutorId",
                table: "Sessions",
                column: "TutorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WhatsAppCampaigns_Users_CreatorId",
                table: "WhatsAppCampaigns",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
