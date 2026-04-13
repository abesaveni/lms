using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Referrals.Queries;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers.Shared;

/// <summary>
/// Referral program endpoints
/// </summary>
[Route("api/shared/referrals")]
[Authorize(Roles = "Student")]
[ApiController]
public class ReferralsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReferralsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("code")]
    public async Task<IActionResult> GetReferralCode()
    {
        var result = await _mediator.Send(new GetReferralCodeQuery());
        return Ok(result);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _mediator.Send(new GetReferralStatsQuery());
        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var result = await _mediator.Send(new GetReferralHistoryQuery());
        return Ok(result);
    }
}

/// <summary>
/// Profile and tutor search endpoints
/// </summary>
[Route("api/shared")]
[ApiController]
public class ProfilesController : ControllerBase
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<TutorProfile> _tutorRepository;
    private readonly IRepository<StudentProfile> _studentRepository;
    private readonly IRepository<TutorFollower> _followerRepository;

    public ProfilesController(
        IRepository<User> userRepository,
        IRepository<TutorProfile> tutorRepository,
        IRepository<StudentProfile> studentRepository,
        IRepository<TutorFollower> followerRepository)
    {
        _userRepository = userRepository;
        _tutorRepository = tutorRepository;
        _studentRepository = studentRepository;
        _followerRepository = followerRepository;
    }

    [HttpGet("tutors")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTutors(CancellationToken cancellationToken)
    {
        var tutors = await _tutorRepository.FindAsync(t => true, cancellationToken);
        
        var results = new List<object>();
        foreach (var tutor in tutors)
        {
            var user = await _userRepository.GetByIdAsync(tutor.UserId, cancellationToken);
            if (user != null && user.IsActive)
            {
                results.Add(new
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
                    FollowerCount = await _followerRepository.CountAsync(f => f.TutorId == user.Id, cancellationToken),
                    VerificationStatus = tutor.VerificationStatus.ToString(),
                    ProfileImage = user.ProfileImageUrl
                });
            }
        }
        
        return Ok(Result<List<object>>.SuccessResult(results));
    }

    [HttpGet("tutors/search")]
    public async Task<IActionResult> SearchTutors([FromQuery] string? query, CancellationToken cancellationToken)
    {
        var tutors = await _tutorRepository.FindAsync(t => true, cancellationToken);
        
        var results = new List<object>();
        foreach (var tutor in tutors)
        {
            var user = await _userRepository.GetByIdAsync(tutor.UserId, cancellationToken);
            if (user != null)
            {
                results.Add(new
                {
                    Id = tutor.Id,
                    Name = user.Username,
                    Bio = tutor.Bio,
                    HourlyRate = tutor.HourlyRate,
                    AverageRating = tutor.AverageRating,
                    TotalReviews = tutor.TotalReviews,
                    FollowerCount = await _followerRepository.CountAsync(f => f.TutorId == user.Id, cancellationToken)
                });
            }
        }
        
        return Ok(Result<List<object>>.SuccessResult(results));
    }

    [HttpGet("tutors/{id}/profile")]
    public async Task<IActionResult> GetTutorProfile(Guid id, CancellationToken cancellationToken)
    {
        // Try searching by TutorId first, then by UserId as a fallback
        var tutor = await _tutorRepository.GetByIdAsync(id, cancellationToken) 
                    ?? await _tutorRepository.FirstOrDefaultAsync(t => t.UserId == id, cancellationToken);
                    
        if (tutor == null)
            return NotFound();

        var user = await _userRepository.GetByIdAsync(tutor.UserId, cancellationToken);
        
        var profile = new
        {
            Id = tutor.Id,
            UserId = user?.Id,
            Name = user?.Username,
            Email = user?.Email,
            Bio = tutor.Bio,
            Headline = tutor.Headline,
            HourlyRate = tutor.HourlyRate,
            YearsOfExperience = tutor.YearsOfExperience,
            AverageRating = tutor.AverageRating,
            TotalReviews = tutor.TotalReviews,
            TotalSessions = tutor.TotalSessions,
            VerificationStatus = tutor.VerificationStatus.ToString(),
            FollowerCount = user != null ? await _followerRepository.CountAsync(f => f.TutorId == user.Id, cancellationToken) : 0
        };
        
        return Ok(Result<object>.SuccessResult(profile));
    }

    [HttpGet("students/{id}/profile")]
    public async Task<IActionResult> GetStudentProfile(Guid id, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.FirstOrDefaultAsync(s => s.UserId == id, cancellationToken);
        if (student == null)
            return NotFound();

        var user = await _userRepository.GetByIdAsync(student.UserId, cancellationToken);
        
        var profile = new
        {
            Id = student.Id,
            Name = user?.Username,
            Email = user?.Email,
            LearningGoals = student.LearningGoals
        };
        
        return Ok(Result<object>.SuccessResult(profile));
    }

    [Authorize(Roles = "Tutor")]
    [HttpPost("tutor/profile/complete")]
    public async Task<IActionResult> CompleteTutorProfile([FromBody] CompleteTutorProfileRequest request, CancellationToken cancellationToken)
    {
        // Implementation would update tutor profile with additional details
        return Ok(Result<bool>.SuccessResult(true));
    }
}

public class CompleteTutorProfileRequest
{
    public string Bio { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public int YearsOfExperience { get; set; }
}

/// <summary>
/// Blog system endpoints
/// </summary>
[Route("api/shared/blogs")]
[ApiController]
public class BlogsController : ControllerBase
{
    private readonly IRepository<Blog> _blogRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BlogsController(
        IRepository<Blog> blogRepository,
        IRepository<User> userRepository,
        IUnitOfWork unitOfWork)
    {
        _blogRepository = blogRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetBlogs(CancellationToken cancellationToken)
    {
        var blogs = await _blogRepository.FindAsync(b => b.IsPublished, cancellationToken);
        
        var results = new List<object>();
        foreach (var blog in blogs)
        {
            var author = await _userRepository.GetByIdAsync(blog.AuthorId, cancellationToken);
            results.Add(new
            {
                Id = blog.Id,
                Title = blog.Title,
                Excerpt = blog.Content != null && blog.Content.Length > 200 
                    ? blog.Content.Substring(0, 200) 
                    : blog.Content ?? "",
                AuthorName = author?.Username,
                PublishedAt = blog.PublishedAt,
                ViewCount = blog.ViewCount
            });
        }
        
        return Ok(Result<List<object>>.SuccessResult(results));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBlog(Guid id, CancellationToken cancellationToken)
    {
        var blog = await _blogRepository.GetByIdAsync(id, cancellationToken);
        if (blog == null)
            return NotFound();

        var author = await _userRepository.GetByIdAsync(blog.AuthorId, cancellationToken);
        
        // Increment view count
        blog.ViewCount++;
        await _blogRepository.UpdateAsync(blog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        var result = new
        {
            Id = blog.Id,
            Title = blog.Title,
            Content = blog.Content,
            AuthorName = author?.Username,
            PublishedAt = blog.PublishedAt,
            ViewCount = blog.ViewCount,
            Tags = blog.Tags
        };
        
        return Ok(Result<object>.SuccessResult(result));
    }
}
