using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using MediatR;

namespace LiveExpert.Application.Features.Admin.Queries;

// Admin Dashboard Query
public class GetAdminDashboardQuery : IRequest<Result<AdminDashboardDto>>
{
}

public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int TotalStudents { get; set; }
    public int TotalTutors { get; set; }
    public int TotalAdmins { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalSessions { get; set; }
    public int CompletedSessions { get; set; }
    public int UpcomingSessions { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal TotalWithdrawals { get; set; }
    public int PendingTutorVerifications { get; set; }
    public int PendingWithdrawals { get; set; }
    public List<RecentUserDto> RecentUsers { get; set; } = new();
    public List<RecentSessionDto> RecentSessions { get; set; } = new();
}

public class RecentUserDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class RecentSessionDto
{
    public Guid SessionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string TutorName { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class GetAdminDashboardQueryHandler : IRequestHandler<GetAdminDashboardQuery, Result<AdminDashboardDto>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<TutorProfile> _tutorProfileRepository;
    private readonly IRepository<WithdrawalRequest> _withdrawalRepository;
    private readonly IRepository<Payment> _paymentRepository;

    public GetAdminDashboardQueryHandler(
        IRepository<User> userRepository,
        IRepository<Session> sessionRepository,
        IRepository<TutorProfile> tutorProfileRepository,
        IRepository<WithdrawalRequest> withdrawalRepository,
        IRepository<Payment> paymentRepository)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _tutorProfileRepository = tutorProfileRepository;
        _withdrawalRepository = withdrawalRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<Result<AdminDashboardDto>> Handle(GetAdminDashboardQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.FindAsync(u => true, cancellationToken);
        var sessions = await _sessionRepository.FindAsync(s => true, cancellationToken);
        var pendingTutors = await _tutorProfileRepository.FindAsync(
            t => t.VerificationStatus == VerificationStatus.Pending, cancellationToken);
        var pendingWithdrawals = await _withdrawalRepository.FindAsync(
            w => w.Status == WithdrawalStatus.Pending, cancellationToken);
        var payments = await _paymentRepository.FindAsync(
            p => p.Status == PaymentStatus.Success, cancellationToken);
        var withdrawals = await _withdrawalRepository.FindAsync(
            w => w.Status == WithdrawalStatus.Approved || w.Status == WithdrawalStatus.Completed, cancellationToken);

        var recentUsers = users.OrderByDescending(u => u.CreatedAt).Take(5)
            .Select(u => new RecentUserDto
            {
                UserId = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role.ToString(),
                CreatedAt = u.CreatedAt
            }).ToList();

        var recentSessions = new List<RecentSessionDto>();
        foreach (var session in sessions.OrderByDescending(s => s.CreatedAt).Take(5))
        {
            var tutor = await _userRepository.GetByIdAsync(session.TutorId, cancellationToken);
            recentSessions.Add(new RecentSessionDto
            {
                SessionId = session.Id,
                Title = session.Title,
                TutorName = tutor?.Username ?? "Unknown",
                ScheduledAt = session.ScheduledAt,
                Status = session.Status.ToString()
            });
        }

        var dto = new AdminDashboardDto
        {
            TotalUsers = users.Count(),
            TotalStudents = users.Count(u => u.Role == UserRole.Student),
            TotalTutors = users.Count(u => u.Role == UserRole.Tutor),
            TotalAdmins = users.Count(u => u.Role == UserRole.Admin),
            ActiveUsers = users.Count(u => u.IsActive),
            TotalSessions = sessions.Count(),
            CompletedSessions = sessions.Count(s => s.Status == SessionStatus.Completed),
            UpcomingSessions = sessions.Count(s => s.Status == SessionStatus.Scheduled && s.ScheduledAt > DateTime.UtcNow),
            TotalRevenue = payments.Sum(p => p.TotalAmount),
            TotalEarnings = payments.Sum(p => p.BaseAmount),
            TotalWithdrawals = withdrawals.Sum(w => w.Amount),
            PendingTutorVerifications = pendingTutors.Count(),
            PendingWithdrawals = pendingWithdrawals.Count(),
            RecentUsers = recentUsers,
            RecentSessions = recentSessions
        };

        return Result<AdminDashboardDto>.SuccessResult(dto);
    }
}
