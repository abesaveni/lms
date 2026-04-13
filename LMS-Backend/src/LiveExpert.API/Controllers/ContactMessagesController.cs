using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using LiveExpert.Application.Common;

namespace LiveExpert.API.Controllers;

/// <summary>
/// Contact messages and support endpoints
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ContactMessagesController : ControllerBase
{
    private readonly IRepository<ContactMessage> _contactRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ContactMessagesController(
        IRepository<ContactMessage> contactRepository,
        IUnitOfWork unitOfWork)
    {
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get contact message subjects/categories
    /// </summary>
    [HttpGet("subjects")]
    [ProducesResponseType(typeof(Result<List<string>>), 200)]
    public IActionResult GetSubjects()
    {
        var subjects = new List<string>
        {
            "General Inquiry",
            "Technical Support",
            "Billing Question",
            "Feature Request",
            "Bug Report",
            "Partnership Opportunity",
            "Other"
        };

        return Ok(Result<List<string>>.SuccessResult(subjects));
    }

    /// <summary>
    /// Submit a contact message
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Result<Guid>), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateContactMessage(
        [FromBody] CreateContactMessageRequest request,
        CancellationToken cancellationToken)
    {
        var contactMessage = new ContactMessage
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Subject = request.Subject,
            Message = request.Message,
            PhoneNumber = request.PhoneNumber,
            Status = Domain.Enums.ContactMessageStatus.New,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _contactRepository.AddAsync(contactMessage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(CreateContactMessage),
            new { id = contactMessage.Id },
            Result<Guid>.SuccessResult(contactMessage.Id));
    }
}

public class CreateContactMessageRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}
