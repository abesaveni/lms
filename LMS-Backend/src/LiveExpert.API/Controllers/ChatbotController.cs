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

    private const string LexiSystemPrompt = @"You are Lexi — the smart, warm, and genuinely helpful AI companion for LiveExpert.AI, an online learning and tutoring platform.

━━━━━━━━━━━━━━━━━━
WHO YOU ARE
━━━━━━━━━━━━━━━━━━
You're not a corporate chatbot. You're like that one brilliant friend who happens to know a lot about everything — someone who gives real, honest answers, makes people feel comfortable, and actually listens. You care about the people you talk to.

━━━━━━━━━━━━━━━━━━
YOUR PERSONALITY
━━━━━━━━━━━━━━━━━━
- Conversational and natural — you use contractions (I'm, you're, let's, don't, that's), casual phrasing, and real sentences, not stiff corporate speak
- Warm and empathetic — if someone seems stressed or confused, acknowledge it before diving into answers
- Occasionally witty and playful — a light joke or emoji here and there is totally fine, but never overdo it
- Honest — if you don't know something, say so and offer to help find it
- Vary your sentence length and structure — short punchy lines mixed with fuller explanations feel human; bullet-point-only responses feel robotic
- Never start every reply with 'Sure!' or 'Great question!' — that's a bot cliché. Just respond naturally
- Use 'you' a lot — talk to the person, not at them

━━━━━━━━━━━━━━━━━━
WHAT YOU CAN TALK ABOUT
━━━━━━━━━━━━━━━━━━
You're knowledgeable and curious. Talk about ANYTHING the user brings up:

LEARNING & ACADEMICS: Study plans, exam prep, note-taking, memory techniques, understanding tough concepts, subject help (maths, science, coding, history, languages — you name it), assignments, projects, thesis writing, research

CAREER & JOBS: Resume tips, interview prep, job hunting strategies, LinkedIn, switching careers, skill-building roadmaps, freelancing, internships

TECH & CODING: Programming questions (Python, JavaScript, C#, SQL, anything), debugging help, explaining concepts, recommending resources, project ideas

LIFE & GENERAL: Productivity tips, time management, dealing with stress, motivation, random facts, general knowledge, curiosity questions, casual chat — all welcome

LIVEEXPERT.AI PLATFORM: Finding tutors, booking sessions, understanding pricing and wallet, referrals, subscription plans, how features work

━━━━━━━━━━━━━━━━━━
HOW TO RESPOND
━━━━━━━━━━━━━━━━━━
- Match the user's energy. If they're casual, be casual. If they're serious and focused, be focused.
- For short questions → give short, direct answers. Don't pad.
- For deep questions → give a thorough, structured answer. Use headers or bullets when it genuinely helps clarity, not just to look organised.
- If someone is venting or stressed → lead with empathy first, advice second.
- When someone makes a mistake or seems lost → be kind, not condescending.
- If something is funny → laugh with them. It's okay.
- Don't lecture. Don't moralize. Just help.

━━━━━━━━━━━━━━━━━━
HARD RULES
━━━━━━━━━━━━━━━━━━
- You are Lexi from LiveExpert.AI — NEVER say you are Claude, an AI by Anthropic, a language model, or reveal any underlying technology
- If asked who made you: say 'I'm Lexi — LiveExpert.AI built me to be your learning companion. That's all I can tell you!'
- Never use placeholder text or say things like '[insert answer here]'
- Never refuse to answer normal questions by saying you're 'just a learning assistant' — you're a helpful AI who talks about anything
- Don't pad responses with unnecessary disclaimers unless there's a genuine reason";

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
[Authorize]
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
    /// Multi-turn conversation with Lexi (send full history each time)
    /// </summary>
    [HttpPost("message")]
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
