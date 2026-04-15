using LiveExpert.Application.Features.Coupons;
using LiveExpert.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers;

/// <summary>Coupon code management — admins manage, students validate before booking.</summary>
[ApiController]
[Route("api/coupons")]
public class CouponsController : BaseController
{
    public CouponsController(IMediator mediator) : base(mediator) { }

    // ── Admin: Create Coupon ───────────────────────────────────────────────────
    /// <summary>
    /// Admin creates a promo coupon.
    /// POST /api/coupons
    /// Body: { code, discountType (0=Percentage, 1=Flat), discountValue, maxUses?, expiresAt?, ... }
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCouponCommand cmd)
        => HandleResult(await _mediator.Send(cmd));

    // ── Admin: Toggle Coupon Active ────────────────────────────────────────────
    /// <summary>PUT /api/coupons/{id}/toggle — enable or disable a coupon.</summary>
    [HttpPut("{id:guid}/toggle")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Toggle(Guid id, [FromBody] ToggleCouponBody body)
        => HandleResult(await _mediator.Send(new ToggleCouponCommand { CouponId = id, IsActive = body.IsActive }));

    // ── Student/Public: Validate Coupon Before Booking ─────────────────────────
    /// <summary>
    /// GET /api/coupons/validate?code=WELCOME20&amp;orderAmount=1500&amp;tutorId=...
    /// Returns discount amount + final price (or invalid reason).
    /// </summary>
    [HttpGet("validate")]
    [Authorize]
    public async Task<IActionResult> Validate([FromQuery] string code, [FromQuery] decimal orderAmount, [FromQuery] Guid? tutorId)
        => HandleResult(await _mediator.Send(new ValidateCouponQuery { Code = code, OrderAmount = orderAmount, TutorId = tutorId }));
}

public record ToggleCouponBody(bool IsActive);
