using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using MediatR;

namespace LiveExpert.Application.Features.VirtualClassroom.Commands;

// Start Virtual Classroom Command
public class StartVirtualClassroomCommand : IRequest<Result<VirtualClassroomDto>>
{
}

public class VirtualClassroomDto
{
    public Guid SessionId { get; set; }
    public string MeetingLink { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int RemainingMinutesToday { get; set; }
}

public class StartVirtualClassroomCommandHandler : IRequestHandler<StartVirtualClassroomCommand, Result<VirtualClassroomDto>>
{
    private readonly IRepository<VirtualClassroomSession> _virtualClassroomRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGoogleCalendarService _googleCalendarService;
    private readonly IUnitOfWork _unitOfWork;
    private const int DAILY_LIMIT_MINUTES = 15;

    public StartVirtualClassroomCommandHandler(
        IRepository<VirtualClassroomSession> virtualClassroomRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService,
        IGoogleCalendarService googleCalendarService,
        IUnitOfWork unitOfWork)
    {
        _virtualClassroomRepository = virtualClassroomRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _googleCalendarService = googleCalendarService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<VirtualClassroomDto>> Handle(StartVirtualClassroomCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result<VirtualClassroomDto>.FailureResult("UNAUTHORIZED", "User not authenticated");
        }

        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (user == null || user.Role != Domain.Enums.UserRole.Tutor)
        {
            return Result<VirtualClassroomDto>.FailureResult("FORBIDDEN", "Only tutors can start virtual classrooms");
        }

        // Check daily usage limit (15 minutes per day)
        var today = DateTime.UtcNow.Date;
        var todaySessions = (await _virtualClassroomRepository.FindAsync(
            vc => vc.TutorId == userId.Value && 
                  vc.StartedAt.Date == today && 
                  vc.Status == "Active",
            cancellationToken)).ToList();

        var totalMinutesUsedToday = todaySessions.Sum(s => (int)(s.ExpiresAt - s.StartedAt).TotalMinutes);
        var remainingMinutes = DAILY_LIMIT_MINUTES - totalMinutesUsedToday;

        if (remainingMinutes <= 0)
        {
            return Result<VirtualClassroomDto>.FailureResult("LIMIT_EXCEEDED", 
                $"You have used all {DAILY_LIMIT_MINUTES} minutes for today. Please try again tomorrow.");
        }

        // Create Google Meet link
        var meetingLink = await _googleCalendarService.CreateMeetingLinkAsync(
            $"Virtual Classroom - {user.FirstName} {user.LastName}",
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(Math.Min(remainingMinutes, 15)),
            cancellationToken);

        // Create virtual classroom session
        var session = new VirtualClassroomSession
        {
            Id = Guid.NewGuid(),
            TutorId = userId.Value,
            MeetingLink = meetingLink,
            StartedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(Math.Min(remainingMinutes, 15)),
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _virtualClassroomRepository.AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<VirtualClassroomDto>.SuccessResult(new VirtualClassroomDto
        {
            SessionId = session.Id,
            MeetingLink = meetingLink,
            StartedAt = session.StartedAt,
            ExpiresAt = session.ExpiresAt,
            RemainingMinutesToday = remainingMinutes - (int)(session.ExpiresAt - session.StartedAt).TotalMinutes
        });
    }
}

// Get Active Virtual Classrooms (for students to see)
public class GetActiveVirtualClassroomsQuery : IRequest<Result<List<ActiveVirtualClassroomDto>>>
{
}

public class ActiveVirtualClassroomDto
{
    public Guid SessionId { get; set; }
    public Guid TutorId { get; set; }
    public string TutorName { get; set; } = string.Empty;
    public string TutorImage { get; set; } = string.Empty;
    public string MeetingLink { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int RemainingMinutes { get; set; }
}

public class GetActiveVirtualClassroomsQueryHandler : IRequestHandler<GetActiveVirtualClassroomsQuery, Result<List<ActiveVirtualClassroomDto>>>
{
    private readonly IRepository<VirtualClassroomSession> _virtualClassroomRepository;
    private readonly IRepository<User> _userRepository;

    public GetActiveVirtualClassroomsQueryHandler(
        IRepository<VirtualClassroomSession> virtualClassroomRepository,
        IRepository<User> userRepository)
    {
        _virtualClassroomRepository = virtualClassroomRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<List<ActiveVirtualClassroomDto>>> Handle(GetActiveVirtualClassroomsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var activeSessions = (await _virtualClassroomRepository.FindAsync(
            vc => vc.Status == "Active" && vc.ExpiresAt > now,
            cancellationToken)).ToList();

        var result = new List<ActiveVirtualClassroomDto>();

        foreach (var session in activeSessions)
        {
            var tutor = await _userRepository.GetByIdAsync(session.TutorId, cancellationToken);
            if (tutor != null)
            {
                var remainingMinutes = (int)(session.ExpiresAt - now).TotalMinutes;
                result.Add(new ActiveVirtualClassroomDto
                {
                    SessionId = session.Id,
                    TutorId = session.TutorId,
                    TutorName = $"{tutor.FirstName} {tutor.LastName}",
                    TutorImage = tutor.ProfileImageUrl ?? "",
                    MeetingLink = session.MeetingLink,
                    StartedAt = session.StartedAt,
                    ExpiresAt = session.ExpiresAt,
                    RemainingMinutes = Math.Max(0, remainingMinutes)
                });
            }
        }

        return Result<List<ActiveVirtualClassroomDto>>.SuccessResult(result);
    }
}

// End Virtual Classroom Session
public class EndVirtualClassroomCommand : IRequest<Result>
{
    public Guid SessionId { get; set; }
}

public class EndVirtualClassroomCommandHandler : IRequestHandler<EndVirtualClassroomCommand, Result>
{
    private readonly IRepository<VirtualClassroomSession> _virtualClassroomRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public EndVirtualClassroomCommandHandler(
        IRepository<VirtualClassroomSession> virtualClassroomRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _virtualClassroomRepository = virtualClassroomRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(EndVirtualClassroomCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result.FailureResult("UNAUTHORIZED", "User not authenticated");
        }

        var session = await _virtualClassroomRepository.GetByIdAsync(request.SessionId, cancellationToken);
        if (session == null || session.TutorId != userId.Value)
        {
            return Result.FailureResult("NOT_FOUND", "Virtual classroom session not found");
        }

        session.Status = "Ended";
        session.UpdatedAt = DateTime.UtcNow;
        await _virtualClassroomRepository.UpdateAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.SuccessResult();
    }
}

