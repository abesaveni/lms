using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers;

[ApiController]
[Route("api/bonus-points")]
public class BonusPointsController : ControllerBase
{
    private readonly IRepository<BonusPoint> _bonusPointRepository;
    private readonly ICurrentUserService _currentUserService;

    public BonusPointsController(
        IRepository<BonusPoint> bonusPointRepository,
        ICurrentUserService currentUserService)
    {
        _bonusPointRepository = bonusPointRepository;
        _currentUserService = currentUserService;
    }

    [HttpGet("summary")]
    [Authorize]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized(Result.FailureResult("UNAUTHORIZED", "User not authenticated"));
        }

        var points = await _bonusPointRepository.FindAsync(p => p.UserId == userId.Value, cancellationToken);
        var ordered = points.OrderByDescending(p => p.CreatedAt).ToList();

        var items = ordered.Select(p => new
        {
            Id = p.Id.ToString(),
            Points = p.Points,
            Reason = p.Reason.ToString(),
            CreatedAt = p.CreatedAt
        }).ToList();

        return Ok(Result<object>.SuccessResult(new
        {
            TotalPoints = ordered.Sum(p => p.Points),
            Items = items
        }));
    }
}
