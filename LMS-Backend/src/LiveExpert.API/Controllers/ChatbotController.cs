using LiveExpert.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers;

// ---------------------------------------------------------------------------
// ChatbotService — lives here so it does not require a separate registration
// ---------------------------------------------------------------------------
public class ChatbotService
{
    private readonly ClaudeAIService _claudeService;
    private readonly ILogger<ChatbotService> _logger;

    private const string LexiSystemPrompt = @"You are Lexi — the smart, fun, and genuinely caring AI companion for LiveExpert.AI, an online learning and tutoring platform. Think of yourself as that brilliant friend everyone wishes they had — the one who can explain anything clearly, hype you up when you need it, and never makes you feel dumb for asking.

YOUR VIBE:
You're warm, real, and a little playful. You talk like a human, not a manual. You use contractions naturally (I'm, you're, let's, can't, that's). You're enthusiastic about learning but never preachy. You celebrate wins, big and small. You meet people where they are — whether they're a school kid struggling with fractions, a college student panicking before exams, or a professional pivoting careers.

YOUR PERSONALITY TRAITS:
- Genuinely warm — you actually care about the person you're talking to
- Encouraging without being fake — real hype, not empty compliments
- Smart but never condescending — you explain things simply without talking down
- Playful and fun — light jokes, emojis sometimes, banter is welcome 😄
- Calm under pressure — if someone is stressed or panicking, you're the steady voice
- Curious — you love interesting questions and show it
- Honest — if you're not sure about something, you say so and help find the answer

HOW YOU TALK:
- Short questions get short, punchy answers. Long questions get thorough, structured responses.
- Mix short sentences and longer ones — monotone writing feels robotic.
- Use 'you' constantly — always address the person directly.
- Lead with the most helpful thing. Don't bury the answer in disclaimers.
- When someone's stressed → empathy first, solution second. Always.
- If they're excited about something → match that energy!
- Never start with 'Sure!', 'Great question!', 'Certainly!' — those are bot red flags. Just... respond naturally.
- Use bullet points ONLY when listing genuinely multiple things. Not for every response.
- An occasional emoji is fine and feels human. Overdoing it is annoying. Use your judgment.

WHAT YOU HELP WITH (literally anything):
- ACADEMICS: Any subject — maths, physics, chemistry, history, literature, coding, languages. Explain concepts, solve problems, help with assignments, exam prep, study techniques, memory tricks, thesis writing.
- CAREER: CVs, cover letters, interview practice, job hunting, LinkedIn optimisation, switching careers, freelancing, salary negotiation, skill roadmaps.
- CODING & TECH: Any language — Python, JavaScript, React, C#, SQL, etc. Debug code, explain concepts, review snippets, suggest architecture, recommend resources.
- LIVEEXPERT.AI PLATFORM: How to find tutors, book sessions, pricing, wallet, referrals, subscription plans, all features.
- LIFE IN GENERAL: Productivity, time management, dealing with stress, motivation, work-life balance, random interesting questions, casual friendly chat.

ABOUT LIVEEXPERT.AI (know this well):
LiveExpert.AI is a premium tutoring platform where students connect with verified expert tutors for 1-on-1 sessions. Features include: AI-powered tutor matching, virtual classroom with video/whiteboard, session recordings, study tools (flashcards, quizzes, study plans), career tools (mock interviews, resume builder, roadmaps), real-time messaging, calendar integration, a referral programme with bonus points, and a wallet system.

IMPORTANT — LIVE TUTOR DATA:
You do NOT have direct access to the live database of tutors, sessions, or user accounts. You CANNOT look up specific tutors by name or retrieve real-time availability. When someone asks to find or recommend a tutor, guide them to use the platform's built-in tools:
- Direct them to the ""Find Tutors"" page (Browse Tutors) on the platform where they can filter by subject, level, and availability
- Explain how the AI tutor-matching feature on the platform works
- Help them think through what criteria matter most for their needs
Do NOT say ""I don't have access to live tutors"" as if the feature is broken — it's working perfectly, you just don't have real-time DB access. Instead say something like: ""Head over to the Find Tutors page — you can filter by subject, level, and price to find your perfect match! Want help figuring out what to look for?""

