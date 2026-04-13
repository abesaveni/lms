using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiveExpert.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePaymentsAndBonusPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Users_UserId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_SessionBookings_CreditTransactions_CreditTransactionId",
                table: "SessionBookings");

            migrationBuilder.DropTable(
                name: "CourseEnrollments");

            migrationBuilder.DropTable(
                name: "Credits");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "QuizAttempts");

            migrationBuilder.DropTable(
                name: "QuizQuestions");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "CreditTransactions");

            migrationBuilder.DropTable(
                name: "Quizzes");

            migrationBuilder.DropColumn(
                name: "Credits",
                table: "WithdrawalRequests");

            migrationBuilder.DropColumn(
                name: "IsSubscribed",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SubscriptionExpiryDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Credits",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "PaymentType",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "CreditsCharged",
                table: "SessionBookings",
                newName: "TotalAmount");

            migrationBuilder.RenameColumn(
                name: "CreditTransactionId",
                table: "SessionBookings",
                newName: "PaymentId");

            migrationBuilder.RenameIndex(
                name: "IX_SessionBookings_CreditTransactionId",
                table: "SessionBookings",
                newName: "IX_SessionBookings_PaymentId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Payments",
                newName: "TutorId");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Payments",
                newName: "TotalAmount");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_UserId",
                table: "Payments",
                newName: "IX_Payments_TutorId");

            migrationBuilder.AddColumn<decimal>(
                name: "BasePrice",
                table: "Sessions",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PricingType",
                table: "Sessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseAmount",
                table: "SessionBookings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "HoursBooked",
                table: "SessionBookings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PlatformFee",
                table: "SessionBookings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseAmount",
                table: "Payments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PlatformFee",
                table: "Payments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "SessionId",
                table: "Payments",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "Payments",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "BonusPoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Points = table.Column<int>(type: "INTEGER", nullable: false),
                    Reason = table.Column<int>(type: "INTEGER", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonusPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BonusPoints_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    StudentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    LastActionById = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastActionAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatRequests_Users_LastActionById",
                        column: x => x.LastActionById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChatRequests_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatRequests_Users_TutorId",
                        column: x => x.TutorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TutorFollowers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StudentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorFollowers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TutorFollowers_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TutorFollowers_Users_TutorId",
                        column: x => x.TutorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserNotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    EmailEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    WhatsAppEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    InAppEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotificationPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserNotificationPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SessionId",
                table: "Payments",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StudentId",
                table: "Payments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_BonusPoints_UserId",
                table: "BonusPoints",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRequests_LastActionById",
                table: "ChatRequests",
                column: "LastActionById");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRequests_StudentId",
                table: "ChatRequests",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRequests_TutorId",
                table: "ChatRequests",
                column: "TutorId");

            migrationBuilder.CreateIndex(
                name: "IX_TutorFollowers_StudentId",
                table: "TutorFollowers",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_TutorFollowers_TutorId",
                table: "TutorFollowers",
                column: "TutorId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotificationPreferences_UserId",
                table: "UserNotificationPreferences",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Sessions_SessionId",
                table: "Payments",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Users_StudentId",
                table: "Payments",
                column: "StudentId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Users_TutorId",
                table: "Payments",
                column: "TutorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SessionBookings_Payments_PaymentId",
                table: "SessionBookings",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Sessions_SessionId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Users_StudentId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Users_TutorId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_SessionBookings_Payments_PaymentId",
                table: "SessionBookings");

            migrationBuilder.DropTable(
                name: "BonusPoints");

            migrationBuilder.DropTable(
                name: "ChatRequests");

            migrationBuilder.DropTable(
                name: "TutorFollowers");

            migrationBuilder.DropTable(
                name: "UserNotificationPreferences");

            migrationBuilder.DropIndex(
                name: "IX_Payments_SessionId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_StudentId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "BasePrice",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "PricingType",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "BaseAmount",
                table: "SessionBookings");

            migrationBuilder.DropColumn(
                name: "HoursBooked",
                table: "SessionBookings");

            migrationBuilder.DropColumn(
                name: "PlatformFee",
                table: "SessionBookings");

            migrationBuilder.DropColumn(
                name: "BaseAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PlatformFee",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "SessionBookings",
                newName: "CreditsCharged");

            migrationBuilder.RenameColumn(
                name: "PaymentId",
                table: "SessionBookings",
                newName: "CreditTransactionId");

            migrationBuilder.RenameIndex(
                name: "IX_SessionBookings_PaymentId",
                table: "SessionBookings",
                newName: "IX_SessionBookings_CreditTransactionId");

            migrationBuilder.RenameColumn(
                name: "TutorId",
                table: "Payments",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "Payments",
                newName: "Amount");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_TutorId",
                table: "Payments",
                newName: "IX_Payments_UserId");

            migrationBuilder.AddColumn<decimal>(
                name: "Credits",
                table: "WithdrawalRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsSubscribed",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionExpiryDate",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Credits",
                table: "Sessions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PaymentType",
                table: "Payments",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SubjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Credits = table.Column<decimal>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsPublished = table.Column<bool>(type: "INTEGER", nullable: false),
                    Level = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TotalSessions = table.Column<int>(type: "INTEGER", nullable: false),
                    TutorProfileId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Courses_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "Credits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AvailableCredits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReservedCredits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalEarned = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalSpent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalWithdrawn = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Credits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Credits_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreditTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalanceBefore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReferenceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ReferenceType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TransactionType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditTransactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PaymentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Credits = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    InvoiceNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    InvoiceType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "CreditPurchase"),
                    IssuedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PaymentGateway = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Razorpay"),
                    PaymentRef = table.Column<string>(type: "TEXT", nullable: true),
                    PdfUrl = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Invoices_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Quizzes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SubjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Duration = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    PassingScore = table.Column<decimal>(type: "TEXT", nullable: false),
                    QuizType = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetUsername = table.Column<string>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    TutorProfileId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quizzes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quizzes_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Quizzes_TutorProfiles_TutorProfileId",
                        column: x => x.TutorProfileId,
                        principalTable: "TutorProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Quizzes_Users_TutorId",
                        column: x => x.TutorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PaymentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    AutoRenew = table.Column<bool>(type: "INTEGER", nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Duration = table.Column<int>(type: "INTEGER", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Plan = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    SubscriptionType = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Subscriptions_Users_UserId",
                        column: x => x.UserId,
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
                    CreditTransactionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    StudentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CertificateUrl = table.Column<string>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletionPercentage = table.Column<decimal>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EnrolledAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastAccessedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "TEXT", nullable: true),
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
                        name: "FK_CourseEnrollments_CreditTransactions_CreditTransactionId",
                        column: x => x.CreditTransactionId,
                        principalTable: "CreditTransactions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CourseEnrollments_StudentProfiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalTable: "StudentProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CourseEnrollments_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuizId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StudentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Answers = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EarnedPoints = table.Column<int>(type: "INTEGER", nullable: false),
                    Passed = table.Column<bool>(type: "INTEGER", nullable: false),
                    Score = table.Column<decimal>(type: "TEXT", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TotalPoints = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizAttempts_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizAttempts_StudentProfiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalTable: "StudentProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuizAttempts_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuizId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CorrectAnswer = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Options = table.Column<string>(type: "TEXT", nullable: true),
                    OrderIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    Points = table.Column<int>(type: "INTEGER", nullable: false),
                    Question = table.Column<string>(type: "TEXT", nullable: false),
                    QuestionType = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizQuestions_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseEnrollments_CourseId",
                table: "CourseEnrollments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEnrollments_CreditTransactionId",
                table: "CourseEnrollments",
                column: "CreditTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEnrollments_StudentId",
                table: "CourseEnrollments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEnrollments_StudentProfileId",
                table: "CourseEnrollments",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_IsPublished",
                table: "Courses",
                column: "IsPublished");

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
                name: "IX_Credits_UserId",
                table: "Credits",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditTransactions_CreatedAt",
                table: "CreditTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CreditTransactions_Status",
                table: "CreditTransactions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CreditTransactions_TransactionType",
                table: "CreditTransactions",
                column: "TransactionType");

            migrationBuilder.CreateIndex(
                name: "IX_CreditTransactions_UserId",
                table: "CreditTransactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditTransactions_UserId_CreatedAt",
                table: "CreditTransactions",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNumber",
                table: "Invoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_PaymentId",
                table: "Invoices",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_PaymentRef",
                table: "Invoices",
                column: "PaymentRef");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_UserId",
                table: "Invoices",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_QuizId",
                table: "QuizAttempts",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_StudentId",
                table: "QuizAttempts",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_StudentProfileId",
                table: "QuizAttempts",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizQuestions_QuizId",
                table: "QuizQuestions",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_SubjectId",
                table: "Quizzes",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_TutorId",
                table: "Quizzes",
                column: "TutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_TutorProfileId",
                table: "Quizzes",
                column: "TutorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PaymentId",
                table: "Subscriptions",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscriptions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Users_UserId",
                table: "Payments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SessionBookings_CreditTransactions_CreditTransactionId",
                table: "SessionBookings",
                column: "CreditTransactionId",
                principalTable: "CreditTransactions",
                principalColumn: "Id");
        }
    }
}
