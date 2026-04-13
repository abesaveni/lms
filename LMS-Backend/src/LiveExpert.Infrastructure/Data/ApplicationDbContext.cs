using LiveExpert.Domain.Common;
using LiveExpert.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LiveExpert.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // User Management
    public DbSet<User> Users { get; set; }
    public DbSet<TutorProfile> TutorProfiles { get; set; }
    public DbSet<StudentProfile> StudentProfiles { get; set; }

    // Sessions
    public DbSet<Session> Sessions { get; set; }
    public DbSet<SessionBooking> SessionBookings { get; set; }
    public DbSet<SessionMeetLink> SessionMeetLinks { get; set; }

    // Messaging
    public DbSet<Message> Messages { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ChatRequest> ChatRequests { get; set; }

    // Notifications
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<NotificationChannel> NotificationChannels { get; set; }
    public DbSet<UserNotificationPreference> UserNotificationPreferences { get; set; }
    public DbSet<BonusPoint> BonusPoints { get; set; }

    // Payments
    public DbSet<Payment> Payments { get; set; }
    public DbSet<WithdrawalRequest> WithdrawalRequests { get; set; }
    public DbSet<BankAccount> BankAccounts { get; set; }

    // Reviews & Disputes
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Dispute> Disputes { get; set; }
    public DbSet<KYCDocument> KYCDocuments { get; set; }

    // Content
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<FAQ> FAQs { get; set; }
    public DbSet<ContactMessage> ContactMessages { get; set; }

    // Business
    public DbSet<ReferralProgram> ReferralPrograms { get; set; }
    public DbSet<Referral> Referrals { get; set; }
    
    // Google Integration
    public DbSet<TutorGoogleTokens> TutorGoogleTokens { get; set; }
    public DbSet<UserCalendarConnection> UserCalendarConnections { get; set; }
    
    // Tutor Verification
    public DbSet<TutorVerification> TutorVerifications { get; set; }

    // Followers
    public DbSet<TutorFollower> TutorFollowers { get; set; }

    // Admin
    public DbSet<SystemSetting> SystemSettings { get; set; }
    public DbSet<APIKey> APIKeys { get; set; }
    public DbSet<ApiSetting> ApiSettings { get; set; }
    public DbSet<AdminPermission> AdminPermissions { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<WhatsAppCampaign> WhatsAppCampaigns { get; set; }
    public DbSet<VirtualClassroomSession> VirtualClassroomSessions { get; set; }
    
    // Tutor Earnings & Payouts
    public DbSet<TutorEarning> TutorEarnings { get; set; }
    public DbSet<PayoutRequest> PayoutRequests { get; set; }
    
    // Consent Management
    public DbSet<CookieConsent> CookieConsents { get; set; }
    public DbSet<UserConsent> UserConsents { get; set; }

    // AI Responses
    public DbSet<AIResponse> AIResponses { get; set; }

    // Courses & Enrollment
    public DbSet<Course> Courses { get; set; }
    public DbSet<CourseSession> CourseSessions { get; set; }
    public DbSet<CourseEnrollment> CourseEnrollments { get; set; }
    public DbSet<TrialSession> TrialSessions { get; set; }
    public DbSet<TutorSubjectRate> TutorSubjectRates { get; set; }

    // Platform fee / subscription revenue
    public DbSet<PlatformFeePayment> PlatformFeePayments { get; set; }

    // Daily Challenges / Games
    public DbSet<DailyChallenge> DailyChallenges { get; set; }
    public DbSet<UserChallengeAttempt> UserChallengeAttempts { get; set; }
    public DbSet<UserChallengeStreak> UserChallengeStreaks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filters for soft delete
        modelBuilder.Entity<User>().HasQueryFilter(u => u.DeletedAt == null);

        ApplyUserNavigationQueryFilters(modelBuilder);
    }

    private static void ApplyUserNavigationQueryFilters(ModelBuilder modelBuilder)
    {
        var userType = typeof(User);
        var deletedAtProperty = nameof(User.DeletedAt);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType == userType || entityType.IsOwned())
            {
                continue;
            }

            var userNavigations = entityType.GetNavigations()
                .Where(n => n.TargetEntityType.ClrType == userType)
                .ToList();

            if (!userNavigations.Any())
            {
                continue;
            }

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            Expression? combined = null;

            foreach (var navigation in userNavigations)
            {
                var navigationExpression = Expression.Property(parameter, navigation.Name);
                var deletedAtExpression = Expression.Property(navigationExpression, deletedAtProperty);
                var isNotDeleted = Expression.Equal(deletedAtExpression, Expression.Constant(null, typeof(DateTime?)));

                Expression navigationFilter = isNotDeleted;
                if (!navigation.ForeignKey.IsRequired)
                {
                    var navigationIsNull = Expression.Equal(navigationExpression, Expression.Constant(null, userType));
                    navigationFilter = Expression.OrElse(navigationIsNull, isNotDeleted);
                }

                combined = combined == null ? navigationFilter : Expression.AndAlso(combined, navigationFilter);
            }

            if (combined == null)
            {
                continue;
            }

            var existingFilter = entityType.GetQueryFilter();
            if (existingFilter != null)
            {
                var replacedExistingFilter = new ReplaceParameterVisitor(existingFilter.Parameters[0], parameter)
                    .Visit(existingFilter.Body);
                combined = Expression.AndAlso(replacedExistingFilter!, combined);
            }

            var lambda = Expression.Lambda(combined, parameter);
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }

    private sealed class ReplaceParameterVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _source;
        private readonly ParameterExpression _target;

        public ReplaceParameterVisitor(ParameterExpression source, ParameterExpression target)
        {
            _source = source;
            _target = target;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _source ? _target : base.VisitParameter(node);
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auto-set timestamps
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
