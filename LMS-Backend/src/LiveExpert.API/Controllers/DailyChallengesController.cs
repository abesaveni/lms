using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using LiveExpert.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LiveExpert.API.Controllers;

/// <summary>
/// Daily puzzle/game challenges — paid students only.
/// Each day one challenge is active. Students earn XP and build streaks.
/// </summary>
[Authorize(Roles = "Student")]
[Route("api/challenges")]
[ApiController]
public class DailyChallengesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DailyChallengesController(
        ApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    // ──────────────────────────────────────────────────────────────────────
    // GET /api/challenges/today
    // Returns today's challenge (without AnswerJson).
    // Also returns whether the current user already completed it today.
    // ──────────────────────────────────────────────────────────────────────
    [HttpGet("today")]
    public async Task<IActionResult> GetToday()
    {
        var today = DateTime.UtcNow.Date;

        var challenge = await _context.DailyChallenges
            .FirstOrDefaultAsync(c => c.ChallengeDate == today);

        if (challenge == null)
            return NotFound(new { error = "No challenge scheduled for today." });

        var userId = _currentUser.UserId;
        UserChallengeAttempt? existing = null;
        if (userId.HasValue)
        {
            existing = await _context.UserChallengeAttempts
                .FirstOrDefaultAsync(a => a.UserId == userId.Value && a.ChallengeId == challenge.Id);
        }

        return Ok(new
        {
            challenge = MapChallenge(challenge),
            alreadyCompleted = existing != null,
            attempt = existing == null ? null : new
            {
                existing.Score,
                Result = existing.Result.ToString(),
                existing.TimeTakenSeconds,
                CompletedAt = existing.CompletedAt
            }
        });
    }

    // ──────────────────────────────────────────────────────────────────────
    // POST /api/challenges/{id}/submit
    // Body: { answerJson: string, timeTakenSeconds: int }
    // Returns: score, result, xpEarned, streakInfo
    // ──────────────────────────────────────────────────────────────────────
    [HttpPost("{id}/submit")]
    public async Task<IActionResult> Submit(Guid id, [FromBody] SubmitAnswerRequest request)
    {
        var userId = _currentUser.UserId;
        if (!userId.HasValue) return Unauthorized();

        var challenge = await _context.DailyChallenges.FindAsync(id);
        if (challenge == null) return NotFound();

        // One attempt per day per user
        var alreadyDone = await _context.UserChallengeAttempts
            .AnyAsync(a => a.UserId == userId.Value && a.ChallengeId == id);
        if (alreadyDone)
            return BadRequest(new { error = "You have already submitted today's challenge." });

        // Score the answer
        int score = ScoreAnswer(challenge, request.AnswerJson);
        var result = score switch
        {
            100 => ChallengeAttemptResult.Perfect,
            >= 70 => ChallengeAttemptResult.Good,
            >= 40 => ChallengeAttemptResult.Partial,
            _    => ChallengeAttemptResult.Failed
        };

        int xpEarned = score >= 40
            ? (int)Math.Round(challenge.XpReward * score / 100.0)
            : 0;

        var attempt = new UserChallengeAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId.Value,
            ChallengeId = id,
            SubmittedAnswerJson = request.AnswerJson,
            Score = score,
            Result = result,
            TimeTakenSeconds = request.TimeTakenSeconds,
            CompletedAt = DateTime.UtcNow
        };
        _context.UserChallengeAttempts.Add(attempt);

        // Update streak
        var streak = await _context.UserChallengeStreaks
            .FirstOrDefaultAsync(s => s.UserId == userId.Value);

        if (streak == null)
        {
            streak = new UserChallengeStreak
            {
                Id = Guid.NewGuid(),
                UserId = userId.Value
            };
            _context.UserChallengeStreaks.Add(streak);
        }

        var today = DateTime.UtcNow.Date;
        if (score >= 40) // partial counts toward streak
        {
            if (streak.LastCompletedDate.HasValue &&
                streak.LastCompletedDate.Value.Date == today.AddDays(-1))
            {
                streak.CurrentStreak++;
            }
            else if (streak.LastCompletedDate.HasValue &&
                     streak.LastCompletedDate.Value.Date == today)
            {
                // Already counted today — no change
            }
            else
            {
                streak.CurrentStreak = 1;
            }

            if (streak.CurrentStreak > streak.LongestStreak)
                streak.LongestStreak = streak.CurrentStreak;

            streak.LastCompletedDate = today;
            streak.TotalCompleted++;
            streak.TotalXpEarned += xpEarned;
        }
        streak.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            score,
            result = result.ToString(),
            xpEarned,
            correctAnswerJson = challenge.AnswerJson,
            streak = new
            {
                streak.CurrentStreak,
                streak.LongestStreak,
                streak.TotalCompleted,
                streak.TotalXpEarned
            }
        });
    }

    // ──────────────────────────────────────────────────────────────────────
    // GET /api/challenges/streak
    // ──────────────────────────────────────────────────────────────────────
    [HttpGet("streak")]
    public async Task<IActionResult> GetStreak()
    {
        var userId = _currentUser.UserId;
        if (!userId.HasValue) return Unauthorized();

        var streak = await _context.UserChallengeStreaks
            .FirstOrDefaultAsync(s => s.UserId == userId.Value);

        // Check if streak is broken (missed yesterday)
        if (streak != null &&
            streak.LastCompletedDate.HasValue &&
            streak.LastCompletedDate.Value.Date < DateTime.UtcNow.Date.AddDays(-1))
        {
            streak.CurrentStreak = 0;
            streak.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        // Did they complete today already?
        var today = DateTime.UtcNow.Date;
        var todayChallenge = await _context.DailyChallenges
            .FirstOrDefaultAsync(c => c.ChallengeDate == today);
        bool completedToday = false;
        if (todayChallenge != null && userId.HasValue)
        {
            completedToday = await _context.UserChallengeAttempts
                .AnyAsync(a => a.UserId == userId.Value && a.ChallengeId == todayChallenge.Id);
        }

        return Ok(new
        {
            currentStreak  = streak?.CurrentStreak ?? 0,
            longestStreak  = streak?.LongestStreak ?? 0,
            totalCompleted = streak?.TotalCompleted ?? 0,
            totalXpEarned  = streak?.TotalXpEarned ?? 0,
            lastCompletedDate = streak?.LastCompletedDate,
            completedToday
        });
    }

    // ──────────────────────────────────────────────────────────────────────
    // GET /api/challenges/history?page=1&pageSize=10
    // ──────────────────────────────────────────────────────────────────────
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = _currentUser.UserId;
        if (!userId.HasValue) return Unauthorized();

        var total = await _context.UserChallengeAttempts
            .CountAsync(a => a.UserId == userId.Value);

        var attempts = await _context.UserChallengeAttempts
            .Include(a => a.Challenge)
            .Where(a => a.UserId == userId.Value)
            .OrderByDescending(a => a.CompletedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new
            {
                a.Id,
                a.Score,
                Result = a.Result.ToString(),
                a.TimeTakenSeconds,
                a.CompletedAt,
                Challenge = new
                {
                    a.Challenge.Id,
                    a.Challenge.Title,
                    a.Challenge.Tag,
                    a.Challenge.Difficulty,
                    Type = a.Challenge.Type.ToString(),
                    a.Challenge.XpReward,
                    ChallengeDate = a.Challenge.ChallengeDate
                }
            })
            .ToListAsync();

        return Ok(new { data = attempts, total, page, pageSize });
    }

    // ──────────────────────────────────────────────────────────────────────
    // GET /api/challenges/upcoming?days=7
    // Returns upcoming challenges (titles + types only, no content)
    // ──────────────────────────────────────────────────────────────────────
    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming([FromQuery] int days = 7)
    {
        var today = DateTime.UtcNow.Date;
        var upcoming = await _context.DailyChallenges
            .Where(c => c.ChallengeDate >= today && c.ChallengeDate <= today.AddDays(days))
            .OrderBy(c => c.ChallengeDate)
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.Tag,
                c.Difficulty,
                Type = c.Type.ToString(),
                c.XpReward,
                ChallengeDate = c.ChallengeDate,
                IsToday = c.ChallengeDate == today
            })
            .ToListAsync();

        return Ok(upcoming);
    }

    // ──────────────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────────────

    private static object MapChallenge(DailyChallenge c) => new
    {
        c.Id,
        c.Title,
        c.Description,
        Type = c.Type.ToString(),
        c.ContentJson,
        c.XpReward,
        c.Difficulty,
        c.Tag,
        ChallengeDate = c.ChallengeDate
    };

    /// <summary>
    /// Server-side scoring: returns 0–100.
    /// </summary>
    private static int ScoreAnswer(DailyChallenge challenge, string submittedJson)
    {
        try
        {
            var correct = JsonDocument.Parse(challenge.AnswerJson).RootElement;
            var submitted = JsonDocument.Parse(submittedJson).RootElement;

            return challenge.Type switch
            {
                ChallengeType.WordScramble => ScoreWord(correct, submitted),
                ChallengeType.Quiz        => ScoreMultipleChoice(correct, submitted, "correct"),
                ChallengeType.FillBlank   => ScoreFillBlank(correct, submitted),
                ChallengeType.CodeBug     => ScoreMultipleChoice(correct, submitted, "correct"),
                ChallengeType.MatchPairs  => ScoreMatchPairs(correct, submitted),
                ChallengeType.TrueFalse   => ScoreTrueFalse(correct, submitted),
                _                         => 0
            };
        }
        catch
        {
            return 0;
        }
    }

    private static int ScoreWord(JsonElement correct, JsonElement submitted)
    {
        var cWord = correct.GetProperty("word").GetString()?.ToUpper() ?? "";
        var sWord = submitted.TryGetProperty("word", out var w) ? w.GetString()?.ToUpper() ?? "" : "";
        return cWord == sWord ? 100 : 0;
    }

    private static int ScoreMultipleChoice(JsonElement correct, JsonElement submitted, string key)
    {
        if (!correct.TryGetProperty(key, out var ci) ||
            !submitted.TryGetProperty(key, out var si)) return 0;
        return ci.GetInt32() == si.GetInt32() ? 100 : 0;
    }

    private static int ScoreFillBlank(JsonElement correct, JsonElement submitted)
    {
        var cAns = correct.GetProperty("answer").GetString()?.Trim().ToLower() ?? "";
        var sAns = submitted.TryGetProperty("answer", out var a) ? a.GetString()?.Trim().ToLower() ?? "" : "";
        return cAns == sAns ? 100 : 0;
    }

    private static int ScoreMatchPairs(JsonElement correct, JsonElement submitted)
    {
        if (!correct.TryGetProperty("matches", out var cm) ||
            !submitted.TryGetProperty("matches", out var sm)) return 0;

        int total = 0, right = 0;
        foreach (var pair in cm.EnumerateObject())
        {
            total++;
            if (sm.TryGetProperty(pair.Name, out var sv) &&
                sv.GetString() == pair.Value.GetString())
                right++;
        }
        return total == 0 ? 0 : (int)Math.Round(100.0 * right / total);
    }

    private static int ScoreTrueFalse(JsonElement correct, JsonElement submitted)
    {
        if (!correct.TryGetProperty("answers", out var ca) ||
            !submitted.TryGetProperty("answers", out var sa)) return 0;

        int total = 0, right = 0;
        foreach (var pair in ca.EnumerateObject())
        {
            total++;
            if (sa.TryGetProperty(pair.Name, out var sv) &&
                sv.GetBoolean() == pair.Value.GetBoolean())
                right++;
        }
        return total == 0 ? 0 : (int)Math.Round(100.0 * right / total);
    }
}

public class SubmitAnswerRequest
{
    public string AnswerJson { get; set; } = "{}";
    public int TimeTakenSeconds { get; set; }
}
