using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LiveExpert.API.Controllers.Student;

/// <summary>
/// Student profile management (Matching Tutor's professional pattern)
/// </summary>
[Authorize(Roles = "Student")]
[Route("api/student/profile")]
[ApiController]
public class StudentProfileController : ControllerBase
{
    private readonly IRepository<StudentProfile> _studentRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<StudentProfileController> _logger;

    public StudentProfileController(
        IRepository<StudentProfile> studentRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        ILogger<StudentProfileController> logger)
    {
        _studentRepository = studentRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    /// <summary>
    /// Get current student profile
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null || userId == Guid.Empty)
            return Unauthorized();

        var studentProfile = await _studentRepository.FirstOrDefaultAsync(
            s => s.UserId == userId.Value, 
            cancellationToken);

        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);

        if (user == null) return NotFound();

        var profile = new StudentProfileDto
        {
            UserId = userId.Value,
            Username = user.Username,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Bio = user.Bio,
            DateOfBirth = user.DateOfBirth?.ToString("yyyy-MM-dd"),
            Location = user.Location,
            ProfilePictureUrl = user.ProfileImageUrl,
            PreferredSubjects = studentProfile?.PreferredSubjects,
            Language = user.Language,
            Timezone = user.Timezone
        };

        return Ok(Result<StudentProfileDto>.SuccessResult(profile));
    }

    /// <summary>
    /// Update student profile (Self-healing & Professional pattern)
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateStudentProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null || userId == Guid.Empty)
            return Unauthorized();

        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (user == null)
            return NotFound(Result<bool>.FailureResult("NOT_FOUND", "User not found"));

        // FAIL-SAFE: Ensure Columns exist (Professional Self-Healing)
        try {
            var db = _unitOfWork as dynamic;
            using var connection = (db._context as Microsoft.EntityFrameworkCore.DbContext).Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open) await connection.OpenAsync();
            using var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = "PRAGMA table_info(Users)";
            var cols = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var reader = await checkCmd.ExecuteReaderAsync()) { while (await reader.ReadAsync()) cols.Add(reader["name"].ToString()!); }
            foreach (var col in new[] { "Bio", "DateOfBirth", "Location", "Language", "Timezone" }) {
                if (!cols.Contains(col)) {
                    using var alterCmd = connection.CreateCommand();
                    alterCmd.CommandText = $"ALTER TABLE Users ADD COLUMN {col} TEXT NULL";
                    await alterCmd.ExecuteNonQueryAsync();
                }
            }
        } catch { }

        // Update core User fields
        user.FirstName = request.FirstName ?? user.FirstName;
        user.LastName = request.LastName ?? user.LastName;
        user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
        user.Bio = request.Bio ?? user.Bio;
        user.Location = request.Location ?? user.Location;
        user.Language = request.Language ?? user.Language;
        user.Timezone = request.Timezone ?? user.Timezone;
        
        if (DateTime.TryParse(request.DateOfBirth, out var dob))
            user.DateOfBirth = dob;

        // Handle Base64 Image Upload (Matching Tutor's professional pattern)
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
                var contentType = "image/jpeg";
                
                var fileUrl = await _fileStorageService.UploadFileAsync(stream, fileName, contentType);
                user.ProfileImageUrl = fileUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process image for student {UserId}", userId);
            }
        }
        else if (request.ProfilePictureUrl != null)
        {
            user.ProfileImageUrl = request.ProfilePictureUrl;
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);
        
        // Update Student Profile specific fields
        var student = await _studentRepository.FirstOrDefaultAsync(s => s.UserId == userId.Value, cancellationToken);
        if (student != null)
        {
            if (request.Bio != null) student.LearningGoals = request.Bio;
            student.UpdatedAt = DateTime.UtcNow;
            await _studentRepository.UpdateAsync(student, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(Result<bool>.SuccessResult(true));
    }
}

public class UpdateStudentProfileRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Bio { get; set; }
    public string? DateOfBirth { get; set; }
    public string? Location { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? ProfilePictureBase64 { get; set; }
    public string? ProfilePictureFileName { get; set; }
    public string? Language { get; set; }
    public string? Timezone { get; set; }
}

public class StudentProfileDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? DateOfBirth { get; set; }
    public string? Location { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? PreferredSubjects { get; set; }
    public string? Language { get; set; }
    public string? Timezone { get; set; }
}
