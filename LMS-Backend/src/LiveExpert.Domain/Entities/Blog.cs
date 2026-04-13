using LiveExpert.Domain.Common;

namespace LiveExpert.Domain.Entities;

public class Blog : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? ThumbnailUrl { get; set; }
    public Guid AuthorId { get; set; }
    public Guid CategoryId { get; set; }
    public string? Tags { get; set; } // comma-separated
    public int ViewCount { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }

    // Navigation Properties
    public User Author { get; set; } = null!;
    public Category Category { get; set; } = null!;
}

public class FAQ : BaseEntity
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public Category Category { get; set; } = null!;
}

public class ReferralProgram : BaseEntity
{
    public Guid ReferrerId { get; set; }
    public Guid ReferredUserId { get; set; }
    public string ReferralCode { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Completed, Rewarded
    public decimal RewardCredits { get; set; }
    public decimal JoiningBonusAmount { get; set; }
    public DateTime? ReferralBonusPaidAt { get; set; }
    public DateTime? JoiningBonusPaidAt { get; set; }
    public DateTime? RewardedAt { get; set; }

    // Navigation Properties
    public User Referrer { get; set; } = null!;
    public User ReferredUser { get; set; } = null!;
}
