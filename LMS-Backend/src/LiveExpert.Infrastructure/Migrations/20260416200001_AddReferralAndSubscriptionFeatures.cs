using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiveExpert.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReferralAndSubscriptionFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── ReferralProgram: expiry window + tutor referral flag ─────────
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "ReferralPrograms",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTutorReferral",
                table: "ReferralPrograms",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            // ── TutorProfile: tutor referral code ────────────────────────────
            migrationBuilder.AddColumn<string>(
                name: "TutorReferralCode",
                table: "TutorProfiles",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            // ── Session: subscription gating ─────────────────────────────────
            migrationBuilder.AddColumn<bool>(
                name: "RequiresSubscription",
                table: "Sessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            // ── SubscriptionPlan: sessions limit ─────────────────────────────
            migrationBuilder.AddColumn<int>(
                name: "SessionsLimit",
                table: "SubscriptionPlans",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            // ── StudentSubscription: auto-renewal + usage + cancellation retention ──
            migrationBuilder.AddColumn<int>(
                name: "SessionsUsed",
                table: "StudentSubscriptions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "AutoRenew",
                table: "StudentSubscriptions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RenewalReminderSentAt",
                table: "StudentSubscriptions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "StudentSubscriptions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RetentionDiscountOffered",
                table: "StudentSubscriptions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "RetentionDiscountPercent",
                table: "StudentSubscriptions",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionOfferExpiry",
                table: "StudentSubscriptions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PendingCancellation",
                table: "StudentSubscriptions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ExpiresAt", table: "ReferralPrograms");
            migrationBuilder.DropColumn(name: "IsTutorReferral", table: "ReferralPrograms");
            migrationBuilder.DropColumn(name: "TutorReferralCode", table: "TutorProfiles");
            migrationBuilder.DropColumn(name: "RequiresSubscription", table: "Sessions");
            migrationBuilder.DropColumn(name: "SessionsLimit", table: "SubscriptionPlans");
            migrationBuilder.DropColumn(name: "SessionsUsed", table: "StudentSubscriptions");
            migrationBuilder.DropColumn(name: "AutoRenew", table: "StudentSubscriptions");
            migrationBuilder.DropColumn(name: "RenewalReminderSentAt", table: "StudentSubscriptions");
            migrationBuilder.DropColumn(name: "CancellationReason", table: "StudentSubscriptions");
            migrationBuilder.DropColumn(name: "RetentionDiscountOffered", table: "StudentSubscriptions");
            migrationBuilder.DropColumn(name: "RetentionDiscountPercent", table: "StudentSubscriptions");
            migrationBuilder.DropColumn(name: "RetentionOfferExpiry", table: "StudentSubscriptions");
            migrationBuilder.DropColumn(name: "PendingCancellation", table: "StudentSubscriptions");
        }
    }
}
