using LiveExpert.API.Controllers;
using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace LiveExpert.API.Controllers;

[Authorize]
[Route("api/users")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<TutorProfile> _tutorRepository;
    private readonly IRepository<StudentProfile> _studentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;

    public UsersController(
        IRepository<User> userRepository,
        IRepository<TutorProfile> tutorRepository,
        IRepository<StudentProfile> studentRepository,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _tutorRepository = tutorRepository;
        _studentRepository = studentRepository;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Unauthorized(Result.FailureResult("UNAUTHORIZED", "User not authenticated"));

        var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user == null)
            return NotFound(Result.FailureResult("NOT_FOUND", "User not found"));

        var userBio = user.Bio;
        object? profileData = null;

        if (user.Role == Domain.Enums.UserRole.Tutor)
        {
            var tutor = await _tutorRepository.FirstOrDefaultAsync(t => t.UserId == userId.Value);
            if (tutor != null)
            {
                userBio = userBio ?? tutor.Bio;
                profileData = new 
                {
                    Bio = tutor.Bio,
                    tutor.Headline,
                    tutor.YearsOfExperience,
                    tutor.HourlyRate,
                    VerificationStatus = tutor.VerificationStatus.ToString(),
                    tutor.AverageRating,
                    tutor.TotalSessions,
                    tutor.Skills,
                    tutor.IsProfileComplete
                };
            }
        }
        else if (user.Role == Domain.Enums.UserRole.Student)
        {
            var student = await _studentRepository.FirstOrDefaultAsync(s => s.UserId == userId.Value);
            if (student != null)
            {
                userBio = userBio ?? student.LearningGoals;
                profileData = new
                {
                    student.ReferralCode,
                    student.PreferredSubjects,
                    Bio = student.LearningGoals,
                    student.LearningGoals
                };
            }
        }

        return Ok(Result<object>.SuccessResult(new
        {
            user.Id,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            Role = user.Role.ToString(),
            user.ProfileImageUrl, 
            user.DateOfBirth,
            user.Location,
            user.IsEmailVerified,
            user.IsPhoneVerified,
            user.IsWhatsAppVerified,
            Bio = userBio,
            PhoneNumber = user.PhoneNumber,
            user.CreatedAt,
            StudentProfile = user.Role == Domain.Enums.UserRole.Student ? profileData : null,
            TutorProfile = user.Role == Domain.Enums.UserRole.Tutor ? profileData : null
        }));


    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Unauthorized(Result.FailureResult("UNAUTHORIZED", "User not authenticated"));

        var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user == null)
            return NotFound(Result.FailureResult("NOT_FOUND", "User not found"));

        // Update user fields
        if (request.FirstName != null)
            user.FirstName = request.FirstName;
            
        if (request.LastName != null)
            user.LastName = request.LastName;

        if (request.PhoneNumber != null)
            user.PhoneNumber = request.PhoneNumber;
        
        if (request.WhatsAppNumber != null)
            user.WhatsAppNumber = request.WhatsAppNumber;

        // Update Bio, Date of Birth and Location
        if (request.Bio != null)
            user.Bio = request.Bio;

        if (request.DateOfBirth.HasValue)
            user.DateOfBirth = request.DateOfBirth.Value;
            
        if (request.Location != null)
            user.Location = request.Location;

        // Handle Base64 Image Upload (matching tutor pattern)
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
            catch (Exception)
            {
                // Log error but continue
            }
        }

        // Keep Student/Tutor specific profiles in sync if needed
        if (user.Role == Domain.Enums.UserRole.Student && request.Bio != null)
        {
            var student = await _studentRepository.FirstOrDefaultAsync(s => s.UserId == userId.Value);
            if (student != null)
            {
                student.LearningGoals = request.Bio;
                student.UpdatedAt = DateTime.UtcNow;
                await _studentRepository.UpdateAsync(student);
            }
        }
        else if (user.Role == Domain.Enums.UserRole.Tutor && request.Bio != null)
        {
            var tutor = await _tutorRepository.FirstOrDefaultAsync(t => t.UserId == userId.Value);
            if (tutor != null)
            {
                tutor.Bio = request.Bio;
                tutor.UpdatedAt = DateTime.UtcNow;
                await _tutorRepository.UpdateAsync(tutor);
            }
        }

        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return Ok(Result.SuccessResult("Profile updated successfully"));
    }

    [HttpPost("profile/image")]
    public async Task<IActionResult> UploadProfileImage([FromForm] UploadProfileImageRequest request)
    {
        var file = request.File;
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Unauthorized(Result.FailureResult("UNAUTHORIZED", "User not authenticated"));

        if (file == null || file.Length == 0)
            return BadRequest(Result.FailureResult("INVALID_FILE", "No file uploaded"));

        var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user == null)
            return NotFound(Result.FailureResult("NOT_FOUND", "User not found"));

        using var stream = file.OpenReadStream();
        var fileUrl = await _fileStorageService.UploadFileAsync(stream, file.FileName, file.ContentType);

        user.ProfileImageUrl = fileUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return Ok(Result<string>.SuccessResult(fileUrl));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return NotFound(Result.FailureResult("NOT_FOUND", "User not found"));

        return Ok(Result<object>.SuccessResult(new
        {
            user.Id,
            user.Username,
            user.ProfileImageUrl,
            Role = user.Role.ToString()
        }));
    }
}

