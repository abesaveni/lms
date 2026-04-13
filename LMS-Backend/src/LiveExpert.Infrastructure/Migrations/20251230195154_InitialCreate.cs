using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiveExpert.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IconUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    WhatsAppNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsPhoneVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsWhatsAppVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    GoogleId = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSubscribed = table.Column<bool>(type: "INTEGER", nullable: false),
                    SubscriptionExpiryDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FAQs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Question = table.Column<string>(type: "TEXT", nullable: false),
                    Answer = table.Column<string>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FAQs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FAQs_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CategoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IconUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subjects_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdminPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AdminId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PermissionKey = table.Column<string>(type: "TEXT", nullable: false),
                    CanView = table.Column<bool>(type: "INTEGER", nullable: false),
                    CanCreate = table.Column<bool>(type: "INTEGER", nullable: false),
                    CanEdit = table.Column<bool>(type: "INTEGER", nullable: false),
                    CanDelete = table.Column<bool>(type: "INTEGER", nullable: false),
                    GrantedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    GrantedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminPermissions_Users_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdminPermissions_Users_GrantedByUserId",
                        column: x => x.GrantedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "APIKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ServiceName = table.Column<string>(type: "TEXT", nullable: false),
                    KeyName = table.Column<string>(type: "TEXT", nullable: false),
                    KeyValue = table.Column<string>(type: "TEXT", nullable: false),
                    Environment = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APIKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_APIKeys_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    EntityType = table.Column<string>(type: "TEXT", nullable: false),
                    EntityId = table.Column<Guid>(type: "TEXT", nullable: true),
                    OldValue = table.Column<string>(type: "TEXT", nullable: true),
                    NewValue = table.Column<string>(type: "TEXT", nullable: true),
                    IpAddress = table.Column<string>(type: "TEXT", nullable: true),
                    UserAgent = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BankAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountHolderName = table.Column<string>(type: "TEXT", nullable: false),
                    AccountNumber = table.Column<string>(type: "TEXT", nullable: false),
                    BankName = table.Column<string>(type: "TEXT", nullable: false),
                    IFSCCode = table.Column<string>(type: "TEXT", nullable: false),
                    BranchName = table.Column<string>(type: "TEXT", nullable: true),
                    AccountType = table.Column<int>(type: "INTEGER", nullable: false),
                    IsVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankAccounts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Blogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "TEXT", nullable: true),
                    AuthorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Tags = table.Column<string>(type: "TEXT", nullable: true),
                    ViewCount = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPublished = table.Column<bool>(type: "INTEGER", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Blogs_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Blogs_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    User1Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    User2Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LastMessageId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastMessageAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    User1UnreadCount = table.Column<int>(type: "INTEGER", nullable: false),
                    User2UnreadCount = table.Column<int>(type: "INTEGER", nullable: false),
                    User1BlockedUser2 = table.Column<bool>(type: "INTEGER", nullable: false),
                    User2BlockedUser1 = table.Column<bool>(type: "INTEGER", nullable: false),
                    User1NotificationsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    User2NotificationsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conversations_Users_User1Id",
                        column: x => x.User1Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Conversations_Users_User2Id",
                        column: x => x.User2Id,
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
                    ReservedCredits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalEarned = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalSpent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalWithdrawn = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Version = table.Column<byte[]>(type: "BLOB", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
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
                    TransactionType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalanceBefore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ReferenceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ReferenceType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
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
                name: "Disputes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RaisedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    DisputeType = table.Column<int>(type: "INTEGER", nullable: false),
                    RelatedToId = table.Column<Guid>(type: "TEXT", nullable: true),
                    RelatedToType = table.Column<string>(type: "TEXT", nullable: true),
                    Subject = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    AssignedTo = table.Column<Guid>(type: "TEXT", nullable: true),
                    Resolution = table.Column<string>(type: "TEXT", nullable: true),
                    AttachmentUrls = table.Column<string>(type: "TEXT", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RaisedByUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disputes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Disputes_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Disputes_Users_RaisedByUserId",
                        column: x => x.RaisedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KYCDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DocumentType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DocumentNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DocumentUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    FrontImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    BackImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    VerificationStatus = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    VerifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KYCDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KYCDocuments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KYCDocuments_Users_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    NotificationType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ActionUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Priority = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    IconUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PaymentType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PaymentGateway = table.Column<string>(type: "TEXT", nullable: false),
                    GatewayOrderId = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    GatewayPaymentId = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    GatewaySignature = table.Column<string>(type: "TEXT", nullable: true),
                    PaymentMethod = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    FailureReason = table.Column<string>(type: "TEXT", nullable: true),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReferralPrograms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReferrerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReferredUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReferralCode = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    RewardCredits = table.Column<decimal>(type: "TEXT", nullable: false),
                    RewardedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferralPrograms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReferralPrograms_Users_ReferredUserId",
                        column: x => x.ReferredUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReferralPrograms_Users_ReferrerId",
                        column: x => x.ReferrerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LearningGoals = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    PreferredSubjects = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsCalendarConnected = table.Column<bool>(type: "INTEGER", nullable: false),
                    CalendarProvider = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CalendarAccessToken = table.Column<string>(type: "TEXT", nullable: true),
                    CalendarRefreshToken = table.Column<string>(type: "TEXT", nullable: true),
                    ReferralCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ReferredBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    ReferredByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentProfiles_Users_ReferredByUserId",
                        column: x => x.ReferredByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SettingKey = table.Column<string>(type: "TEXT", nullable: false),
                    SettingValue = table.Column<string>(type: "TEXT", nullable: false),
                    DataType = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsEncrypted = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemSettings_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TutorProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Bio = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Headline = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    YearsOfExperience = table.Column<int>(type: "INTEGER", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VerificationStatus = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    VerifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ResumeUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    VideoIntroUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    AverageRating = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    TotalReviews = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalSessions = table.Column<int>(type: "INTEGER", nullable: false),
                    CompletionRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ResponseTime = table.Column<int>(type: "INTEGER", nullable: false),
                    IsCalendarConnected = table.Column<bool>(type: "INTEGER", nullable: false),
                    CalendarProvider = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CalendarAccessToken = table.Column<string>(type: "TEXT", nullable: true),
                    CalendarRefreshToken = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TutorProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WhatsAppCampaigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    TargetAudience = table.Column<int>(type: "INTEGER", nullable: false),
                    MessageTemplate = table.Column<string>(type: "TEXT", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalRecipients = table.Column<int>(type: "INTEGER", nullable: false),
                    SentCount = table.Column<int>(type: "INTEGER", nullable: false),
                    DeliveredCount = table.Column<int>(type: "INTEGER", nullable: false),
                    FailedCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhatsAppCampaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WhatsAppCampaigns_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WithdrawalRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Credits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    BankAccountId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ProcessedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    TransactionId = table.Column<string>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WithdrawalRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WithdrawalRequests_BankAccounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WithdrawalRequests_Users_ProcessedBy",
                        column: x => x.ProcessedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WithdrawalRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SenderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReceiverId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConversationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MessageType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    FileUrl = table.Column<string>(type: "TEXT", nullable: true),
                    FileName = table.Column<string>(type: "TEXT", nullable: true),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: true),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDelivered = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    ReplyToMessageId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Messages_Messages_ReplyToMessageId",
                        column: x => x.ReplyToMessageId,
                        principalTable: "Messages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Messages_Users_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotificationChannels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    NotificationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChannelType = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ExternalId = table.Column<string>(type: "TEXT", nullable: true),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    SentAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationChannels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationChannels_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    InvoiceType = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Tax = table.Column<decimal>(type: "TEXT", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    PaymentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PdfUrl = table.Column<string>(type: "TEXT", nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Invoices_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SubscriptionType = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    PaymentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AutoRenew = table.Column<bool>(type: "INTEGER", nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Plan = table.Column<int>(type: "INTEGER", nullable: false),
                    Duration = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
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
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    SubjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TotalSessions = table.Column<int>(type: "INTEGER", nullable: false),
                    Credits = table.Column<decimal>(type: "TEXT", nullable: false),
                    Level = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsPublished = table.Column<bool>(type: "INTEGER", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
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
                name: "Quizzes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    SubjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuizType = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetUsername = table.Column<string>(type: "TEXT", nullable: true),
                    Duration = table.Column<int>(type: "INTEGER", nullable: false),
                    PassingScore = table.Column<decimal>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    TutorProfileId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
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
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SessionType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    SubjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Duration = table.Column<int>(type: "INTEGER", nullable: false),
                    Credits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxStudents = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentStudents = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    MeetingLink = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    RecordingUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsRecorded = table.Column<bool>(type: "INTEGER", nullable: false),
                    CalendarEventId = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TutorProfileId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sessions_TutorProfiles_TutorProfileId",
                        column: x => x.TutorProfileId,
                        principalTable: "TutorProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Sessions_Users_TutorId",
                        column: x => x.TutorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseEnrollments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CourseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StudentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EnrolledAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletionPercentage = table.Column<decimal>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    LastAccessedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreditTransactionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    StudentProfileId = table.Column<Guid>(type: "TEXT", nullable: true),
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
                    Score = table.Column<decimal>(type: "TEXT", nullable: false),
                    TotalPoints = table.Column<int>(type: "INTEGER", nullable: false),
                    EarnedPoints = table.Column<int>(type: "INTEGER", nullable: false),
                    Passed = table.Column<bool>(type: "INTEGER", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Answers = table.Column<string>(type: "TEXT", nullable: true),
                    StudentProfileId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
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
                    Question = table.Column<string>(type: "TEXT", nullable: false),
                    QuestionType = table.Column<int>(type: "INTEGER", nullable: false),
                    Options = table.Column<string>(type: "TEXT", nullable: true),
                    CorrectAnswer = table.Column<string>(type: "TEXT", nullable: false),
                    Points = table.Column<int>(type: "INTEGER", nullable: false),
                    OrderIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StudentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Rating = table.Column<int>(type: "INTEGER", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    IsPublic = table.Column<bool>(type: "INTEGER", nullable: false),
                    TutorResponse = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    RespondedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId1 = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_Users_TutorId",
                        column: x => x.TutorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reviews_Users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SessionBookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StudentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BookingStatus = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CreditsCharged = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AttendanceMarked = table.Column<bool>(type: "INTEGER", nullable: false),
                    Attended = table.Column<bool>(type: "INTEGER", nullable: false),
                    AttendedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    JoinedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LeftAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SpecialInstructions = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreditTransactionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CancellationReason = table.Column<string>(type: "TEXT", nullable: true),
                    RefundAmount = table.Column<decimal>(type: "TEXT", nullable: true),
                    RefundProcessedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionBookings_CreditTransactions_CreditTransactionId",
                        column: x => x.CreditTransactionId,
                        principalTable: "CreditTransactions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SessionBookings_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionBookings_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminPermissions_AdminId",
                table: "AdminPermissions",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminPermissions_GrantedByUserId",
                table: "AdminPermissions",
                column: "GrantedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_APIKeys_UpdatedByUserId",
                table: "APIKeys",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_UserId",
                table: "BankAccounts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Blogs_AuthorId",
                table: "Blogs",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Blogs_CategoryId",
                table: "Blogs",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_User1Id",
                table: "Conversations",
                column: "User1Id");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_User1Id_User2Id",
                table: "Conversations",
                columns: new[] { "User1Id", "User2Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_User2Id",
                table: "Conversations",
                column: "User2Id");

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
                name: "IX_Disputes_AssignedToUserId",
                table: "Disputes",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Disputes_RaisedByUserId",
                table: "Disputes",
                column: "RaisedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FAQs_CategoryId",
                table: "FAQs",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_PaymentId",
                table: "Invoices",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_UserId",
                table: "Invoices",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_KYCDocuments_UserId",
                table: "KYCDocuments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_KYCDocuments_VerificationStatus",
                table: "KYCDocuments",
                column: "VerificationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_KYCDocuments_VerifiedByUserId",
                table: "KYCDocuments",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationId",
                table: "Messages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_CreatedAt",
                table: "Messages",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ReceiverId",
                table: "Messages",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ReplyToMessageId",
                table: "Messages",
                column: "ReplyToMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId",
                table: "Messages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationChannels_NotificationId",
                table: "NotificationChannels",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsRead",
                table: "Notifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_GatewayPaymentId",
                table: "Payments",
                column: "GatewayPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status",
                table: "Payments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserId",
                table: "Payments",
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
                name: "IX_ReferralPrograms_ReferredUserId",
                table: "ReferralPrograms",
                column: "ReferredUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralPrograms_ReferrerId",
                table: "ReferralPrograms",
                column: "ReferrerId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_Rating",
                table: "Reviews",
                column: "Rating");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_SessionId",
                table: "Reviews",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_StudentId",
                table: "Reviews",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_TutorId",
                table: "Reviews",
                column: "TutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId1",
                table: "Reviews",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_SessionBookings_BookingStatus",
                table: "SessionBookings",
                column: "BookingStatus");

            migrationBuilder.CreateIndex(
                name: "IX_SessionBookings_CreditTransactionId",
                table: "SessionBookings",
                column: "CreditTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionBookings_SessionId",
                table: "SessionBookings",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionBookings_SessionId_StudentId",
                table: "SessionBookings",
                columns: new[] { "SessionId", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_SessionBookings_StudentId",
                table: "SessionBookings",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_ScheduledAt",
                table: "Sessions",
                column: "ScheduledAt");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_Status",
                table: "Sessions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_SubjectId",
                table: "Sessions",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_TutorId",
                table: "Sessions",
                column: "TutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_TutorId_ScheduledAt",
                table: "Sessions",
                columns: new[] { "TutorId", "ScheduledAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_TutorProfileId",
                table: "Sessions",
                column: "TutorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_ReferralCode",
                table: "StudentProfiles",
                column: "ReferralCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_ReferredByUserId",
                table: "StudentProfiles",
                column: "ReferredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_UserId",
                table: "StudentProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_CategoryId",
                table: "Subjects",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Name",
                table: "Subjects",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PaymentId",
                table: "Subscriptions",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_UpdatedByUserId",
                table: "SystemSettings",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TutorProfiles_AverageRating",
                table: "TutorProfiles",
                column: "AverageRating");

            migrationBuilder.CreateIndex(
                name: "IX_TutorProfiles_UserId",
                table: "TutorProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TutorProfiles_VerificationStatus",
                table: "TutorProfiles",
                column: "VerificationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_GoogleId",
                table: "Users",
                column: "GoogleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PhoneNumber",
                table: "Users",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppCampaigns_CreatorId",
                table: "WhatsAppCampaigns",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequests_BankAccountId",
                table: "WithdrawalRequests",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequests_ProcessedBy",
                table: "WithdrawalRequests",
                column: "ProcessedBy");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequests_Status",
                table: "WithdrawalRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequests_UserId",
                table: "WithdrawalRequests",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminPermissions");

            migrationBuilder.DropTable(
                name: "APIKeys");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Blogs");

            migrationBuilder.DropTable(
                name: "CourseEnrollments");

            migrationBuilder.DropTable(
                name: "Credits");

            migrationBuilder.DropTable(
                name: "Disputes");

            migrationBuilder.DropTable(
                name: "FAQs");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "KYCDocuments");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "NotificationChannels");

            migrationBuilder.DropTable(
                name: "QuizAttempts");

            migrationBuilder.DropTable(
                name: "QuizQuestions");

            migrationBuilder.DropTable(
                name: "ReferralPrograms");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "SessionBookings");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "WhatsAppCampaigns");

            migrationBuilder.DropTable(
                name: "WithdrawalRequests");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "StudentProfiles");

            migrationBuilder.DropTable(
                name: "Quizzes");

            migrationBuilder.DropTable(
                name: "CreditTransactions");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "BankAccounts");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropTable(
                name: "TutorProfiles");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
