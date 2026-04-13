using LiveExpert.API.Services;
using LiveExpert.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AIController : ControllerBase
    {
        private readonly LMSAIService _lmsAIService;
        private readonly ILogger<AIController> _logger;

        public AIController(LMSAIService lmsAIService, ILogger<AIController> logger)
        {
            _lmsAIService = lmsAIService;
            _logger = logger;
        }

        [HttpGet("available-models")]
        public async Task<IActionResult> AvailableModels()
        {
            try
            {
                _logger.LogInformation("Fetching available models from Gemini API");
                var models = await _lmsAIService.GetAvailableModelsAsync();
                return Ok(new { models = models });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching available models");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("tutor-match")]
        public async Task<IActionResult> TutorMatch([FromBody] TutorMatchRequest request)
        {
            try
            {
                _logger.LogInformation("TutorMatch request: Subject={Subject}, Level={Level}", request.Subject, request.Level);
                var result = await _lmsAIService.TutorMatch(request.Subject, request.Level);
                return Ok(new { recommendation = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TutorMatch endpoint");
                return StatusCode(500, new { error = ex.Message, details = ex.InnerException?.Message });
            }
        }

        [HttpPost("study-plan")]
        public async Task<IActionResult> StudyPlan([FromBody] StudyPlanRequest request)
        {
            try
            {
                var result = await _lmsAIService.StudyPlan(request.Subject, request.Goal, request.Time, request.Duration);
                return Ok(new { plan = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in StudyPlan endpoint");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("session-summary")]
        public async Task<IActionResult> SessionSummary([FromBody] SessionSummaryRequest request)
        {
            try
            {
                var result = await _lmsAIService.SessionSummary(request.Transcript);
                return Ok(new { summary = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SessionSummary endpoint");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("quiz")]
        public async Task<IActionResult> Quiz([FromBody] QuizRequest request)
        {
            try
            {
                var result = await _lmsAIService.QuizGenerator(request.Topic, request.Difficulty);
                return Ok(new { quiz = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Quiz endpoint");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("flashcards")]
        public async Task<IActionResult> Flashcards([FromBody] FlashcardRequest request)
        {
            try
            {
                var result = await _lmsAIService.Flashcards(request.Topic);
                return Ok(new { flashcards = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Flashcards endpoint");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("homework-help")]
        public async Task<IActionResult> HomeworkHelp([FromBody] HomeworkRequest request)
        {
            try
            {
                var result = await _lmsAIService.HomeworkHelper(request.Question);
                return Ok(new { explanation = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HomeworkHelp endpoint");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("session-notes")]
        public async Task<IActionResult> SessionNotes([FromBody] SessionNotesRequest request)
        {
            try
            {
                var result = await _lmsAIService.GenerateSessionNotes(request.Transcript, request.FocusAreas);
                return Ok(new { notes = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SessionNotes endpoint");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("lesson-plan")]
        public async Task<IActionResult> LessonPlan([FromBody] LessonPlanRequest request)
        {
            try
            {
                var result = await _lmsAIService.GenerateLessonPlan(request.Subject, request.Level, request.Topic, request.LearningObjectives);
                return Ok(new { plan = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LessonPlan endpoint");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("progress-report")]
        public async Task<IActionResult> ProgressReport([FromBody] ProgressReportRequest request)
        {
            try
            {
                var result = await _lmsAIService.GenerateProgressReport(request.StudentName, request.FeedbackHistory);
                return Ok(new { report = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProgressReport endpoint");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] AIChatRequest request)
        {
            try
            {
                var result = await _lmsAIService.GeneralChat(request.Message, request.UserContext);
                return Ok(new { response = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Chat endpoint");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class AIChatRequest
    {
        public string Message { get; set; } = string.Empty;
        public string? UserContext { get; set; }
    }
}