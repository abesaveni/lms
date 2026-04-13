using System;
using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;

namespace LiveExpert.API.Controllers;

[Authorize]
[Route("api/tutors")]
[ApiController]
public class TutorFollowersController : ControllerBase
{
    private readonly IRepository<TutorFollower> _followerRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<TutorProfile> _tutorRepository;
    private readonly IRepository<Subject> _subjectRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly IUnitOfWork _unitOfWork;

    public TutorFollowersController(
        IRepository<TutorFollower> followerRepository,
        IRepository<User> userRepository,
        IRepository<TutorProfile> tutorRepository,
        IRepository<Subject> subjectRepository,
        ICurrentUserService currentUserService,
        INotificationDispatcher notificationDispatcher,
        IUnitOfWork unitOfWork)
    {
        _followerRepository = followerRepository;
        _userRepository = userRepository;
        _tutorRepository = tutorRepository;
        _subjectRepository = subjectRepository;
        _currentUserService = currentUserService;
        _notificationDispatcher = notificationDispatcher;
        _unitOfWork = unitOfWork;
    }

    [HttpPost("{tutorId}/follow")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> FollowTutor(Guid tutorId, CancellationToken cancellationToken)
    {
        var studentId = _currentUserService.UserId;
        if (!studentId.HasValue)
        {
            return Unauthorized(Result.FailureResult("UNAUTHORIZED", "User not authenticated"));
        }

        var tutor = await _userRepository.GetByIdAsync(tutorId, cancellationToken);
        if (tutor == null || tutor.Role != UserRole.Tutor)
        {
            return NotFound(Result.FailureResult("NOT_FOUND", "Tutor not found"));
        }

        var existing = await _followerRepository.FirstOrDefaultAsync(
            f => f.TutorId == tutorId && f.StudentId == studentId.Value,
            cancellationToken);

        if (existing != null)
        {
            return Ok(Result.SuccessResult("Already following"));
        }

        var follower = new TutorFollower
        {
            Id = Guid.NewGuid(),
            TutorId = tutorId,
            StudentId = studentId.Value,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _followerRepository.AddAsync(follower, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var student = await _userRepository.GetByIdAsync(studentId.Value, cancellationToken);
        if (student != null)
        {
            var tutorName = $"{tutor.FirstName} {tutor.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(tutorName))
            {
                tutorName = tutor.Username;
            }

            var studentName = $"{student.FirstName} {student.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(studentName))
            {
                studentName = student.Username;
            }

            await _notificationDispatcher.SendAsync(new NotificationDispatchRequest
            {
                UserId = tutor.Id,
                Category = NotificationCategory.EngagementReminders,
                IsTransactional = true,
                Title = "New follower",
                Message = $"{studentName} started following you.",
                ActionUrl = "/tutor/profile",
                WhatsAppTo = tutor.WhatsAppNumber ?? tutor.PhoneNumber,
                WhatsAppMessage = NotificationTemplates.NewFollowerTutorWhatsApp(tutorName, studentName),
                SendEmail = false,
                SendInApp = true
            }, cancellationToken);
        }

        return Ok(Result.SuccessResult("Followed"));
    }

    [HttpDelete("{tutorId}/follow")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> UnfollowTutor(Guid tutorId, CancellationToken cancellationToken)
    {
        var studentId = _currentUserService.UserId;
        if (!studentId.HasValue)
        {
            return Unauthorized(Result.FailureResult("UNAUTHORIZED", "User not authenticated"));
        }

        var existing = await _followerRepository.FirstOrDefaultAsync(
            f => f.TutorId == tutorId && f.StudentId == studentId.Value,
            cancellationToken);

        if (existing == null)
        {
            return Ok(Result.SuccessResult("Not following"));
        }

        await _followerRepository.DeleteAsync(existing, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(Result.SuccessResult("Unfollowed"));
    }

    [HttpGet("{tutorId}/follow-status")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetFollowStatus(Guid tutorId, CancellationToken cancellationToken)
    {
        var studentId = _currentUserService.UserId;
        if (!studentId.HasValue)
        {
            return Unauthorized(Result.FailureResult("UNAUTHORIZED", "User not authenticated"));
        }

        var isFollowing = await _followerRepository.AnyAsync(
            f => f.TutorId == tutorId && f.StudentId == studentId.Value,
            cancellationToken);

        return Ok(Result<bool>.SuccessResult(isFollowing));
    }

    [HttpGet("{tutorId}/followers/count")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFollowerCount(Guid tutorId, CancellationToken cancellationToken)
    {
        var count = await _followerRepository.CountAsync(f => f.TutorId == tutorId, cancellationToken);
        return Ok(Result<int>.SuccessResult(count));
    }

}
