using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using MediatR;

namespace LiveExpert.Application.Features.Referrals.Queries;

// Get Referral Code Query
public class GetReferralCodeQuery : IRequest<Result<ReferralCodeDto>>
{
}

public class ReferralCodeDto
{
    public string ReferralCode { get; set; } = string.Empty;
    public int TotalReferrals { get; set; }
    public decimal TotalEarnings { get; set; }
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
            return Result<ReferralCodeDto>.FailureResult("FORBIDDEN", "Referral program is for students only");

        var studentProfile = await _studentRepository.FirstOrDefaultAsync(
            s => s.UserId == userId.Value, cancellationToken);
        if (studentProfile == null)
            return Result<ReferralCodeDto>.FailureResult("NOT_FOUND", "Student profile not found");

        var referrals = await _referralRepository.FindAsync(
            r => r.ReferrerId == userId.Value, cancellationToken);

        var dto = new ReferralCodeDto
        {
            ReferralCode = studentProfile.ReferralCode,
            TotalReferrals = referrals.Count(),
            TotalEarnings = referrals.Where(r => r.ReferralBonusPaidAt != null).Sum(r => r.RewardCredits)
        };

        return Result<ReferralCodeDto>.SuccessResult(dto);
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
            r => r.ReferrerId == userId.Value, cancellationToken);

        var dto = new ReferralStatsDto
        {
            TotalReferrals = referrals.Count(),
            SuccessfulReferrals = referrals.Count(r => r.ReferralBonusPaidAt != null),
            TotalEarnings = referrals.Where(r => r.ReferralBonusPaidAt != null).Sum(r => r.RewardCredits),
            PendingEarnings = referrals.Where(r => r.ReferralBonusPaidAt == null).Sum(r => r.RewardCredits),
            MonthlyReferrals = new List<MonthlyReferralDto>()
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

        var dtos = new List<ReferralHistoryDto>();
        foreach (var referral in referrals)
        {
            var referredUser = await _userRepository.GetByIdAsync(referral.ReferredUserId, cancellationToken);
            dtos.Add(new ReferralHistoryDto
            {
                Id = referral.Id,
                ReferredUserName = referredUser?.Username ?? "Unknown",
                ReferredAt = referral.CreatedAt,
                Reward = referral.RewardCredits,
                IsRewardClaimed = referral.ReferralBonusPaidAt != null,
                Status = referral.ReferralBonusPaidAt != null ? "Bonus Paid" : "Pending Booking"
            });
        }

        return Result<List<ReferralHistoryDto>>.SuccessResult(dtos);
    }
}
