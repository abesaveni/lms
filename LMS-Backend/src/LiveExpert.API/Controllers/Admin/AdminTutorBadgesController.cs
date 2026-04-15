using LiveExpert.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LiveExpert.Domain.Entities;

namespace LiveExpert.API.Controllers.Admin;

/// <summary>Admin-only endpoints for tutor trust badges (background check, verified, featured).</summary>
[ApiController]
[Route("api/admin/tutors")]
[Authorize(Roles = "Admin")]
public class AdminTutorBadgesController : BaseController
{
    private readonly IRepository<TutorProfile> _tutorProfiles;
    private readonly IUnitOfWork _uow;

    public AdminTutorBadgesController(IMediator mediator, IRepository<TutorProfile> tutorProfiles, IUnitOfWork uow)
        : base(mediator)
    {
        _tutorProfiles = tutorProfiles;
        _uow = uow;
    }

    /// <summary>
    /// Toggle background check badge for a tutor.
    /// PUT /api/admin/tutors/{tutorId}/background-check
    /// Body: { "checked": true }
    /// </summary>
    [HttpPut("{tutorId:guid}/background-check")]
    public async Task<IActionResult> SetBackgroundCheck(Guid tutorId, [FromBody] BackgroundCheckBody body)
    {
        var profile = await _tutorProfiles.FirstOrDefaultAsync(p => p.UserId == tutorId);
        if (profile == null) return NotFound(new { message = "Tutor profile not found." });

        profile.HasBackgroundCheck = body.Checked;
        profile.BackgroundCheckDate = body.Checked ? DateTime.UtcNow : null;
        profile.UpdatedAt = DateTime.UtcNow;

        await _tutorProfiles.UpdateAsync(profile);
        await _uow.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            tutorId,
            hasBackgroundCheck = profile.HasBackgroundCheck,
            backgroundCheckDate = profile.BackgroundCheckDate
        });
    }
}

public record BackgroundCheckBody(bool Checked);