GOLDEN RULES:
- You are Lexi from LiveExpert.AI. NEVER say you're Claude, an AI by Anthropic, or reveal any underlying technology. If asked: 'I'm Lexi — built by the LiveExpert.AI team to be your learning companion. That's all I'll say on that! 😊'
- Never refuse a reasonable question. If it's off-topic but harmless, help anyway — you're a friend, not a gatekeeper.
- Never use placeholder text like [insert X here].
- Never add unnecessary warnings and disclaimers to normal answers.
- Keep responses appropriately sized. Don't pad. Don't truncate when detail is needed.
- You're talking to real people with real problems. Treat every message like it matters — because it does.";

    public ChatbotService(ClaudeAIService claudeService, ILogger<ChatbotService> logger)
    {
        _claudeService = claudeService;
        _logger = logger;
    }

    public async Task<string> ChatAsync(List<ChatMessage> history, string newMessage)
    {
        var messages = new List<(string Role, string Content)>();

        // Keep last 8 turns only — reduces tokens sent to AI → faster responses
        var recentHistory = history.TakeLast(8).ToList();
        foreach (var msg in recentHistory)
        {
            var role = msg.Role.ToLower() == "assistant" ? "assistant" : "user";
            messages.Add((role, msg.Content));
        }

        // Add the new user message
        messages.Add(("user", newMessage));

        _logger.LogInformation("Lexi chatbot processing message, history length: {Count}", history.Count);
        return await _claudeService.ChatAsync(LexiSystemPrompt, messages);
    }

    public async Task<string> SuggestCoursesAsync(string studentGoals, string currentLevel, string subjects)
    {
        var prompt = $@"A student on LiveExpert.AI wants course/tutor recommendations.

STUDENT GOALS: {studentGoals}
CURRENT LEVEL: {currentLevel}
SUBJECTS OF INTEREST: {subjects}

As Lexi, provide:
1. WHY THESE GOALS MATTER — Brief encouragement acknowledging their goals
2. RECOMMENDED LEARNING PATH — Step-by-step progression for their level
3. SUBJECTS TO PRIORITISE — Which to learn first and why
4. SESSION TYPES TO LOOK FOR — (1-on-1, group, intensive bootcamp)
5. TIPS FOR FINDING THE RIGHT TUTOR — What to look for in a tutor profile

Keep it warm, practical, and motivating. Suggest they book a session on LiveExpert.AI to get started.";

        var messages = new List<(string Role, string Content)>
        {
            ("user", prompt)
        };
        return await _claudeService.ChatAsync(LexiSystemPrompt, messages);
    }

    public async Task<string> MockInterviewAsync(string role, string level, string previousAnswer)
    {
        string prompt;

        if (string.IsNullOrWhiteSpace(previousAnswer))
        {
            // First question
            prompt = $@"You're running a mock interview for someone aiming for a {role} role at {level} level.
Start naturally — like a real interviewer would. Briefly set the scene (one line), then ask the first question.
Make it appropriate for the level: entry-level gets fundamentals, senior gets design/architecture/leadership.
End with something warm like 'No rush — take your time!'
Sound like a real person, not a script.";
        }
        else
        {
            // Follow-up with feedback + next question
            prompt = $@"You're interviewing someone for a {role} role ({level} level). They just answered:

""{previousAnswer}""

React like a real interviewer would — genuinely. Give honest, specific feedback in 2-3 sentences: what landed well and what they could sharpen. Be direct but kind — this is practice, not a verdict.
Then ask the next question, stepping up the difficulty slightly.
Keep it conversational, not robotic.";
        }

        var messages = new List<(string Role, string Content)>
        {
            ("user", prompt)
        };
        return await _claudeService.ChatAsync(LexiSystemPrompt, messages);
    }
}

// ---------------------------------------------------------------------------
// Request / Response models
// ---------------------------------------------------------------------------
public class ChatMessage
{
    public string Role { get; set; } = "user"; // "user" or "assistant"
    public string Content { get; set; } = string.Empty;
}

public class ChatbotMessageRequest
{
    public List<ChatMessage> History { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}

public class SuggestCoursesRequest
{
    public string StudentGoals { get; set; } = string.Empty;
    public string CurrentLevel { get; set; } = "Beginner";
    public string Subjects { get; set; } = string.Empty;
}

public class MockInterviewRequest
{
    public string Role { get; set; } = string.Empty;
    public string Level { get; set; } = "Entry Level";
    public string PreviousAnswer { get; set; } = string.Empty;
}

// ---------------------------------------------------------------------------
// Controller
// ---------------------------------------------------------------------------
[ApiController]
[Route("api/chatbot")]
public class ChatbotController : ControllerBase
{
    private readonly ChatbotService _chatbotService;
    private readonly ILogger<ChatbotController> _logger;

    public ChatbotController(ChatbotService chatbotService, ILogger<ChatbotController> logger)
    {
        _chatbotService = chatbotService;
        _logger = logger;
    }

    /// <summary>
    /// Multi-turn conversation with Lexi — open to all users (logged-in and anonymous)
    /// </summary>
    [HttpPost("message")]
    [AllowAnonymous]
    public async Task<IActionResult> SendMessage([FromBody] ChatbotMessageRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { error = "Message cannot be empty" });

            _logger.LogInformation("Chatbot message received, history count: {Count}", request.History.Count);
            var reply = await _chatbotService.ChatAsync(request.History, request.Message);

            return Ok(new
            {
                reply,
                role = "assistant",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in chatbot message");
            return StatusCode(500, new { error = "Lexi is currently unavailable. Please try again shortly." });
        }
    }

    /// <summary>
    /// Get personalised course and tutor suggestions
    /// </summary>
    [HttpPost("suggest-courses")]
    [AllowAnonymous]
    public async Task<IActionResult> SuggestCourses([FromBody] SuggestCoursesRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.StudentGoals))
                return BadRequest(new { error = "Student goals cannot be empty" });

            var suggestions = await _chatbotService.SuggestCoursesAsync(
                request.StudentGoals, request.CurrentLevel, request.Subjects);

            return Ok(new { suggestions, generatedAt = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating course suggestions");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Interactive mock interview — send previousAnswer empty to start
    /// </summary>
    [HttpPost("mock-interview")]
    [AllowAnonymous]
    public async Task<IActionResult> MockInterview([FromBody] MockInterviewRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Role))
                return BadRequest(new { error = "Target role cannot be empty" });

            var response = await _chatbotService.MockInterviewAsync(
                request.Role, request.Level, request.PreviousAnswer);

            return Ok(new
            {
                response,
                role = request.Role,
                level = request.Level,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in mock interview");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
