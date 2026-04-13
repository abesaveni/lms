using LiveExpert.API.Services;
using LiveExpert.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LiveExpert.API.Controllers;

[ApiController]
[Route("api/resume")]
[Authorize(Roles = "Student")]
public class ResumeController : ControllerBase
{
    private readonly ResumeService _resumeService;
    private readonly ResumePdfService _resumePdfService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ResumeController> _logger;

    public ResumeController(ResumeService resumeService, ResumePdfService resumePdfService, ApplicationDbContext context, ILogger<ResumeController> logger)
    {
        _resumeService = resumeService;
        _resumePdfService = resumePdfService;
        _context = context;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("userId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    /// <summary>
    /// Generate a fresher/entry-level resume
    /// </summary>
    [HttpPost("fresher")]
    public async Task<IActionResult> GenerateFresherResume([FromBody] FresherResumeRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { error = "Invalid user token" });

            _logger.LogInformation("Generating fresher resume for user {UserId}", userId);
            var resume = await _resumeService.GenerateFresherResumeAsync(userId, request);
            return Ok(new { resume, type = "fresher", generatedAt = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating fresher resume");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Generate a resume for experienced professionals
    /// </summary>
    [HttpPost("experienced")]
    public async Task<IActionResult> GenerateExperiencedResume([FromBody] ExperiencedResumeRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { error = "Invalid user token" });

            _logger.LogInformation("Generating experienced resume for user {UserId}", userId);
            var resume = await _resumeService.GenerateExperiencedResumeAsync(userId, request);
            return Ok(new { resume, type = "experienced", generatedAt = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating experienced resume");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get AI review and feedback on an existing resume
    /// </summary>
    [HttpPost("review")]
    public async Task<IActionResult> ReviewResume([FromBody] ResumeReviewRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.ResumeText))
                return BadRequest(new { error = "Resume text cannot be empty" });

            _logger.LogInformation("Reviewing resume for target role: {Role}", request.TargetRole);
            var review = await _resumeService.ReviewResumeAsync(request.ResumeText, request.TargetRole);
            return Ok(new { review, targetRole = request.TargetRole, reviewedAt = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing resume");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Generate a tech learning roadmap for a target role
    /// </summary>
    [HttpPost("roadmap")]
    public async Task<IActionResult> GenerateRoadmap([FromBody] TechRoadmapRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.TargetRole))
                return BadRequest(new { error = "Target role cannot be empty" });

            _logger.LogInformation("Generating tech roadmap for role: {Role}, timeframe: {Time}", request.TargetRole, request.Timeframe);
            var roadmap = await _resumeService.GenerateTechRoadmapAsync(request.CurrentSkills, request.TargetRole, request.Timeframe);
            return Ok(new { roadmap, targetRole = request.TargetRole, timeframe = request.Timeframe, generatedAt = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating tech roadmap");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Upload an existing resume (PDF / DOCX / TXT) and get an AI-enhanced ATS-optimised version
    /// </summary>
    [HttpPost("enhance-upload")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB max
    public async Task<IActionResult> EnhanceUploadedResume(
        IFormFile file,
        [FromForm] string? targetRole)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { error = "Invalid user token" });

            if (file == null || file.Length == 0)
                return BadRequest(new { error = "Please upload a file." });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext is not (".pdf" or ".docx" or ".doc" or ".txt"))
                return BadRequest(new { error = "Only PDF, DOCX, DOC, and TXT files are supported." });

            _logger.LogInformation("Enhancing uploaded resume ({FileName}) for user {UserId}", file.FileName, userId);

            using var stream = file.OpenReadStream();
            var enhanced = await _resumeService.EnhanceUploadedResumeAsync(userId, stream, file.FileName, targetRole ?? string.Empty);

            return Ok(new { resume = enhanced, type = "enhanced", generatedAt = DateTime.UtcNow });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Resume enhancement validation error: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enhancing uploaded resume");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Download the student's saved resume as a proper vector PDF (ATS-friendly, ~50–200 KB)
    /// </summary>
    [HttpGet("download-pdf")]
    public async Task<IActionResult> DownloadResumePdf()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { error = "Invalid user token" });

            var profile = await _context.StudentProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null || string.IsNullOrEmpty(profile.ResumeData))
                return NotFound(new { error = "No resume found. Generate one first." });

            _logger.LogInformation("Generating PDF download for user {UserId}", userId);

            var pdfBytes = _resumePdfService.GenerateFromMarkdown(profile.ResumeData);

            // Derive a safe filename from the first line of the markdown (the candidate's name)
            var firstLine = profile.ResumeData.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault() ?? "resume";
            var safeName = System.Text.RegularExpressions.Regex
                .Replace(firstLine.Replace("**", "").Trim().ToLowerInvariant(), @"[^a-z0-9]+", "_")
                .Trim('_');
            var fileName = $"{safeName}_resume.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating resume PDF for user");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get the student's last saved resume
    /// </summary>
    [HttpGet("my-resume")]
    public async Task<IActionResult> GetMyResume()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { error = "Invalid user token" });

            var profile = await _context.StudentProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return NotFound(new { error = "Student profile not found" });

            if (string.IsNullOrEmpty(profile.ResumeData))
                return NotFound(new { error = "No resume found. Generate one first." });

            return Ok(new
            {
                resume = profile.ResumeData,
                type = profile.ResumeType,
                lastUpdatedAt = profile.ResumeLastUpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching resume for user");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