public class UpdateProfileRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? WhatsAppNumber { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePictureBase64 { get; set; }
    public string? ProfilePictureFileName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Location { get; set; }
}

public class UploadProfileImageRequest
{
    public IFormFile File { get; set; } = null!;
}

[Route("api/[controller]")]
[ApiController]
public class TutorsController : ControllerBase
{
    private readonly IRepository<TutorProfile> _tutorRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Subject> _subjectRepository;
    private readonly IRepository<TutorFollower> _followerRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public TutorsController(
        IRepository<TutorProfile> tutorRepository,
        IRepository<User> userRepository,
        IRepository<Subject> subjectRepository,
        IRepository<TutorFollower> followerRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _tutorRepository = tutorRepository;
        _userRepository = userRepository;
        _subjectRepository = subjectRepository;
        _followerRepository = followerRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    [Authorize(Roles = "Tutor")]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateTutorProfile([FromBody] UpdateTutorProfileRequest request)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Unauthorized(Result.FailureResult("UNAUTHORIZED", "User not authenticated"));

        var tutorProfile = await _tutorRepository.FirstOrDefaultAsync(t => t.UserId == userId.Value);
        if (tutorProfile == null)
            return NotFound(Result.FailureResult("NOT_FOUND", "Tutor profile not found"));

        tutorProfile.Bio = request.Bio;
        tutorProfile.Headline = request.Headline;
        if (request.HourlyRate.HasValue)
            tutorProfile.HourlyRate = request.HourlyRate.Value;
        
        if (request.YearsOfExperience.HasValue)
            tutorProfile.YearsOfExperience = request.YearsOfExperience.Value;
            
        if (!string.IsNullOrEmpty(request.Skills))
            tutorProfile.Skills = request.Skills;
            
        tutorProfile.UpdatedAt = DateTime.UtcNow;

        await _tutorRepository.UpdateAsync(tutorProfile);
        await _unitOfWork.SaveChangesAsync();

        return Ok(Result.SuccessResult("Tutor profile updated successfully"));
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetTutors([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var allTutors = await _tutorRepository.GetAllAsync();
            var approvedTutors = allTutors
                .Where(t => t.VerificationStatus == Domain.Enums.VerificationStatus.Approved)
                .ToList();

            var tutors = approvedTutors
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Get user information for each tutor
            var tutorDtos = new List<object>();
            foreach (var tutor in tutors)
            {
                var user = await _userRepository.GetByIdAsync(tutor.UserId);
                if (user != null && user.IsActive)
                {
                    tutorDtos.Add(new
                    {
                        Id = tutor.Id,
                        UserId = user.Id,
                        Name = user.Username,
                        Email = user.Email,
                        Bio = tutor.Bio,
                        Headline = tutor.Headline,
                        HourlyRate = tutor.HourlyRate,
                        YearsOfExperience = tutor.YearsOfExperience,
                        AverageRating = tutor.AverageRating,
                        TotalReviews = tutor.TotalReviews,
                        TotalSessions = tutor.TotalSessions,
                        FollowerCount = await _followerRepository.CountAsync(f => f.TutorId == user.Id),
                        VerificationStatus = tutor.VerificationStatus.ToString(),
                        ProfileImage = user.ProfileImageUrl,
                        Subjects = string.IsNullOrEmpty(tutor.Skills) 
                            ? new string[] { } 
                            : tutor.Skills.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray()
                    });
                }
            }

            return Ok(Result<object>.SuccessResult(new
            {
                Items = tutorDtos,
                Pagination = new
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalRecords = approvedTutors.Count
                }
            }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, Result<object>.FailureResult("SERVER_ERROR", $"An error occurred: {ex.Message}"));
        }
    }

    [AllowAnonymous]
    [HttpGet("{id}/profile")]
    public async Task<IActionResult> GetTutorProfile(Guid id)
    {
        var tutor = await _tutorRepository.GetByIdAsync(id) 
                    ?? await _tutorRepository.FirstOrDefaultAsync(t => t.UserId == id);
                    
        if (tutor == null)
            return NotFound(Result.FailureResult("NOT_FOUND", "Tutor profile not found"));

        var user = await _userRepository.GetByIdAsync(tutor.UserId);
        var followerCount = user != null ? await _followerRepository.CountAsync(f => f.TutorId == user.Id) : 0;
        
        return Ok(Result<object>.SuccessResult(new
        {
            Id = tutor.Id,
            UserId = user?.Id,
            Name = user?.Username ?? (user?.FirstName != null ? user.FirstName + " " + user.LastName : user?.Username),
            Email = user?.Email,
            Bio = tutor.Bio,
            Headline = tutor.Headline,
            HourlyRate = tutor.HourlyRate,
            YearsOfExperience = tutor.YearsOfExperience,
            AverageRating = tutor.AverageRating,
            TotalReviews = tutor.TotalReviews,
            TotalSessions = tutor.TotalSessions,
            VerificationStatus = tutor.VerificationStatus.ToString(),
            ProfileImage = user?.ProfileImageUrl,
            FollowerCount = followerCount,
            Location = user?.Location,
            Subjects = string.IsNullOrEmpty(tutor.Skills) 
                ? new string[] { } 
                : tutor.Skills.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray()
        }));
    }

    [AllowAnonymous]
    [HttpGet("subjects")]
    public async Task<IActionResult> GetSubjects(CancellationToken cancellationToken)
    {
        var subjects = await _subjectRepository.FindAsync(s => s.IsActive, cancellationToken);
        var results = subjects
            .OrderBy(s => s.Name)
            .Select(s => new
            {
                Id = s.Id,
                Name = s.Name
            })
            .ToList();

        return Ok(Result<object>.SuccessResult(results));
    }
}

public class UpdateTutorProfileRequest
{
    public string? Bio { get; set; }
    public string? Headline { get; set; }
    public decimal? HourlyRate { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? Skills { get; set; }
}

/// <summary>
/// Public platform statistics — no auth required (used on landing page)
/// </summary>
[Route("api/platform")]
[ApiController]
[AllowAnonymous]
public class PlatformStatsController : ControllerBase
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<TutorProfile> _tutorRepository;
    private readonly IRepository<Session> _sessionRepository;

