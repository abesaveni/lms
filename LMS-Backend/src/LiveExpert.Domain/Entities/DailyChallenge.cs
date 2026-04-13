using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

/// <summary>
/// One puzzle/game per day. ContentJson and AnswerJson are type-specific JSON payloads.
/// </summary>
public class DailyChallenge
{
    public Guid Id { get; set; }

    /// <summary>UTC calendar date this challenge is active for (time portion ignored).</summary>
    public DateTime ChallengeDate { get; set; }

    public ChallengeType Type { get; set; }

    /// <summary>Display title shown to the student.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Short description / instructions.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Type-specific question payload serialised as JSON.</summary>
    public string ContentJson { get; set; } = "{}";

    /// <summary>Correct answer(s) serialised as JSON — never sent to client.</summary>
    public string AnswerJson { get; set; } = "{}";

    /// <summary>XP / bonus points awarded on completion.</summary>
    public int XpReward { get; set; } = 10;

    /// <summary>Difficulty label shown to the student.</summary>
    public string Difficulty { get; set; } = "Medium";

    /// <summary>Topic tag, e.g. "JavaScript", "OOP", "Algorithms".</summary>
    public string Tag { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserChallengeAttempt> Attempts { get; set; } = new List<UserChallengeAttempt>();
}
