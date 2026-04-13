using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiveExpert.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyChallenges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyChallenges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChallengeDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    ContentJson = table.Column<string>(type: "TEXT", nullable: false),
                    AnswerJson = table.Column<string>(type: "TEXT", nullable: false),
                    XpReward = table.Column<int>(type: "INTEGER", nullable: false),
                    Difficulty = table.Column<string>(type: "TEXT", nullable: false),
                    Tag = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyChallenges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserChallengeStreaks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CurrentStreak = table.Column<int>(type: "INTEGER", nullable: false),
                    LongestStreak = table.Column<int>(type: "INTEGER", nullable: false),
                    LastCompletedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TotalCompleted = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalXpEarned = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserChallengeStreaks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserChallengeStreaks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserChallengeAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChallengeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SubmittedAnswerJson = table.Column<string>(type: "TEXT", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false),
                    Result = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeTakenSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserChallengeAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserChallengeAttempts_DailyChallenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "DailyChallenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserChallengeAttempts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserChallengeAttempts_ChallengeId",
                table: "UserChallengeAttempts",
                column: "ChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserChallengeAttempts_UserId",
                table: "UserChallengeAttempts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserChallengeStreaks_UserId",
                table: "UserChallengeStreaks",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserChallengeAttempts");

            migrationBuilder.DropTable(
                name: "UserChallengeStreaks");

            migrationBuilder.DropTable(
                name: "DailyChallenges");
        }
    }
}
