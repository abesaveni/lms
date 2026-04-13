namespace LiveExpert.Domain.Enums;

public enum UserRole
{
    Student,
    Tutor,
    Admin
}

public enum VerificationStatus
{
    NotStarted,
    Pending,
    Approved,
    Rejected
}

public enum SessionType
{
    OneOnOne,
    Group
}

public enum SessionPricingType
{
    Fixed,
    Hourly
}

public enum PlatformFeeType
{
    Fixed,
    PerHour,
    Percentage
}

public enum SessionStatus
{
    Scheduled,
    Live,
    InProgress,
    Completed,
    Cancelled,
    NoShow
}

public enum BookingStatus
{
    Pending,
    Confirmed,
    Attended,
    Completed,
    NoShow,
    Cancelled,
    Refunded
}

public enum PaymentStatus
{
    Pending,
    Success,
    Failed,
    Refunded
}

public enum BonusPointReason
{
    Registration,
    Referral
}

public enum DisputeType
{
    Payment,
    SessionQuality,
    Behaviour,
    Technical,
    Other
}

public enum DisputeStatus
{
    Open,
    InProgress,
    Resolved,
    Closed,
    Escalated
}

public enum Priority
{
    Low,
    Normal,
    Medium,
    High,
    Critical
}

public enum NotificationType
{
    SessionBooked,
    SessionReminder,
    SessionCancelled,
    SessionCompleted,
    PaymentReceived,
    PaymentFailed,
    NewMessage,
    WithdrawalApproved,
    WithdrawalRejected,
    TutorVerified,
    TutorRejected,
    DisputeRaised,
    DisputeResolved,
    ReferralBonus,
    BonusPointsAdded,
    PayoutRequest
}

public enum NotificationChannel
{
    InApp,
    Email,
    WhatsApp,
    SMS
}

public enum NotificationChannelStatus
{
    Pending,
    Sent,
    Failed,
    Delivered,
    Read
}

public enum NotificationCategory
{
    SessionBooking,
    ChatRequests,
    EarningsPayouts,
    PointsBonuses,
    EngagementReminders,
    MarketingAnnouncements,
    AccountSecurity,
    Payment
}

public enum MessageType
{
    Text,
    Image,
    File,
    System
}

public enum ChatRequestStatus
{
    Pending,
    Accepted,
    Hold,
    Rejected
}

public enum WithdrawalStatus
{
    Pending,
    Approved,
    Processing,
    Completed,
    Rejected
}

public enum DocumentType
{
    Aadhaar,
    PAN,
    Passport,
    DrivingLicense
}

public enum AccountType
{
    Savings,
    Current
}

public enum CalendarProvider
{
    Google,
    Outlook
}

public enum CampaignStatus
{
    Draft,
    Scheduled,
    Sending,
    Sent,
    Failed
}

public enum TargetAudience
{
    AllUsers,
    Students,
    Tutors,
    Specific
}

public enum ContactMessageStatus
{
    New,
    InProgress,
    Responded,
    Closed
}

public enum ReferralStatus
{
    Pending,
    Completed,
    Rewarded
}

public enum ApiProvider
{
    GoogleOAuth,
    GoogleCalendar,
    WhatsApp,
    Email,
    Razorpay,
    Stripe,
    OpenAI,
    Other
}

public enum EarningStatus
{
    Pending,
    Available,
    Paid
}

public enum PayoutStatus
{
    Pending,
    Approved,
    Rejected,
    Paid
}

public enum CourseStatus
{
    Draft,
    Published,
    Paused,
    Archived
}

public enum CourseLevel
{
    Beginner,
    Intermediate,
    Advanced,
    AllLevels
}

public enum CourseDeliveryType
{
    LiveOneOnOne,
    LiveGroup,
    Recorded
}

public enum EnrollmentStatus
{
    Active,
    Completed,
    Cancelled,
    Expired,
    Refunded
}

public enum EnrollmentType
{
    Full,       // Full course bundle
    Partial     // N sessions purchased
}

public enum TrialSessionStatus
{
    Pending,
    Scheduled,
    Completed,
    Cancelled,
    NoShow,
    ConvertedToEnrollment
}

public enum AutoPayoutSchedule
{
    Disabled,
    Weekly,     // Every Monday
    BiWeekly,   // 1st and 15th
    Monthly     // 1st of month
}

public enum BillingTransactionType
{
    SessionBooking,
    CourseEnrollment,
    Subscription,
    TrialSession,
    Refund
}

public enum ChallengeType
{
    WordScramble = 1,   // Unscramble a word
    Quiz = 2,           // MCQ with timer
    MatchPairs = 3,     // Match concepts to definitions
    FillBlank = 4,      // Fill in the blank
    TrueFalse = 5,      // Rapid-fire true/false
    CodeBug = 6         // Find the bug in a code snippet
}

public enum ChallengeAttemptResult
{
    Perfect = 1,   // 100%
    Good = 2,      // ≥70%
    Partial = 3,   // ≥40%
    Failed = 4     // <40%
}

