using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

public class User : AuditableEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? WhatsAppNumber { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsPhoneVerified { get; set; }
    public bool IsWhatsAppVerified { get; set; }
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiresAt { get; set; }
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? GoogleId { get; set; }
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? ProfileImageUrl { get; set; }
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? Bio { get; set; }
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public DateTime? DateOfBirth { get; set; }
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? Location { get; set; }
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? Language { get; set; }
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? Timezone { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    // Trial & Subscription
    public DateTime? TrialStartDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    public bool IsSubscribed { get; set; } = false;
    public DateTime? SubscribedUntil { get; set; }

    // Navigation Properties
    public TutorProfile? TutorProfile { get; set; }
    public StudentProfile? StudentProfile { get; set; }
    public ICollection<BonusPoint> BonusPoints { get; set; } = new List<BonusPoint>();
    public ICollection<Session> TutorSessions { get; set; } = new List<Session>();
    public ICollection<SessionBooking> StudentBookings { get; set; } = new List<SessionBooking>();
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    public ICollection<Review> GivenReviews { get; set; } = new List<Review>();
    public ICollection<Review> ReceivedReviews { get; set; } = new List<Review>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
    public ICollection<KYCDocument> KYCDocuments { get; set; } = new List<KYCDocument>();
    public ICollection<CookieConsent> CookieConsents { get; set; } = new List<CookieConsent>();
    public ICollection<UserConsent> UserConsents { get; set; } = new List<UserConsent>();
}
