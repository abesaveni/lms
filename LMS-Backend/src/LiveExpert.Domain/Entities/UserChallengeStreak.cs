namespace LiveExpert.Domain.Entities;

/// <summary>
/// Tracks a student's daily challenge streak — one row per user.
/// </summary>
public class UserChallengeStreak
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>Consecutive days completed (resets if a day is missed).</summary>
    public int CurrentStreak { get; set; }

    /// <summary>All-time best streak.</summary>
    public int LongestStreak { get; set; }

    /// <summary>UTC date of the last completed challenge (date portion only).</summary>
    public DateTime? LastCompletedDate { get; set; }

    /// <summary>Lifetime total completed challenges.</summary>
    public int TotalCompleted { get; set; }

    /// <summary>Lifetime total XP earned from challenges.</summary>
    public int TotalXpEarned { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
