using LiveExpert.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers;

// ─────────────────────────────────────────────────────────────────────────────
// Request models
// ─────────────────────────────────────────────────────────────────────────────

public class CareerPathRequest
{
    public string Interest { get; set; } = string.Empty;
    public string CurrentLevel { get; set; } = "Beginner";
    public string CareerGoal { get; set; } = string.Empty;
}

public class LinkedInOptimizerRequest
{
    public string CurrentAbout { get; set; } = string.Empty;
    public string CurrentHeadline { get; set; } = string.Empty;
    public string Skills { get; set; } = string.Empty;
    public string TargetRole { get; set; } = string.Empty;
}

public class ProjectIdeasRequest
{
    public string TechStack { get; set; } = string.Empty;
    public string ExperienceLevel { get; set; } = "Beginner";
    public string InterestedDomain { get; set; } = string.Empty;
}

public class CodeReviewRequest
{
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string? Context { get; set; }
}

// Simplified — accepts plain string fields from the frontend form
public class PortfolioGeneratorRequest
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string Skills { get; set; } = string.Empty;
    public string Projects { get; set; } = string.Empty;   // plain text list
    public string Email { get; set; } = string.Empty;
    public string? Github { get; set; }
}

public class DailyQuizRequest
{
    public string Subject { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "Medium";
}

public class FlashcardsRequest
{
    public string Topic { get; set; } = string.Empty;
    public int Count { get; set; } = 10;
}

// Simplified — accepts title/description/subjectType from the frontend form
public class AssignmentHelperRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SubjectType { get; set; } = string.Empty;
    public string? Deadline { get; set; }
}

// Simplified — single subject form instead of multi-subject array
public class StudyScheduleRequest
{
    public string Subject { get; set; } = string.Empty;
    public string ExamDate { get; set; } = string.Empty;
    public int HoursPerDay { get; set; } = 3;
    public string CurrentLevel { get; set; } = "Beginner";
    public string? Topics { get; set; }
}

public class WeeklyDigestRequest
{
    public string Interest { get; set; } = string.Empty;
    public string CareerGoal { get; set; } = string.Empty;
}

public class WellnessCheckinRequest
{
    public int EnergyLevel { get; set; } = 3;   // 1–5
    public int StressLevel { get; set; } = 3;    // 1–5
    public string? Mood { get; set; }
    public string? Challenges { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────
// Controller
// ─────────────────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/student-features")]
[Authorize(Roles = "Student")]
public class StudentFeaturesController : ControllerBase
{
    private readonly ClaudeAIService _ai;
    private readonly ILogger<StudentFeaturesController> _logger;

    public StudentFeaturesController(ClaudeAIService ai, ILogger<StudentFeaturesController> logger)
    {
        _ai = ai;
        _logger = logger;
    }

    // ── 1. Career Path ────────────────────────────────────────────────────────
    [HttpPost("career-path")]
    public async Task<IActionResult> CareerPath([FromBody] CareerPathRequest req)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(req.Interest))
                return BadRequest(new { error = "Interest is required" });

            var prompt = $@"Generate a detailed 6-month tech career roadmap tailored for the Indian job market.

STUDENT PROFILE:
- Interest / Domain: {req.Interest}
- Current Level: {req.CurrentLevel}
- Career Goal: {req.CareerGoal}

Provide a structured roadmap with:
1. MONTH-BY-MONTH BREAKDOWN — What to focus on each month (Month 1–6)
2. SKILLS TO LEARN — Prioritised skill list for each phase
3. PROJECTS TO BUILD — 2–3 portfolio projects that demonstrate readiness
4. JOB ROLES UNLOCKED — Which roles become achievable after 3 months and 6 months
5. EXPECTED SALARY RANGE — Entry-level salary ranges in INR for relevant roles in India
6. FREE RESOURCES — Best free platforms/courses to use
7. ACTION STEPS FOR THIS WEEK — 3 immediate things to start today

Keep it motivating, specific, and realistic for an Indian student or working professional.";

            var roadmap = await _ai.GenerateContentAsync(prompt);
            return Ok(new { success = true, roadmap });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating career path");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // ── 2. LinkedIn Optimizer ─────────────────────────────────────────────────
    [HttpPost("linkedin-optimizer")]
    public async Task<IActionResult> LinkedInOptimizer([FromBody] LinkedInOptimizerRequest req)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(req.TargetRole))
                return BadRequest(new { error = "Target role is required" });