    public PlatformStatsController(
        IRepository<User> userRepository,
        IRepository<TutorProfile> tutorRepository,
        IRepository<Session> sessionRepository)
    {
        _userRepository = userRepository;
        _tutorRepository = tutorRepository;
        _sessionRepository = sessionRepository;
    }

    /// <summary>
    /// Returns live student + tutor counts, active session count, and current/next session details
    /// for the landing page dashboard card — no auth required
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var studentCount = await _userRepository.CountAsync(u => u.Role == UserRole.Student && u.IsActive);
        var tutorCount   = await _tutorRepository.CountAsync(t => t.VerificationStatus == VerificationStatus.Approved);

        // Count sessions currently live
        var liveSessionCount = await _sessionRepository.CountAsync(
            s => s.Status == SessionStatus.Live || s.Status == SessionStatus.InProgress);

        // Count upcoming sessions in the next 24 hours
        var cutoff = DateTime.UtcNow.AddHours(24);
        var upcomingSessionCount = await _sessionRepository.CountAsync(
            s => s.Status == SessionStatus.Scheduled && s.ScheduledAt > DateTime.UtcNow && s.ScheduledAt <= cutoff);

        // Fetch the spotlight session: first live, otherwise soonest upcoming
        Session? spotlight = null;
        var liveSessions = await _sessionRepository.FindAsync(
            s => s.Status == SessionStatus.Live || s.Status == SessionStatus.InProgress);
        spotlight = liveSessions.OrderBy(s => s.ScheduledAt).FirstOrDefault();

        if (spotlight == null)
        {
            var upcoming = await _sessionRepository.FindAsync(
                s => s.Status == SessionStatus.Scheduled && s.ScheduledAt > DateTime.UtcNow && s.ScheduledAt <= cutoff);
            spotlight = upcoming.OrderBy(s => s.ScheduledAt).FirstOrDefault();
        }

        object? spotlightDto = null;
        if (spotlight != null)
        {
            var tutor = await _userRepository.GetByIdAsync(spotlight.TutorId);
            var tutorName = tutor != null
                ? $"{tutor.FirstName} {tutor.LastName}".Trim()
                : "Expert Tutor";
            if (string.IsNullOrWhiteSpace(tutorName)) tutorName = tutor?.Username ?? "Expert Tutor";

            spotlightDto = new
            {
                sessionId   = spotlight.Id,
                title       = spotlight.Title,
                tutorName,
                scheduledAt = spotlight.ScheduledAt,
                duration    = spotlight.Duration,
                isLive      = spotlight.Status == SessionStatus.Live || spotlight.Status == SessionStatus.InProgress,
            };
        }

        return Ok(new
        {
            studentCount,
            tutorCount,
            liveSessionCount,
            upcomingSessionCount,
            spotlightSession = spotlightDto
        });
    }
}
