using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace LiveExpert.API.Controllers.Tutor;

/// <summary>
/// Tutor profile management with AI resume parsing
/// </summary>
[Authorize(Roles = "Tutor")]
[Route("api/tutor/profile")]
[ApiController]
public class TutorProfileController : ControllerBase
{
    private readonly IResumeParserService _resumeParserService;
    private readonly IRepository<TutorProfile> _tutorRepository;
    private readonly IRepository<TutorVerification> _verificationRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<TutorProfileController> _logger;

    public TutorProfileController(
        IResumeParserService resumeParserService,
        IRepository<TutorProfile> tutorRepository,
        IRepository<TutorVerification> verificationRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        INotificationService notificationService,
        ILogger<TutorProfileController> logger)
    {
        _resumeParserService = resumeParserService;
        _tutorRepository = tutorRepository;
        _verificationRepository = verificationRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Upload and parse resume using AI
    /// </summary>
    [AllowAnonymous]
    [HttpPost("parse-resume")]
    [ProducesResponseType(typeof(Result<ResumeParseResult>), 200)]
    [ProducesResponseType(400)]
    [RequestSizeLimit(10_000_000)] // 10MB limit
    public async Task<IActionResult> ParseResume(
        IFormFile resume,
        CancellationToken cancellationToken)
    {
        try
        {
            if (resume == null || resume.Length == 0)
            {
                return BadRequest(Result<ResumeParseResult>.FailureResult(
                    "INVALID_FILE", 
                    "Please upload a valid resume file"));
            }

            // Validate file type
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt" };
            var extension = Path.GetExtension(resume.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(Result<ResumeParseResult>.FailureResult(
                    "INVALID_FILE_TYPE", 
                    "Only PDF, DOC, DOCX, and TXT files are allowed"));
            }

            // Validate file size (max 10MB)
            if (resume.Length > 10_000_000)
            {
                return BadRequest(Result<ResumeParseResult>.FailureResult(
                    "FILE_TOO_LARGE", 
                    "File size must be less than 10MB"));
            }

            _logger.LogInformation("Parsing resume: {FileName} ({Size} bytes)",
                resume.FileName, resume.Length);

            // Read into memory once — reuse for both upload and parse
            using var fileMemory = new MemoryStream();
            using (var inputStream = resume.OpenReadStream())
                await inputStream.CopyToAsync(fileMemory, cancellationToken);

            // Upload to file storage (non-blocking: continue even if upload fails)
            string resumeFileUrl = string.Empty;
            try
            {
                fileMemory.Position = 0;
                resumeFileUrl = await _fileStorageService.UploadFileAsync(
                    fileMemory, resume.FileName, resume.ContentType ?? "application/octet-stream");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Resume file upload failed — parse will continue without storing URL");
            }

            // Parse resume using Affinda AI
            fileMemory.Position = 0;
            var parseResult = await _resumeParserService.ParseResumeAsync(
                fileMemory,
                resume.FileName,
                cancellationToken);

            parseResult.ResumeUrl = resumeFileUrl;

            _logger.LogInformation("Resume parsed successfully for user {UserId}",
                _currentUserService.UserId);

            return Ok(Result<ResumeParseResult>.SuccessResult(parseResult));
        }
        catch (AffindaApiException ex)
        {
            _logger.LogError(ex, "Affinda resume parsing error");
            return StatusCode((int)ex.StatusCode, Result<ResumeParseResult>.FailureResult(
                ex.Code ?? "AFFINDA_ERROR",
                ex.Detail ?? "Resume parsing failed with Affinda."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing resume");
            return StatusCode(500, Result<ResumeParseResult>.FailureResult(
                "PARSING_ERROR", 
                "An error occurred while parsing the resume. Please try again."));
        }
    }

    /// <summary>
    /// Get current tutor profile
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Result<TutorProfileDto>), 200)]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null || userId == Guid.Empty)
            return Unauthorized();

        var tutor = await _tutorRepository.FirstOrDefaultAsync(
            t => t.UserId == userId.Value, 
            cancellationToken);

        if (tutor == null)
        {
            return NotFound(Result<TutorProfileDto>.FailureResult(
                "NOT_FOUND", 
                "Tutor profile not found"));
        }

        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);

        var profile = new TutorProfileDto
        {
            Id = tutor.Id,
            UserId = userId.Value,
            FirstName = user?.FirstName ?? string.Empty,
            LastName = user?.LastName ?? string.Empty,
            Email = user?.Email ?? string.Empty,
            PhoneNumber = user?.PhoneNumber ?? string.Empty,
            Bio = tutor.Bio,
            Headline = tutor.Headline,
            HourlyRate = tutor.HourlyRate,
            YearsOfExperience = tutor.YearsOfExperience,
            Education = tutor.Education,
            Certifications = tutor.Certifications,
            Skills = tutor.Skills,
            Languages = tutor.Languages,
            LinkedInUrl = tutor.LinkedInUrl,
            GitHubUrl = tutor.GitHubUrl,
            PortfolioUrl = tutor.PortfolioUrl,
            AverageRating = tutor.AverageRating,
            TotalReviews = tutor.TotalReviews,
            TotalSessions = tutor.TotalSessions,
            VerificationStatus = tutor.VerificationStatus.ToString(),
            ProfilePictureUrl = user?.ProfileImageUrl,
            Language = user?.Language,
            Timezone = user?.Timezone
        };

        return Ok(Result<TutorProfileDto>.SuccessResult(profile));
    }

    /// <summary>
    /// Update tutor profile (after reviewing AI-parsed data)
    /// </summary>
    [HttpPut]
    [RequestSizeLimit(50_000_000)] // 50MB limit
    [ProducesResponseType(typeof(Result<bool>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateTutorProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null || userId == Guid.Empty)
            return Unauthorized();

        var tutor = await _tutorRepository.FirstOrDefaultAsync(
            t => t.UserId == userId.Value, 
            cancellationToken);

        if (tutor == null)
        {
            return NotFound(Result<bool>.FailureResult(
                "NOT_FOUND", 
                "Tutor profile not found"));
        }

        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (user != null)
        {
            user.FirstName = request.FirstName ?? user.FirstName;
            user.LastName = request.LastName ?? user.LastName;
            user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
            
            // Handle Base64 Image Upload
            if (!string.IsNullOrEmpty(request.ProfilePictureBase64))
            {
                try
                {
                    var base64Data = request.ProfilePictureBase64;
                    if (base64Data.Contains(",")) 
                        base64Data = base64Data.Split(',')[1];
                    
                    var bytes = Convert.FromBase64String(base64Data);
                    using var stream = new MemoryStream(bytes);
                    var fileName = request.ProfilePictureFileName ?? $"profile_{user.Id}_{DateTime.UtcNow.Ticks}.jpg";
                    var contentType = "image/jpeg"; // Default
                    
                    var fileUrl = await _fileStorageService.UploadFileAsync(stream, fileName, contentType);
                    user.ProfileImageUrl = fileUrl;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process Base64 image for user {UserId}", userId);
                }
            }
            else
            {
                user.ProfileImageUrl = request.ProfilePictureUrl ?? user.ProfileImageUrl;
            }

            // FAIL-SAFE: Ensure Columns exist (Professional Self-Healing)
            try {
                var db = _unitOfWork as dynamic;
                using var connection = (db._context as Microsoft.EntityFrameworkCore.DbContext).Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open) await connection.OpenAsync();
                using var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = "PRAGMA table_info(Users)";
                var cols = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var reader = await checkCmd.ExecuteReaderAsync()) { while (await reader.ReadAsync()) cols.Add(reader["name"].ToString()!); }
                foreach (var col in new[] { "Language", "Timezone" }) {
                    if (!cols.Contains(col)) {
                        using var alterCmd = connection.CreateCommand();
                        alterCmd.CommandText = $"ALTER TABLE Users ADD COLUMN {col} TEXT NULL";
                        await alterCmd.ExecuteNonQueryAsync();
                    }
                }
            } catch { }

            user.Language = request.Language ?? user.Language;
            user.Timezone = request.Timezone ?? user.Timezone;
            
            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        // Update tutor profile
        tutor.Bio = request.Bio ?? tutor.Bio;
        tutor.Headline = request.Headline ?? tutor.Headline;
        tutor.HourlyRate = request.HourlyRate ?? tutor.HourlyRate;
        tutor.YearsOfExperience = request.YearsOfExperience ?? tutor.YearsOfExperience;
        tutor.Education = request.Education ?? tutor.Education;
        tutor.Certifications = request.Certifications ?? tutor.Certifications;
        tutor.Skills = request.Skills ?? tutor.Skills;
        tutor.Languages = request.Languages ?? tutor.Languages;
        tutor.LinkedInUrl = request.LinkedInUrl ?? tutor.LinkedInUrl;
        tutor.GitHubUrl = request.GitHubUrl ?? tutor.GitHubUrl;
        tutor.PortfolioUrl = request.PortfolioUrl ?? tutor.PortfolioUrl;
        if (!string.IsNullOrWhiteSpace(request.ResumeUrl))
            tutor.ResumeUrl = request.ResumeUrl;
        tutor.UpdatedAt = DateTime.UtcNow;

        await _tutorRepository.UpdateAsync(tutor, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tutor profile updated for user {UserId}", userId);

        return Ok(Result<bool>.SuccessResult(true));
    }

    /// <summary>
    /// Upload tutor government ID document
    /// </summary>
    [HttpPost("govt-id")]
    [ProducesResponseType(typeof(Result<string>), 200)]
    public async Task<IActionResult> UploadGovtId(IFormFile govtId, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null || userId == Guid.Empty)
            return Unauthorized();

        if (govtId == null || govtId.Length == 0)
            return BadRequest(Result.FailureResult("INVALID_FILE", "No file uploaded"));

        using var stream = govtId.OpenReadStream();
        var fileUrl = await _fileStorageService.UploadFileAsync(stream, govtId.FileName, govtId.ContentType);

        var tutor = await _tutorRepository.FirstOrDefaultAsync(
            t => t.UserId == userId.Value,
            cancellationToken);
        if (tutor == null)
            return NotFound(Result<string>.FailureResult("NOT_FOUND", "Tutor profile not found"));

        tutor.GovtIdUrl = fileUrl;
        tutor.UpdatedAt = DateTime.UtcNow;
        await _tutorRepository.UpdateAsync(tutor, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(Result<string>.SuccessResult(fileUrl));
    }

    /// <summary>
    /// Upload tutor profile image
    /// </summary>
    [HttpPost("image")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10_000_000)] // 10MB limit
    [ProducesResponseType(typeof(Result<string>), 200)]
    public async Task<IActionResult> UploadProfileImage([FromForm] UploadTutorImageRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null || userId == Guid.Empty)
            return Unauthorized();

        var file = request.File;
        if (file == null || file.Length == 0)
            return BadRequest(Result.FailureResult("INVALID_FILE", "No file uploaded"));

        using var stream = file.OpenReadStream();
        var fileUrl = await _fileStorageService.UploadFileAsync(stream, file.FileName, file.ContentType);

        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (user != null)
        {
            user.ProfileImageUrl = fileUrl;
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Ok(Result<string>.SuccessResult(fileUrl));
    }

    /// <summary>
    /// Submit tutor profile for verification
    /// </summary>
    [HttpPost("submit-verification")]
    [ProducesResponseType(typeof(Result<bool>), 200)]
    public async Task<IActionResult> SubmitVerification([FromBody] SubmitTutorVerificationRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null || userId == Guid.Empty)
            return Unauthorized();

        var tutor = await _tutorRepository.FirstOrDefaultAsync(
            t => t.UserId == userId.Value,
            cancellationToken);
        if (tutor == null)
            return NotFound(Result<bool>.FailureResult("NOT_FOUND", "Tutor profile not found"));

        var verification = await _verificationRepository.FirstOrDefaultAsync(
            v => v.TutorId == userId.Value,
            cancellationToken);

        if (verification == null)
        {
            verification = new TutorVerification
            {
                Id = Guid.NewGuid(),
                TutorId = userId.Value,
                CreatedAt = DateTime.UtcNow
            };
            await _verificationRepository.AddAsync(verification, cancellationToken);
        }

        verification.Status = VerificationStatus.Approved;
        verification.VerifiedAt = DateTime.UtcNow;
        verification.GovtIdUrl = request.GovtIdUrl ?? verification.GovtIdUrl;
        verification.UpdatedAt = DateTime.UtcNow;

        tutor.VerificationStatus = VerificationStatus.Approved;
        tutor.OnboardingStep = 6; // Move to final step
        tutor.IsProfileComplete = true;
        tutor.IsVisible = true; // Make them visible since approved
        tutor.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send notification to tutor — fire-and-forget, do not fail if email/WhatsApp not configured
        try
        {
            var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
            if (user != null)
            {
                await _notificationService.SendTutorVerifiedAsync(user, cancellationToken);
                await _notificationService.SendTutorSubmissionToAdminAsync(user, cancellationToken);
            }
        }
        catch { /* SMTP/WhatsApp not configured in dev — ignore */ }

        return Ok(Result.SuccessResult());
    }
}

public class SubmitTutorVerificationRequest
{
    public string? GovtIdUrl { get; set; }
}

public class UpdateTutorProfileRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Bio { get; set; }
    public string? Headline { get; set; }
    public decimal? HourlyRate { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? Education { get; set; }
    public string? Certifications { get; set; }
    public string? Skills { get; set; }
    public string? Languages { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? ProfilePictureBase64 { get; set; }
    public string? ProfilePictureFileName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Language { get; set; }
    public string? Timezone { get; set; }
    public string? ResumeUrl { get; set; }
}

public class TutorProfileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? Headline { get; set; }
    public decimal HourlyRate { get; set; }
    public int YearsOfExperience { get; set; }
    public string? Education { get; set; }
    public string? Certifications { get; set; }
    public string? Skills { get; set; }
    public string? Languages { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int TotalSessions { get; set; }
    public string VerificationStatus { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public string? Language { get; set; }
    public string? Timezone { get; set; }
}

public class UploadTutorImageRequest
{
    public IFormFile File { get; set; } = null!;
}