            var prompt = $@"You are a LinkedIn profile optimisation expert. Rewrite and improve this student's profile for maximum recruiter visibility.

CURRENT PROFILE:
- About/Summary: {req.CurrentAbout}
- Current Headline: {req.CurrentHeadline}
- Current Skills: {req.Skills}
- Target Role: {req.TargetRole}

Return a JSON object with this exact structure:
{{
  ""optimisedAbout"": ""Rewritten About section (max 2600 chars, punchy, keyword-rich, first person)"",
  ""optimisedHeadline"": ""Optimised headline under 220 chars with role + value proposition"",
  ""skillsToAdd"": [""skill1"", ""skill2"", ""skill3"", ""skill4"", ""skill5"", ""skill6"", ""skill7"", ""skill8"", ""skill9"", ""skill10""],
  ""connectionMessages"": [
    ""Template 1 for cold outreach to recruiter (under 300 chars)"",
    ""Template 2 — referral request to employee at target company (under 300 chars)"",
    ""Template 3 — informational interview request (under 300 chars)""
  ]
}}

Return ONLY the JSON, no markdown wrapping.";

            var raw = await _ai.GenerateContentAsync(prompt);

            try
            {
                var parsed = System.Text.Json.JsonDocument.Parse(raw);
                return Ok(new { success = true, data = parsed });
            }
            catch
            {
                return Ok(new { success = true, rawResponse = raw });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimising LinkedIn profile");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // ── 3. Project Ideas ──────────────────────────────────────────────────────
    [HttpPost("project-ideas")]
    public async Task<IActionResult> ProjectIdeas([FromBody] ProjectIdeasRequest req)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(req.TechStack))
                return BadRequest(new { error = "Tech stack is required" });

            var prompt = $@"Generate 5 portfolio project ideas as a JSON array.

STUDENT PROFILE:
- Tech Stack: {req.TechStack}
- Experience Level: {req.ExperienceLevel}
- Interested Domain: {req.InterestedDomain}

Each project must follow this JSON structure:
{{
  ""title"": ""Project name"",
  ""description"": ""2-3 sentence description of what it does and its value"",
  ""techStack"": [""tech1"", ""tech2""],
  ""features"": [""Feature 1"", ""Feature 2"", ""Feature 3""],
  ""buildSteps"": [""Step 1"", ""Step 2"", ""Step 3"", ""Step 4""],
  ""difficultyLevel"": ""Beginner / Intermediate / Advanced"",
  ""estimatedDays"": 14
}}

Return a JSON array of 5 objects. No markdown, no explanation — just the JSON array.";

            var raw = await _ai.GenerateContentAsync(prompt);

            try
            {
                var parsed = System.Text.Json.JsonDocument.Parse(raw);
                return Ok(new { success = true, projects = parsed });
            }
            catch
            {
                return Ok(new { success = true, rawResponse = raw });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating project ideas");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // ── 4. Code Review ────────────────────────────────────────────────────────
    [HttpPost("code-review")]
    public async Task<IActionResult> CodeReview([FromBody] CodeReviewRequest req)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(req.Code))
                return BadRequest(new { error = "Code is required" });

            var contextNote = string.IsNullOrWhiteSpace(req.Context)
                ? ""
                : $"\nCONTEXT: {req.Context}";

            var prompt = $@"Perform a detailed code review of the following {req.Language} code.{contextNote}

CODE:
```{req.Language}
{req.Code}
```

Return a JSON object with this structure:
{{
  ""overallScore"": 75,
  ""issues"": [
    {{ ""severity"": ""error"", ""message"": ""Description of the issue and how to fix it"" }},
    {{ ""severity"": ""warning"", ""message"": ""Description of the warning"" }},
    {{ ""severity"": ""suggestion"", ""message"": ""A suggestion for improvement"" }}
  ],
  ""improvedCode"": ""The fully corrected and improved version of the code"",
  ""explanations"": [
    ""Explanation of change 1"",
    ""Explanation of change 2""
  ]
}}

Severity values must be exactly: ""error"", ""warning"", or ""suggestion"".
Return ONLY the JSON. No markdown.";

            var raw = await _ai.GenerateContentAsync(prompt);

