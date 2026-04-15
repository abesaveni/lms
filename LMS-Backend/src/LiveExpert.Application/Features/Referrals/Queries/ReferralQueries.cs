using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using MediatR;

namespace LiveExpert.Application.Features.Referrals.Queries;

// Get Referral Code Query (Students)
public class GetReferralCodeQuery : IRequest<Result<ReferralCodeDto>>
{
}

public class ReferralCodeDto
{
    public string ReferralCode { get; set; } = string.Empty;
    public int TotalReferrals { get; set; }
    public int PendingReferrals { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal PendingEarnings { get; set; }
}

public class GetReferralCodeQueryHandler : IRequestHandler<GetReferralCodeQuery, Result<ReferralCodeDto>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<StudentProfile> _studentRepository;
    private readonly IRepository<ReferralProgram> _referralRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetReferralCodeQueryHandler(
        IRepository<User> userRepository,
        IRepository<StudentProfile> studentRepository,
        IRepository<ReferralProgram> referralRepository,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _studentRepository = studentRepository;
        _referralRepository = referralRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ReferralCodeDto>> Handle(GetReferralCodeQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<ReferralCodeDto>.FailureResult("UNAUTHORIZED", "User not authenticated");

        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (user == null)
            return Result<ReferralCodeDto>.FailureResult("NOT_FOUND", "User not found");
        if (user.Role != Domain.Enums.UserRole.Student)
            return Result<ReferralCodeDto>.FailureResult("FORBIDDEN", "This endpoint is for students only");

        var studentProfile = await _studentRepository.FirstOrDefaultAsync(
            s => s.UserId == userId.Value, cancellationToken);
        if (studentProfile == null)
            return Result<ReferralCodeDto>.FailureResult("NOT_FOUND", "Student profile not found");

        var referrals = await _referralRepository.FindAsync(
            r => r.ReferrerId == userId.Value && !r.IsTutorReferral, cancellationToken);
        var list = referrals.ToList();

        var dto = new ReferralCodeDto
        {
            ReferralCode = studentProfile.ReferralCode,
            TotalReferrals = list.Count,
            PendingReferrals = list.Count(r => r.ReferralBonusPaidAt == null),
            TotalEarnings = list.Where(r => r.ReferralBonusPaidAt != null).Sum(r => r.RewardCredits),
            PendingEarnings = list.Where(r => r.ReferralBonusPaidAt == null).Sum(r => r.RewardCredits)
        };

        return Result<ReferralCodeDto>.SuccessResult(dto);
    }
}

// Get Tutor Referral Code Query (Feature 18)
public class GetTutorReferralCodeQuery : IRequest<Result<TutorReferralCodeDto>>
{
}

public class TutorReferralCodeDto
{
    public string TutorReferralCode { get; set; } = string.Empty;
    public int TotalTutorReferrals { get; set; }
    public int CompletedTutorReferrals { get; set; }
    public decimal TotalEarnings { get; set; }
}

public class GetTutorReferralCodeQueryHandler : IRequestHandler<GetTutorReferralCodeQuery, Result<TutorReferralCodeDto>>
{
    private readonly IRepository<TutorProfile> _tutorRepository;
    private readonly IRepository<ReferralProgram> _referralRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetTutorReferralCodeQueryHandler(
        IRepository<TutorProfile> tutorRepository,
        IRepository<ReferralProgram> referralRepository,
        ICurrentUserService currentUserService)
    {
        _tutorRepository = tutorRepository;
        _referralRepository = referralRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<TutorReferralCodeDto>> Handle(GetTutorReferralCodeQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<TutorReferralCodeDto>.FailureResult("UNAUTHORIZED", "User not authenticated");

        var tutor = await _tutorRepository.FirstOrDefaultAsync(t => t.UserId == userId.Value, cancellationToken);
        if (tutor == null)
            return Result<TutorReferralCodeDto>.FailureResult("NOT_FOUND", "Tutor profile not found");

        var referrals = await _referralRepository.FindAsync(
            r => r.ReferrerId == userId.Value && r.IsTutorReferral, cancellationToken);
        var list = referrals.ToList();

        return Result<TutorReferralCodeDto>.SuccessResult(new TutorReferralCodeDto
        {
            TutorReferralCode = tutor.TutorReferralCode ?? "",
            TotalTutorReferrals = list.Count,
            CompletedTutorReferrals = list.Count(r => r.ReferralBonusPaidAt != null),
            TotalEarnings = list.Where(r => r.ReferralBonusPaidAt != null).Sum(r => r.RewardCredits)
        });
    }
}

// Get Referral Stats Query
public class GetReferralStatsQuery : IRequest<Result<ReferralStatsDto>>
{
}

public class ReferralStatsDto
{
    public int TotalReferrals { get; set; }
    public int SuccessfulReferrals { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal PendingEarnings { get; set; }
    public List<MonthlyReferralDto> MonthlyReferrals { get; set; } = new();
}

public class MonthlyReferralDto
{
    public string Month { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Earnings { get; set; }
}

public class GetReferralStatsQueryHandler : IRequestHandler<GetReferralStatsQuery, Result<ReferralStatsDto>>
{
    private readonly IRepository<ReferralProgram> _referralRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetReferralStatsQueryHandler(
        IRepository<ReferralProgram> referralRepository,
        ICurrentUserService currentUserService)
    {
        _referralRepository = referralRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ReferralStatsDto>> Handle(GetReferralStatsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<ReferralStatsDto>.FailureResult("UNAUTHORIZED", "User not authenticated");

        var referrals = await _referralRepository.FindAsync(
            r => r.ReferrerId == userId.Value && !r.IsTutorReferral, cancellationToken);
        var list = referrals.ToList();

        // Monthly breakdown for last 6 months
        var monthly = list
            .GroupBy(r => new { r.CreatedAt.Year, r.CreatedAt.Month })
            .OrderByDescending(g => g.Key.Year).ThenByDescending(g => g.Key.Month)
            .Take(6)
            .Select(g => new MonthlyReferralDto
            {
                Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                Count = g.Count(),
                Earnings = g.Where(r => r.ReferralBonusPaidAt != null).Sum(r => r.RewardCredits)
            }).ToList();

        var dto = new ReferralStatsDto
        {
            TotalReferrals = list.Count,
            SuccessfulReferrals = list.Count(r => r.ReferralBonusPaidAt != null),
            TotalEarnings = list.Where(r => r.ReferralBonusPaidAt != null).Sum(r => r.RewardCredits),
            PendingEarnings = list.Where(r => r.ReferralBonusPaidAt == null).Sum(r => r.RewardCredits),
            MonthlyReferrals = monthly
        };

        return Result<ReferralStatsDto>.SuccessResult(dto);
    }
}

// Get Referral History Query
public class GetReferralHistoryQuery : IRequest<Result<List<ReferralHistoryDto>>>
{
}

public class ReferralHistoryDto
{
    public Guid Id { get; set; }
    public string ReferredUserName { get; set; } = string.Empty;
    public DateTime ReferredAt { get; set; }
    public decimal Reward { get; set; }
    public bool IsRewardClaimed { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public bool IsExpired { get; set; }
}

public class GetReferralHistoryQueryHandler : IRequestHandler<GetReferralHistoryQuery, Result<List<ReferralHistoryDto>>>
{
    private readonly IRepository<ReferralProgram> _referralRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetReferralHistoryQueryHandler(
        IRepository<ReferralProgram> referralRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService)
    {
        _referralRepository = referralRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<ReferralHistoryDto>>> Handle(GetReferralHistoryQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<List<ReferralHistoryDto>>.FailureResult("UNAUTHORIZED", "User not authenticated");

        var referrals = await _referralRepository.FindAsync(r => r.ReferrerId == userId.Value, cancellationToken);
        var now = DateTime.UtcNow;

        var dtos = new List<ReferralHistoryDto>();
        foreach (var referral in referrals.OrderByDescending(r => r.CreatedAt))
        {
            var referredUser = await _userRepository.GetByIdAsync(referral.ReferredUserId, cancellationToken);
            var isExpired = referral.ReferralBonusPaidAt == null && referral.ExpiresAt.HasValue && referral.ExpiresAt.Value < now;
            dtos.Add(new ReferralHistoryDto
            {
                Id = referral.Id,
                ReferredUserName = referredUser?.Username ?? "Unknown",
                ReferredAt = referral.CreatedAt,
                Reward = referral.RewardCredits,
                IsRewardClaimed = referral.ReferralBonusPaidAt != null,
                Status = referral.ReferralBonusPaidAt != null ? "Bonus Paid"
                    : isExpired ? "Expired"
                    : "Awaiting First Payment",
                ExpiresAt = referral.ExpiresAt,
                IsExpired = isExpired
            });
        }

        return Result<List<ReferralHistoryDto>>.SuccessResult(dtos);
    }
}

// Feature 19: Referral Leaderboard — top referrers this month
public class GetReferralLeaderboardQuery : IRequest<Result<List<ReferralLeaderboardDto>>>
{
}

public class ReferralLeaderboardDto
{
    public int Rank { get; set; }
    public string Username { get; set; } = string.Empty;
    public int ReferralsThisMonth { get; set; }
    public int TotalReferrals { get; set; }
    public decimal TotalEarnings { get; set; }
}

public class GetReferralLeaderboardQueryHandler : IRequestHandler<GetReferralLeaderboardQuery, Result<List<ReferralLeaderboardDto>>>
{
    private readonly IRepository<ReferralProgram> _referralRepository;
    private readonly IRepository<User> _userRepository;

