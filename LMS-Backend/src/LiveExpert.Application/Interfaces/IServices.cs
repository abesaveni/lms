using LiveExpert.Domain.Common;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using System.Linq;
using System.Linq.Expressions;

namespace LiveExpert.Application.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    IQueryable<T> GetQueryable();
}

public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Username { get; }
    string? Email { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
}

public interface IDateTimeService
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
}

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task SendTemplateEmailAsync(string to, string templateName, object data);
    Task SendBulkEmailAsync(List<string> recipients, string subject, string body);
}

public interface ISMSService
{
    Task SendSMSAsync(string phoneNumber, string message);
    Task SendOTPAsync(string phoneNumber, string otp);
}


public interface IFileStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task<Stream> DownloadFileAsync(string fileUrl);
    Task DeleteFileAsync(string fileUrl);
    Task<string> GetSignedUrlAsync(string fileUrl, int expiryMinutes = 60);
}

public interface IPaymentService
{
    Task<(string orderId, string key)> CreateOrderAsync(decimal amount, string currency, Dictionary<string, string> metadata);
    Task<bool> VerifyPaymentSignatureAsync(string orderId, string paymentId, string signature);
    Task<string> InitiateRefundAsync(string paymentId, decimal amount);
    Task<string> InitiatePayoutAsync(string accountNumber, string ifsc, decimal amount, string purpose);
}

public interface ICalendarService
{
    Task<string> GetAuthorizationUrlAsync(string provider, string redirectUri);
    Task<(string accessToken, string refreshToken)> ExchangeCodeForTokensAsync(string code, string provider);
    Task<string> CreateEventAsync(string accessToken, string provider, CalendarEventDto eventDto);
    Task UpdateEventAsync(string accessToken, string provider, string eventId, CalendarEventDto eventDto);
    Task DeleteEventAsync(string accessToken, string provider, string eventId);
    Task<List<CalendarEventDto>> GetEventsAsync(string accessToken, string provider, DateTime startDate, DateTime endDate);
}

public class CalendarEventDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Location { get; set; }
    public List<string> Attendees { get; set; } = new();
}

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
}

public interface INotificationService
{
    Task SendNotificationAsync(Guid userId, string title, string message, NotificationType? notificationType = null, string? actionUrl = null, CancellationToken cancellationToken = default);
    Task SendBulkNotificationAsync(List<Guid> userIds, string title, string message);
    
    // Semantic Event-based notifications
    Task SendWelcomeMessageAsync(User user, CancellationToken cancellationToken = default);
    Task SendSessionScheduledAsync(User user, Session session, string sessionLink, string otherPartyName, CancellationToken cancellationToken = default);
    Task SendForgotPasswordEmailAsync(User user, string resetLink, int expiresMinutes, CancellationToken cancellationToken = default);
    Task SendSessionCancelledAsync(User user, string sessionTitle, DateTime sessionTime, string cancelledBy, CancellationToken cancellationToken = default);
    Task SendSessionReminderAsync(User user, string sessionTitle, DateTime sessionTime, string joinLink, CancellationToken cancellationToken = default);
    Task SendSessionFeedbackAsync(User user, string tutorName, string sessionTitle, string feedbackLink, CancellationToken cancellationToken = default);
    Task SendTutorProfileUnderReviewAsync(User user, CancellationToken cancellationToken = default);
    Task SendTutorVerifiedAsync(User user, CancellationToken cancellationToken = default);
    Task SendTutorRejectedAsync(User user, string reason, CancellationToken cancellationToken = default);
    Task SendTutorSubmissionToAdminAsync(User tutor, CancellationToken cancellationToken = default);
}

public interface INotificationPreferenceService
{
    Task<List<UserNotificationPreference>> GetOrCreateDefaultsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsChannelEnabledAsync(Guid userId, NotificationCategory category, LiveExpert.Domain.Enums.NotificationChannel channel, bool isTransactional, CancellationToken cancellationToken = default);
    Task UpdatePreferencesAsync(Guid userId, List<UserNotificationPreference> preferences, CancellationToken cancellationToken = default);
}

public interface INotificationDispatcher
{
    Task SendAsync(NotificationDispatchRequest request, CancellationToken cancellationToken = default);
}

public class NotificationDispatchRequest
{
    public Guid UserId { get; set; }
    public NotificationCategory Category { get; set; }
    public bool IsTransactional { get; set; } = true;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public string? EmailTo { get; set; }
    public string? EmailSubject { get; set; }
    public string? EmailBody { get; set; }
    public bool EmailIsHtml { get; set; } = false;
    public string? WhatsAppTo { get; set; }
    public string? WhatsAppMessage { get; set; }
    public string? WhatsAppTemplateName { get; set; }
    public List<string>? WhatsAppParameters { get; set; }
    public bool SendInApp { get; set; } = true;
    public bool SendEmail { get; set; } = true;
    public bool SendWhatsApp { get; set; } = true;
}

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
    string Hash(string text);
    bool VerifyHash(string text, string hash);
}

public interface IGoogleCalendarService
{
    Task<string> CreateMeetingLinkAsync(string title, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);
}

public interface IAPIKeyService
{
    /// <summary>
    /// Get API key value from database (with fallback to configuration)
    /// </summary>
    Task<string?> GetAPIKeyAsync(string serviceName, string keyName, string? defaultValue = null);
    
    /// <summary>
    /// Get all API keys for a service
    /// </summary>
    Task<Dictionary<string, string>> GetServiceAPIKeysAsync(string serviceName);
    
    /// <summary>
    /// Update or create an API key
    /// </summary>
    Task<bool> UpdateAPIKeyAsync(string serviceName, string keyName, string keyValue, Guid? updatedBy = null);
    
    /// <summary>
    /// Clear cache for a specific service/key
    /// </summary>
    void ClearCache(string? serviceName = null, string? keyName = null);
}
