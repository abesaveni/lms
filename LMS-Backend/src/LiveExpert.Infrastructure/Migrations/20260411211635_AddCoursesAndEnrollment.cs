using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiveExpert.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCoursesAndEnrollment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AgeGroups",
                table: "TutorProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AutoPayoutMinimumAmount",
                table: "TutorProfiles",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "AutoPayoutSchedule",
                table: "TutorProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TeachingStyles",
                table: "TutorProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TrialAvailable",
                table: "TutorProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TrialDurationMinutes",
                table: "TutorProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TrialPrice",
                table: "TutorProfiles",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    ShortDescription = table.Column<string>(type: "TEXT", nullable: true),
                    FullDescription = table.Column<string>(type: "TEXT", nullable: true),
                    SubjectId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SubjectName = table.Column<string>(type: "TEXT", nullable: true),
                    CategoryName = table.Column<string>(type: "TEXT", nullable: true),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    Language = table.Column<string>(type: "TEXT", nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "TEXT", nullable: true),
                    TagsJson = table.Column<string>(type: "TEXT", nullable: true),
                    TotalSessions = table.Column<int>(type: "INTEGER", nullable: false),
                    SessionDurationMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    DeliveryType = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxStudentsPerBatch = table.Column<int>(type: "INTEGER", nullable: false),
                    PricePerSession = table.Column<decimal>(type: "TEXT", nullable: false),
                    BundlePrice = table.Column<decimal>(type: "TEXT", nullable: true),
                    AllowPartialBooking = table.Column<bool>(type: "INTEGER", nullable: false),
                    MinSessionsForPartial = table.Column<int>(type: "INTEGER", nullable: false),
                    RefundPolicy = table.Column<string>(type: "TEXT", nullable: true),
                    TrialAvailable = table.Column<bool>(type: "INTEGER", nullable: false),
                    TrialDurationMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    TrialPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    Prerequisites = table.Column<string>(type: "TEXT", nullable: true),
                    MaterialsRequired = table.Column<string>(type: "TEXT", nullable: true),
                    WhatYouWillLearn = table.Column<string>(type: "TEXT", nullable: true),
                    SyllabusJson = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    IsVisible = table.Column<bool>(type: "INTEGER", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TotalEnrollments = table.Column<int>(type: "INTEGER", nullable: false),
                    AverageRating = table.Column<decimal>(type: "TEXT", nullable: false),
                    TotalReviews = table.Column<int>(type: "INTEGER", nullable: false),
                    TutorProfileId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Courses_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Courses_TutorProfiles_TutorProfileId",
                        column: x => x.TutorProfileId,
                        principalTable: "TutorProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Courses_Users_TutorId",
                        column: x => x.TutorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TutorSubjectRates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SubjectId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SubjectName = table.Column<string>(type: "TEXT", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "TEXT", nullable: false),
                    TrialRate = table.Column<decimal>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    TutorProfileId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorSubjectRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TutorSubjectRates_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TutorSubjectRates_TutorProfiles_TutorProfileId",
                        column: x => x.TutorProfileId,
                        principalTable: "TutorProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TutorSubjectRates_Users_TutorId",
                        column: x => x.TutorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseEnrollments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CourseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StudentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EnrollmentType = table.Column<int>(type: "INTEGER", nullable: false),
                    SessionsPurchased = table.Column<int>(type: "INTEGER", nullable: false),
                    SessionsCompleted = table.Column<int>(type: "INTEGER", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "TEXT", nullable: false),
                    PlatformFee = table.Column<decimal>(type: "TEXT", nullable: false),
                    TutorEarningAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    GatewayOrderId = table.Column<string>(type: "TEXT", nullable: true),
                    GatewayPaymentId = table.Column<string>(type: "TEXT", nullable: true),
                    GatewaySignature = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    EnrolledAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CancellationReason = table.Column<string>(type: "TEXT", nullable: true),
                    RefundAmount = table.Column<decimal>(type: "TEXT", nullable: true),
                    RefundedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseEnrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseEnrollments_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseEnrollments_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CourseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SessionNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    TopicsCovered = table.Column<string>(type: "TEXT", nullable: true),
                    ScheduledAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DurationMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    MeetingLink = table.Column<string>(type: "TEXT", nullable: true),
                    RecordingUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TutorNotes = table.Column<string>(type: "TEXT", nullable: true),
                    HomeworkAssigned = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseSessions_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseSessions_Users_TutorId",
                        column: x => x.TutorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrialSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StudentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CourseId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ScheduledAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DurationMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    MeetingLink = table.Column<string>(type: "TEXT", nullable: true),
                    Price = table.Column<decimal>(type: "TEXT", nullable: false),
                    GatewayOrderId = table.Column<string>(type: "TEXT", nullable: true),
                    GatewayPaymentId = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    StudentFeedback = table.Column<string>(type: "TEXT", nullable: true),
                    StudentRating = table.Column<int>(type: "INTEGER", nullable: true),
                    ConvertedToEnrollment = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnrollmentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrialSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrialSessions_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TrialSessions_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrialSessions_Users_TutorId",
                        column: x => x.TutorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseEnrollments_CourseId",
                table: "CourseEnrollments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEnrollments_StudentId",
                table: "CourseEnrollments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_SubjectId",
                table: "Courses",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_TutorId",
                table: "Courses",
                column: "TutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_TutorProfileId",
                table: "Courses",
                column: "TutorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSessions_CourseId",
                table: "CourseSessions",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSessions_TutorId",
                table: "CourseSessions",
                column: "TutorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrialSessions_CourseId",
                table: "TrialSessions",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_TrialSessions_StudentId",
                table: "TrialSessions",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrialSessions_TutorId",
                table: "TrialSessions",
                column: "TutorId");

            migrationBuilder.CreateIndex(
                name: "IX_TutorSubjectRates_SubjectId",
                table: "TutorSubjectRates",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TutorSubjectRates_TutorId",
                table: "TutorSubjectRates",
                column: "TutorId");

            migrationBuilder.CreateIndex(
                name: "IX_TutorSubjectRates_TutorProfileId",
                table: "TutorSubjectRates",
                column: "TutorProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseEnrollments");

            migrationBuilder.DropTable(
                name: "CourseSessions");

            migrationBuilder.DropTable(
                name: "TrialSessions");

            migrationBuilder.DropTable(
                name: "TutorSubjectRates");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropColumn(
                name: "AgeGroups",
                table: "TutorProfiles");

            migrationBuilder.DropColumn(
                name: "AutoPayoutMinimumAmount",
                table: "TutorProfiles");

            migrationBuilder.DropColumn(
                name: "AutoPayoutSchedule",
                table: "TutorProfiles");

            migrationBuilder.DropColumn(
                name: "TeachingStyles",
                table: "TutorProfiles");

            migrationBuilder.DropColumn(
                name: "TrialAvailable",
                table: "TutorProfiles");

            migrationBuilder.DropColumn(
                name: "TrialDurationMinutes",
                table: "TutorProfiles");

            migrationBuilder.DropColumn(
                name: "TrialPrice",
                table: "TutorProfiles");
        }
    }
}
