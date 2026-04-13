using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiveExpert.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTutorProfileMissingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Payments_PaymentId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IssuedDate",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Tax",
                table: "Invoices");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "Invoices",
                newName: "IssuedAt");

            migrationBuilder.RenameColumn(
                name: "PaidDate",
                table: "Invoices",
                newName: "PaymentRef");

            migrationBuilder.AddColumn<string>(
                name: "GovtIdUrl",
                table: "TutorProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsProfileComplete",
                table: "TutorProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVisible",
                table: "TutorProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OnboardingStep",
                table: "TutorProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "VerificationId",
                table: "TutorProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GoogleCalendarEventId",
                table: "Sessions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceType",
                table: "Invoices",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "CreditPurchase",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<decimal>(
                name: "Credits",
                table: "Invoices",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PaymentGateway",
                table: "Invoices",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "Razorpay");

            migrationBuilder.CreateTable(
                name: "ApiSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Provider = table.Column<string>(type: "TEXT", nullable: false),
                    KeyName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    KeyValue = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    LastValidatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CookieConsents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Necessary = table.Column<bool>(type: "INTEGER", nullable: false),
                    Functional = table.Column<bool>(type: "INTEGER", nullable: false),
                    Analytics = table.Column<bool>(type: "INTEGER", nullable: false),
                    Marketing = table.Column<bool>(type: "INTEGER", nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", nullable: true),
                    UserAgent = table.Column<string>(type: "TEXT", nullable: true),
                    ConsentGivenAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ConsentUpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CookieConsents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CookieConsents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PayoutRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BankAccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ProcessedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    AdminNotes = table.Column<string>(type: "TEXT", nullable: true),
                    TransactionReference = table.Column<string>(type: "TEXT", nullable: true),
                    PaymentMethod = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Bank Transfer"),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayoutRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayoutRequests_BankAccounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayoutRequests_Users_ProcessedBy",
                        column: x => x.ProcessedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayoutRequests_Users_TutorId",
                        column: x => x.TutorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Referrals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReferrerUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReferredUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReferralCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    BonusCredits = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    RewardedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TriggerReferenceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TriggerActivityType = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Referrals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Referrals_Users_ReferredUserId",
                        column: x => x.ReferredUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Referrals_Users_ReferrerUserId",
                        column: x => x.ReferrerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SessionMeetLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MeetUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CalendarEventId = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    SessionStartedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SessionEndedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionMeetLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionMeetLinks_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TutorGoogleTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccessToken = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    RefreshToken = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    TokenExpiry = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GoogleEmail = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    LastRefreshedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorGoogleTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TutorGoogleTokens_Users_TutorId",
                        column: x => x.TutorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TutorVerifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    AdminNotes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    VerifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    GovtIdUrl = table.Column<string>(type: "TEXT", nullable: true),
                    IntroVideoUrl = table.Column<string>(type: "TEXT", nullable: true),
                    ResumeUrl = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TutorVerifications_Users_TutorId",
                        column: x => x.TutorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TutorVerifications_Users_VerifiedBy",
                        column: x => x.VerifiedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserCalendarConnections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Provider = table.Column<string>(type: "TEXT", nullable: false),
                    AccessToken = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    RefreshToken = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    TokenExpiry = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GoogleEmail = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    LastRefreshedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ConnectedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCalendarConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCalendarConnections_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserConsents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConsentType = table.Column<int>(type: "INTEGER", nullable: false),
                    Granted = table.Column<bool>(type: "INTEGER", nullable: false),
                    GrantedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IpAddress = table.Column<string>(type: "TEXT", nullable: true),
                    UserAgent = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserConsents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserConsents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TutorEarnings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SourceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    AvailableAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PayoutRequestId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CommissionPercentage = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    CommissionAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorEarnings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TutorEarnings_PayoutRequests_PayoutRequestId",
                        column: x => x.PayoutRequestId,
                        principalTable: "PayoutRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TutorEarnings_Users_TutorId",
                        column: x => x.TutorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TutorProfiles_VerificationId",
                table: "TutorProfiles",
                column: "VerificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNumber",
                table: "Invoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_PaymentRef",
                table: "Invoices",
                column: "PaymentRef");

            migrationBuilder.CreateIndex(
                name: "IX_ApiSettings_Provider_KeyName",
                table: "ApiSettings",
                columns: new[] { "Provider", "KeyName" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CookieConsents_UserId",
                table: "CookieConsents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutRequests_BankAccountId",
                table: "PayoutRequests",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutRequests_ProcessedBy",
                table: "PayoutRequests",
                column: "ProcessedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutRequests_RequestedAt",
                table: "PayoutRequests",
                column: "RequestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutRequests_Status",
                table: "PayoutRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutRequests_TutorId",
                table: "PayoutRequests",
                column: "TutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_ReferralCode",
                table: "Referrals",
                column: "ReferralCode");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_ReferredUserId",
                table: "Referrals",
                column: "ReferredUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_ReferrerUserId",
                table: "Referrals",
                column: "ReferrerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionMeetLinks_SessionId",
                table: "SessionMeetLinks",
                column: "SessionId",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_TutorEarnings_PayoutRequestId",
                table: "TutorEarnings",
                column: "PayoutRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_TutorEarnings_SourceType_SourceId",
                table: "TutorEarnings",
                columns: new[] { "SourceType", "SourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_TutorEarnings_Status",
                table: "TutorEarnings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TutorEarnings_TutorId",
                table: "TutorEarnings",
                column: "TutorId");

            migrationBuilder.CreateIndex(
                name: "IX_TutorGoogleTokens_TutorId",
                table: "TutorGoogleTokens",
                column: "TutorId",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_TutorVerifications_Status",
                table: "TutorVerifications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TutorVerifications_TutorId",
                table: "TutorVerifications",
                column: "TutorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TutorVerifications_VerifiedBy",
                table: "TutorVerifications",
                column: "VerifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserCalendarConnections_UserId",
                table: "UserCalendarConnections",
                column: "UserId",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_UserConsents_UserId",
                table: "UserConsents",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Payments_PaymentId",
                table: "Invoices",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TutorProfiles_TutorVerifications_VerificationId",
                table: "TutorProfiles",
                column: "VerificationId",
                principalTable: "TutorVerifications",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Payments_PaymentId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_TutorProfiles_TutorVerifications_VerificationId",
                table: "TutorProfiles");

            migrationBuilder.DropTable(
                name: "ApiSettings");

            migrationBuilder.DropTable(
                name: "CookieConsents");

            migrationBuilder.DropTable(
                name: "Referrals");

            migrationBuilder.DropTable(
                name: "SessionMeetLinks");

            migrationBuilder.DropTable(
                name: "TutorEarnings");

            migrationBuilder.DropTable(
                name: "TutorGoogleTokens");

            migrationBuilder.DropTable(
                name: "TutorVerifications");

            migrationBuilder.DropTable(
                name: "UserCalendarConnections");

            migrationBuilder.DropTable(
                name: "UserConsents");

            migrationBuilder.DropTable(
                name: "PayoutRequests");

            migrationBuilder.DropIndex(
                name: "IX_TutorProfiles_VerificationId",
                table: "TutorProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_InvoiceNumber",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_PaymentRef",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "GovtIdUrl",
                table: "TutorProfiles");

            migrationBuilder.DropColumn(
                name: "IsProfileComplete",
                table: "TutorProfiles");

            migrationBuilder.DropColumn(
                name: "IsVisible",
                table: "TutorProfiles");

            migrationBuilder.DropColumn(
                name: "OnboardingStep",
                table: "TutorProfiles");

            migrationBuilder.DropColumn(
                name: "VerificationId",
                table: "TutorProfiles");

            migrationBuilder.DropColumn(
                name: "GoogleCalendarEventId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "Credits",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PaymentGateway",
                table: "Invoices");

            migrationBuilder.RenameColumn(
                name: "PaymentRef",
                table: "Invoices",
                newName: "PaidDate");

            migrationBuilder.RenameColumn(
                name: "IssuedAt",
                table: "Invoices",
                newName: "TotalAmount");

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceType",
                table: "Invoices",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldDefaultValue: "CreditPurchase");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Invoices",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "IssuedDate",
                table: "Invoices",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Invoices",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Tax",
                table: "Invoices",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Payments_PaymentId",
                table: "Invoices",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id");
        }
    }
}
