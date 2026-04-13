using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers.Admin;

/// <summary>
/// Admin bulk messaging endpoints
/// </summary>
[Route("api/admin/messages")]
[Authorize(Roles = "Admin")]
[ApiController]
public class AdminMessagesController : ControllerBase
{
    private readonly IRepository<User> _userRepository;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public AdminMessagesController(
        IRepository<User> userRepository,
        INotificationService notificationService,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _notificationService = notificationService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get all users for messaging (Admin only)
    /// </summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(Result<List<UserDto>>), 200)]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FullName = !string.IsNullOrEmpty(u.FirstName) && !string.IsNullOrEmpty(u.LastName) 
                    ? $"{u.FirstName} {u.LastName}" 
                    : u.Username,
                Role = u.Role.ToString()
            }).ToList();

            return Ok(Result<List<UserDto>>.SuccessResult(userDtos));
        }
        catch (Exception ex)
        {
            return BadRequest(Result<List<UserDto>>.FailureResult("SERVER_ERROR", $"Failed to fetch users: {ex.Message}"));
        }
    }

    /// <summary>
    /// Send bulk message to selected users
    /// </summary>
    [HttpPost("send-bulk")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SendBulkMessage([FromBody] BulkMessageRequest request)
    {
        if (request.UserIds == null || request.UserIds.Count == 0)
        {
            return BadRequest(Result.FailureResult("INVALID_REQUEST", "Please select at least one user"));
        }

        if (string.IsNullOrEmpty(request.Subject) || string.IsNullOrEmpty(request.Message))
        {
            return BadRequest(Result.FailureResult("INVALID_REQUEST", "Subject and message are required"));
        }

        try
        {
            var successCount = 0;
            var failCount = 0;

            foreach (var userId in request.UserIds)
            {
                try
                {
                    var user = await _userRepository.GetByIdAsync(userId);
                    if (user == null) continue;

                    // Send in-app notification
                    await _notificationService.SendNotificationAsync(
                        userId,
                        request.Subject,
                        request.Message,
                        NotificationType.NewMessage,
                        null,
                        default);

                    // Send email notification
                    await _emailService.SendEmailAsync(
                        user.Email,
                        request.Subject,
                        request.Message,
                        true);

                    successCount++;
                }
                catch
                {
                    failCount++;
                }
            }

            return Ok(Result<object>.SuccessResult(new
            {
                SuccessCount = successCount,
                FailCount = failCount,
                TotalSent = successCount
            }));
        }
        catch (Exception ex)
        {
            return BadRequest(Result.FailureResult("SERVER_ERROR", $"Failed to send messages: {ex.Message}"));
        }
    }
}

public class BulkMessageRequest
{
    public List<Guid> UserIds { get; set; } = new();
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

