using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

/// <summary>
/// One attempt by a user on a DailyChallenge (one per user per day).
/// </summary>
public class UserChallengeAttempt
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid ChallengeId { get; set; }
    public DailyChallenge Challenge { get; set; } = null!;

    /// <summary>User's submitted answer serialised as JSON.</summary>
    public string SubmittedAnswerJson { get; set; } = "{}";

    /// <summary>0–100 score calculated server-side.</summary>
    public int Score { get; set; }

    public ChallengeAttemptResult Result { get; set; }

    /// <summary>Seconds taken to complete (for future leaderboard use).</summary>
    public int TimeTakenSeconds { get; set; }

    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}