    public GetReferralLeaderboardQueryHandler(
        IRepository<ReferralProgram> referralRepository,
        IRepository<User> userRepository)
    {
        _referralRepository = referralRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<List<ReferralLeaderboardDto>>> Handle(GetReferralLeaderboardQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // Only count student referrals (not tutor-to-tutor)
        var allReferrals = await _referralRepository.FindAsync(
            r => r.ReferralBonusPaidAt != null && !r.IsTutorReferral, cancellationToken);

        var grouped = allReferrals
            .GroupBy(r => r.ReferrerId)
            .Select(g => new
            {
                ReferrerId = g.Key,
                ThisMonth = g.Count(r => r.ReferralBonusPaidAt >= monthStart),
                Total = g.Count(),
                Earnings = g.Sum(r => r.RewardCredits)
            })
            .OrderByDescending(x => x.ThisMonth)
            .ThenByDescending(x => x.Total)
            .Take(10)
            .ToList();

        var result = new List<ReferralLeaderboardDto>();
        int rank = 1;
        foreach (var entry in grouped)
        {
            var user = await _userRepository.GetByIdAsync(entry.ReferrerId, cancellationToken);
            result.Add(new ReferralLeaderboardDto
            {
                Rank = rank++,
                Username = user?.Username ?? "Unknown",
                ReferralsThisMonth = entry.ThisMonth,
                TotalReferrals = entry.Total,
                TotalEarnings = entry.Earnings
            });
        }

        return Result<List<ReferralLeaderboardDto>>.SuccessResult(result);
    }
}