            try
            {
                var parsed = System.Text.Json.JsonDocument.Parse(raw);
                return Ok(new { success = true, data = parsed });
            }
            catch
            {
                return Ok(new { success = true, rawResponse = raw });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing code");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // ── 5. Portfolio Generator ────────────────────────────────────────────────
    [HttpPost("portfolio-generator")]
    public async Task<IActionResult> PortfolioGenerator([FromBody] PortfolioGeneratorRequest req)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return BadRequest(new { error = "Name is required" });

            var prompt = $@"Generate a complete, single-file HTML portfolio website for this developer.

DEVELOPER INFO:
- Name: {req.Name}
- Title / Role: {req.Role}
- About: {req.Bio}
- Skills: {req.Skills}
- Projects: {req.Projects}
- Email: {req.Email}
- GitHub: {req.Github ?? "N/A"}

REQUIREMENTS:
- Single HTML file, fully self-contained (inline CSS + JS)
- Use Tailwind CSS via CDN (https://cdn.tailwindcss.com)
- Modern, clean design with a dark or gradient hero section
- Sections: Hero (name + title + CTA), About, Skills (badges/tags), Projects (cards), Contact
- Smooth scroll navigation
- Responsive (mobile-friendly)
- Project cards should show name, description, tech stack badges, and a link button
- Contact section with email link and social links
- No external JS frameworks — vanilla JS only
- Subtle CSS transition animations

Return ONLY the complete HTML from <!DOCTYPE html> to </html>. No explanation, no markdown.";

            var html = await _ai.GenerateContentAsync(prompt);
            return Ok(new { success = true, html });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating portfolio");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // ── 6. Daily Quiz ─────────────────────────────────────────────────────────
    [HttpPost("daily-quiz")]
    public async Task<IActionResult> DailyQuiz([FromBody] DailyQuizRequest req)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(req.Subject))
                return BadRequest(new { error = "Subject is required" });

            var prompt = $@"Generate 10 fresh multiple-choice questions (MCQs) on the subject: {req.Subject}
Difficulty: {req.Difficulty}

Return a JSON array of 10 question objects:
[
  {{
    ""question"": ""The question text"",
    ""options"": [""Option one"", ""Option two"", ""Option three"", ""Option four""],
    ""correctAnswer"": 0,
    ""explanation"": ""Brief explanation of why option 0 is correct""
  }}
]

IMPORTANT RULES:
- options is an array of 4 plain strings (no A./B./C. prefix)
- correctAnswer is a zero-based INTEGER index (0, 1, 2, or 3) — NOT a letter
- Questions must be unique and not trivially easy
- Vary question types: conceptual, application, scenario-based
- Return ONLY the JSON array, no markdown.";

            var raw = await _ai.GenerateContentAsync(prompt);

            try
            {
                var parsed = System.Text.Json.JsonDocument.Parse(raw);
                return Ok(new { success = true, questions = parsed });
            }
            catch
            {
                return Ok(new { success = true, rawResponse = raw });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating quiz");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // ── 7. Flashcards ─────────────────────────────────────────────────────────
    [HttpPost("flashcards")]
    public async Task<IActionResult> Flashcards([FromBody] FlashcardsRequest req)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(req.Topic))
                return BadRequest(new { error = "Topic is required" });

            var count = Math.Clamp(req.Count, 1, 50);

            var prompt = $@"Generate {count} flashcards on the topic: {req.Topic}

Return a JSON array:
[
  {{
    ""front"": ""Question or term on the front of the card"",
    ""back"": ""Concise answer or definition on the back""
  }}
]

Rules:
- Keep backs concise (1–3 sentences max)
- Cover a breadth of the topic
- Return ONLY the JSON array, no markdown.";

            var raw = await _ai.GenerateContentAsync(prompt);

            try
            {
                var parsed = System.Text.Json.JsonDocument.Parse(raw);
                return Ok(new { success = true, flashcards = parsed });
            }
            catch
            {
                return Ok(new { success = true, rawResponse = raw });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating flashcards");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // ── 8. Assignment Helper ──────────────────────────────────────────────────
    [HttpPost("assignment-helper")]
    public async Task<IActionResult> AssignmentHelper([FromBody] AssignmentHelperRequest req)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(req.Title))
                return BadRequest(new { error = "Assignment title is required" });

            var deadlineNote = string.IsNullOrWhiteSpace(req.Deadline)
                ? ""
                : $"\n- Deadline: {req.Deadline}";

            var prompt = $@"A student needs guided help with this {req.SubjectType} assignment.

ASSIGNMENT DETAILS:
- Title: {req.Title}
- Description: {req.Description}{deadlineNote}

IMPORTANT: Do NOT give the complete answer. Instead, guide the student to think and learn.

Return a JSON object:
{{
  ""approach"": ""A step-by-step written approach explaining how to tackle this assignment (3-5 paragraphs, use markdown for structure)"",
  ""researchPointers"": [
    ""Specific topic or resource to research 1"",
    ""Specific topic or resource to research 2"",
    ""Specific topic or resource to research 3"",
    ""Specific topic or resource to research 4""
  ],
  ""topicsToCover"": [
    ""Key topic 1"",
    ""Key topic 2"",
    ""Key topic 3"",
    ""Key topic 4"",
    ""Key topic 5""
  ],
  ""checklist"": [
    ""Checklist item 1 — something a complete submission must include"",
    ""Checklist item 2"",
    ""Checklist item 3"",
    ""Checklist item 4"",
    ""Checklist item 5""
  ]
}}

Return ONLY the JSON object. No markdown wrapping.";

            var raw = await _ai.GenerateContentAsync(prompt);

            try
            {
                var parsed = System.Text.Json.JsonDocument.Parse(raw);
                return Ok(new { success = true, data = parsed });
            }
            catch
            {
                return Ok(new { success = true, rawResponse = raw });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in assignment helper");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // ── 9. Study Schedule ─────────────────────────────────────────────────────
    [HttpPost("study-schedule")]
    public async Task<IActionResult> StudySchedule([FromBody] StudyScheduleRequest req)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(req.Subject))
                return BadRequest(new { error = "Subject is required" });

            var topicsNote = string.IsNullOrWhiteSpace(req.Topics)
                ? ""
                : $"\n- Topics to Cover: {req.Topics}";

            var prompt = $@"Create a day-by-day study timetable.

STUDENT INPUT:
- Subject: {req.Subject}
- Exam / Deadline Date: {req.ExamDate}
- Daily Available Hours: {req.HoursPerDay} hours/day
- Current Level: {req.CurrentLevel}{topicsNote}

REQUIREMENTS:
- Cover the subject thoroughly from today until the exam
- Include revision days (at least 2 days before the exam)
- Include at least one rest day per week
- Adapt difficulty based on current level

Return a JSON object:
{{
  ""totalDays"": 14,
  ""studyTips"": [
    ""Tip 1 personalised to this subject and level"",
    ""Tip 2"",
    ""Tip 3""
  ],
  ""schedule"": [
    {{
      ""day"": ""Day 1 — Monday"",
      ""date"": ""2026-04-14"",
      ""focusTopic"": ""Introduction to the subject"",
      ""hoursPlanned"": 3,
      ""tasks"": [
        ""Task description 1"",
        ""Task description 2""
      ]
    }}
  ]
}}

Return ONLY the JSON object. No markdown.";

            var raw = await _ai.GenerateContentAsync(prompt);

            try
            {
                var parsed = System.Text.Json.JsonDocument.Parse(raw);
                return Ok(new { success = true, schedule = parsed });
            }
            catch
            {
                return Ok(new { success = true, rawResponse = raw });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating study schedule");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // ── 10. Weekly Digest ─────────────────────────────────────────────────────
    [HttpPost("weekly-digest")]
    public async Task<IActionResult> WeeklyDigest([FromBody] WeeklyDigestRequest req)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(req.Interest))
                return BadRequest(new { error = "Interest is required" });

            var prompt = $@"Generate a personalised tech weekly digest for a student.

STUDENT PROFILE:
- Interest Area: {req.Interest}
- Career Goal: {req.CareerGoal}

Generate 5 important topics, trends, or developments in {req.Interest} that are relevant in 2026.

Return a JSON object:
{{
  ""digest"": [
    {{
      ""title"": ""Topic or trend title"",
      ""summary"": ""2-sentence plain-English summary"",
      ""whyItMatters"": ""1 sentence on why this matters for the student's career goal"",
      ""learnMoreSearchQuery"": ""A Google search query string to learn more""
    }}
  ]
}}

Return ONLY the JSON object. No markdown.";

            var raw = await _ai.GenerateContentAsync(prompt);

            try
            {
                var parsed = System.Text.Json.JsonDocument.Parse(raw);
                return Ok(new { success = true, digest = parsed });
            }
            catch
            {
                return Ok(new { success = true, rawResponse = raw });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating weekly digest");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // ── 11. Wellness Check-in ─────────────────────────────────────────────────
    [HttpPost("wellness-checkin")]
    public async Task<IActionResult> WellnessCheckin([FromBody] WellnessCheckinRequest req)
    {
        try
        {
            var challengesNote = string.IsNullOrWhiteSpace(req.Challenges)
                ? ""
                : $"\n- Challenges: {req.Challenges}";

            var prompt = $@"Analyse a student's daily wellness check-in and provide personalised support.

WELLNESS DATA:
- Energy Level: {req.EnergyLevel}/5
- Stress Level: {req.StressLevel}/5
- Current Mood: {req.Mood ?? "Not specified"}{challengesNote}

Based on this data, return a JSON object:
{{
  ""wellnessScore"": 72,
  ""summary"": ""One sentence summary of their current state"",
  ""affirmation"": ""A warm, genuine motivational affirmation (1 sentence, not cheesy)"",
  ""tips"": [
    ""Personalised wellness tip 1 (specific, actionable)"",
    ""Personalised wellness tip 2"",
    ""Personalised wellness tip 3"",
    ""Personalised wellness tip 4""
  ]
}}

Scoring guide:
- wellnessScore (0-100): energy contributes 30%, stress inverse 30%, mood positivity 40%
- High stress (4-5) should bring score below 50

Return ONLY the JSON object. No markdown.";

            var raw = await _ai.GenerateContentAsync(prompt);

            try
            {
                var parsed = System.Text.Json.JsonDocument.Parse(raw);
                return Ok(new { success = true, data = parsed });
            }
            catch
            {
                return Ok(new { success = true, rawResponse = raw });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in wellness check-in");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
