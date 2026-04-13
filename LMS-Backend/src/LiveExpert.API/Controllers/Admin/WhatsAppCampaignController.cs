using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using LiveExpert.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace LiveExpert.API.Controllers.Admin;

/// <summary>
/// WhatsApp Campaign Management (Admin only)
/// </summary>
[Route("api/admin/campaigns")]
[ApiController]
[Authorize(Roles = "Admin")]
[EnableCors("AllowAllDev")]
public class WhatsAppCampaignController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWhatsAppService _whatsAppService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<WhatsAppCampaignController> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public WhatsAppCampaignController(
        ApplicationDbContext context,
        IWhatsAppService whatsAppService,
        ICurrentUserService currentUserService,
        ILogger<WhatsAppCampaignController> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _context = context;
        _whatsAppService = whatsAppService;
        _currentUserService = currentUserService;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    /// Handle OPTIONS preflight request
    /// </summary>
    [HttpOptions]
    public IActionResult Options()
    {
        return Ok();
    }

    /// <summary>
    /// Get all campaigns
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<WhatsAppCampaignResponse>>), 200)]
    public async Task<IActionResult> GetAllCampaigns()
    {
        // Set CORS headers manually to ensure they're always present
        SetCorsHeaders();
        
        // Log authentication status for debugging
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            _logger.LogWarning("GetAllCampaigns: User not authenticated. Has Authorization header: {HasAuth}, User: {User}", 
                Request.Headers.ContainsKey("Authorization"), User.Identity?.Name ?? "anonymous");
            return Unauthorized(Result<List<WhatsAppCampaignResponse>>.FailureResult("UNAUTHORIZED", "User not authenticated. Please log in again."));
        }

        // Try multiple ways to get role claim
        var userRole = User.FindFirst("role")?.Value 
            ?? User.FindFirst(ClaimTypes.Role)?.Value
            ?? User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value
            ?? _currentUserService.Role;
        
        if (string.IsNullOrEmpty(userRole) || userRole != "Admin")
        {
            _logger.LogWarning("GetAllCampaigns: User {UserId} does not have Admin role. Current role: {Role}. Available claims: {Claims}", 
                _currentUserService.UserId, userRole ?? "null", 
                string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));
            return Forbid();
        }
        
        try
        {
            // Check if context is available
            if (_context == null)
            {
                _logger.LogWarning("ApplicationDbContext is null");
                return Ok(Result<List<WhatsAppCampaignResponse>>.SuccessResult(new List<WhatsAppCampaignResponse>()));
            }

            // Check if DbSet exists and is accessible
            if (_context.WhatsAppCampaigns == null)
            {
                _logger.LogWarning("WhatsAppCampaigns DbSet is null");
                return Ok(Result<List<WhatsAppCampaignResponse>>.SuccessResult(new List<WhatsAppCampaignResponse>()));
            }

            // Try to query campaigns
            List<WhatsAppCampaign> campaigns = new List<WhatsAppCampaign>();
            try
            {
                // Use a simple query first to test database connection
                campaigns = await _context.WhatsAppCampaigns
                    .AsNoTracking()
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                // Database connection or table doesn't exist
                _logger.LogWarning(dbEx, "Database error accessing WhatsAppCampaigns - table might not exist: {Message}", dbEx.Message);
                return Ok(Result<List<WhatsAppCampaignResponse>>.SuccessResult(new List<WhatsAppCampaignResponse>()));
            }
            catch (InvalidOperationException invalidOpEx)
            {
                // Table might not be configured
                _logger.LogWarning(invalidOpEx, "WhatsAppCampaigns table configuration issue: {Message}", invalidOpEx.Message);
                return Ok(Result<List<WhatsAppCampaignResponse>>.SuccessResult(new List<WhatsAppCampaignResponse>()));
            }
            catch (Microsoft.Data.Sqlite.SqliteException sqliteEx)
            {
                // SQLite specific errors (table doesn't exist, etc.)
                _logger.LogWarning(sqliteEx, "SQLite error accessing WhatsAppCampaigns: {Message}", sqliteEx.Message);
                return Ok(Result<List<WhatsAppCampaignResponse>>.SuccessResult(new List<WhatsAppCampaignResponse>()));
            }
            catch (System.Data.Common.DbException dbEx)
            {
                // Generic database errors
                _logger.LogWarning(dbEx, "Database error accessing WhatsAppCampaigns: {Message}", dbEx.Message);
                return Ok(Result<List<WhatsAppCampaignResponse>>.SuccessResult(new List<WhatsAppCampaignResponse>()));
            }
            catch (NullReferenceException nullEx)
            {
                // Null reference errors
                _logger.LogWarning(nullEx, "Null reference error accessing WhatsAppCampaigns: {Message}", nullEx.Message);
                return Ok(Result<List<WhatsAppCampaignResponse>>.SuccessResult(new List<WhatsAppCampaignResponse>()));
            }
            catch (Exception dbEx)
            {
                // Catch any other database-related exception
                _logger.LogWarning(dbEx, "Unexpected error accessing WhatsAppCampaigns: {Message}", dbEx.Message);
                return Ok(Result<List<WhatsAppCampaignResponse>>.SuccessResult(new List<WhatsAppCampaignResponse>()));
            }

            // Safely map campaigns to response
            var response = new List<WhatsAppCampaignResponse>();
            try
            {
                response = campaigns.Select(c => new WhatsAppCampaignResponse
                {
                    Id = c.Id.ToString(),
                    Name = c.Name ?? string.Empty,
                    Message = c.MessageTemplate ?? string.Empty,
                    TargetAudience = c.TargetAudience.ToString(),
                    Status = c.Status.ToString(),
                    TotalRecipients = c.TotalRecipients,
                    SentCount = c.SentCount,
                    DeliveredCount = c.DeliveredCount,
                    FailedCount = c.FailedCount,
                    ScheduledAt = c.ScheduledAt,
                    CreatedAt = c.CreatedAt
                }).ToList();
            }
            catch (Exception mapEx)
            {
                _logger.LogWarning(mapEx, "Error mapping campaigns to response: {Message}", mapEx.Message);
                // Return empty list if mapping fails
                response = new List<WhatsAppCampaignResponse>();
            }

            return Ok(Result<List<WhatsAppCampaignResponse>>.SuccessResult(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all campaigns: {Message}", ex.Message);
            // Always return 200 with empty list to avoid CORS issues with 500 errors
            return Ok(Result<List<WhatsAppCampaignResponse>>.SuccessResult(new List<WhatsAppCampaignResponse>()));
        }
    }

    /// <summary>
    /// Set CORS headers manually to ensure they're always present
    /// </summary>
    private void SetCorsHeaders()
    {
        try
        {
            var origin = Request.Headers["Origin"].ToString();
            var allowedOrigins = new[] { "http://localhost:3000", "http://localhost:5173", "http://localhost:5175", "https://liveexpert.ai" };
            
            if (!string.IsNullOrEmpty(origin) && (allowedOrigins.Contains(origin) || origin.Contains("localhost")))
            {
                Response.Headers["Access-Control-Allow-Origin"] = origin;
            }
            else
            {
                Response.Headers["Access-Control-Allow-Origin"] = "http://localhost:5173";
            }
            
            Response.Headers["Access-Control-Allow-Credentials"] = "true";
            Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, PATCH, OPTIONS";
            Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization, X-Requested-With";
        }
        catch
        {
            // Ignore errors setting headers
        }
    }

    /// <summary>
    /// Create WhatsApp campaign
    /// </summary>
    [HttpPost("whatsapp")]
    [ProducesResponseType(typeof(Result<WhatsAppCampaignResponse>), 201)]
    public async Task<IActionResult> CreateCampaign([FromForm] CreateCampaignRequest request)
    {
        // Set CORS headers manually to ensure they're always present
        SetCorsHeaders();
        
        try
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            {
                return Unauthorized(Result<WhatsAppCampaignResponse>.FailureResult("UNAUTHORIZED", "User not authenticated."));
            }

            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(Result<WhatsAppCampaignResponse>.FailureResult("VALIDATION_ERROR", "Name and Message are required."));
            }

            // Parse target audience
            TargetAudience targetAudience;
            List<string> phoneNumbers = new();

            if (request.TargetAudience?.ToLower() == "dataset" && request.PhoneNumberFile != null)
            {
                // Parse file to extract phone numbers
                phoneNumbers = await ParsePhoneNumberFileAsync(request.PhoneNumberFile);
                if (phoneNumbers.Count == 0)
                {
                    return BadRequest(Result<WhatsAppCampaignResponse>.FailureResult("VALIDATION_ERROR", "No valid phone numbers found in the uploaded file."));
                }
                targetAudience = TargetAudience.Specific;
            }
            else if (request.TargetAudience?.ToLower() == "selected" && !string.IsNullOrEmpty(request.PhoneNumbers))
            {
                // Parse JSON array of phone numbers
                try
                {
                    phoneNumbers = System.Text.Json.JsonSerializer.Deserialize<List<string>>(request.PhoneNumbers) ?? new();
                }
                catch
                {
                    return BadRequest(Result<WhatsAppCampaignResponse>.FailureResult("VALIDATION_ERROR", "Invalid phone numbers format."));
                }
                targetAudience = TargetAudience.Specific;
            }
            else
            {
                // Map string to enum
                targetAudience = request.TargetAudience?.ToLower() switch
                {
                    "all" => TargetAudience.AllUsers,
                    "students" => TargetAudience.Students,
                    "tutors" => TargetAudience.Tutors,
                    _ => TargetAudience.AllUsers
                };
            }

            // Get recipient count based on target audience
            int totalRecipients = 0;
            if (targetAudience == TargetAudience.Specific)
            {
                totalRecipients = phoneNumbers.Count;
            }
            else
            {
                var query = _context.Users.AsQueryable();
                if (targetAudience == TargetAudience.Students)
                {
                    query = query.Where(u => u.Role == UserRole.Student);
                }
                else if (targetAudience == TargetAudience.Tutors)
                {
                    query = query.Where(u => u.Role == UserRole.Tutor);
                }
                totalRecipients = await query.CountAsync();
            }

            // Verify user exists
            var creator = await _context.Users.FindAsync(userId.Value);
            if (creator == null)
            {
                return BadRequest(Result<WhatsAppCampaignResponse>.FailureResult("VALIDATION_ERROR", "User not found."));
            }

            // Create campaign
            var campaign = new WhatsAppCampaign
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                MessageTemplate = request.Message,
                TargetAudience = targetAudience,
                Status = CampaignStatus.Scheduled,
                TotalRecipients = totalRecipients,
                SentCount = 0,
                DeliveredCount = 0,
                FailedCount = 0,
                CreatedBy = userId.Value,
                ScheduledAt = request.ScheduledAt.HasValue ? request.ScheduledAt.Value.ToUniversalTime() : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Set the Creator navigation property - EF Core will handle the foreign key
            campaign.Creator = creator;

            try
            {
                // Add campaign
                var entry = _context.WhatsAppCampaigns.Add(campaign);
                
                // Explicitly set the CreatorId foreign key to match CreatedBy
                // This ensures the foreign key constraint is satisfied
                entry.Property("CreatorId").CurrentValue = userId.Value;
                
                await _context.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error saving campaign: {Message}", dbEx.Message);
                var innerEx = dbEx.InnerException;
                var errorMessage = innerEx != null ? innerEx.Message : dbEx.Message;
                _logger.LogError(innerEx, "Inner exception: {InnerMessage}", innerEx?.Message);
                
                // Log the full exception details for debugging
                _logger.LogError(dbEx, "Full exception: {Exception}", dbEx.ToString());
                
                // Return detailed error message
                return Ok(Result<WhatsAppCampaignResponse>.FailureResult("DATABASE_ERROR", $"An error occurred while saving the campaign: {errorMessage}"));
            }
            catch (Exception saveEx)
            {
                _logger.LogError(saveEx, "Error saving campaign: {Message}", saveEx.Message);
                _logger.LogError(saveEx, "Full exception: {Exception}", saveEx.ToString());
                return Ok(Result<WhatsAppCampaignResponse>.FailureResult("SERVER_ERROR", $"An error occurred: {saveEx.Message}"));
            }

            // Start sending campaign in background (fire and forget)
            // Use service scope factory to create a new scope for the background task
            _ = Task.Run(async () =>
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var scopedContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var scopedWhatsAppService = scope.ServiceProvider.GetRequiredService<IWhatsAppService>();
                await SendCampaignAsync(campaign.Id, phoneNumbers, targetAudience, scopedContext, scopedWhatsAppService);
            });

            var response = new WhatsAppCampaignResponse
            {
                Id = campaign.Id.ToString(),
                Name = campaign.Name,
                Message = campaign.MessageTemplate,
                TargetAudience = campaign.TargetAudience.ToString(),
                Status = campaign.Status.ToString(),
                TotalRecipients = campaign.TotalRecipients,
                SentCount = campaign.SentCount,
                DeliveredCount = campaign.DeliveredCount,
                FailedCount = campaign.FailedCount,
                ScheduledAt = campaign.ScheduledAt,
                CreatedAt = campaign.CreatedAt
            };

            return CreatedAtAction(nameof(GetAllCampaigns), new { id = campaign.Id }, Result<WhatsAppCampaignResponse>.SuccessResult(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating campaign: {Message}", ex.Message);
            // Return 200 with error result to ensure CORS headers are set
            return Ok(Result<WhatsAppCampaignResponse>.FailureResult("SERVER_ERROR", $"An error occurred: {ex.Message}"));
        }
    }

    /// <summary>
    /// Get campaign by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Result<WhatsAppCampaignResponse>), 200)]
    public async Task<IActionResult> GetCampaign(Guid id)
    {
        try
        {
            var campaign = await _context.WhatsAppCampaigns
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (campaign == null)
            {
                return NotFound(Result<WhatsAppCampaignResponse>.FailureResult("NOT_FOUND", "Campaign not found."));
            }

            var response = new WhatsAppCampaignResponse
            {
                Id = campaign.Id.ToString(),
                Name = campaign.Name,
                Message = campaign.MessageTemplate,
                TargetAudience = campaign.TargetAudience.ToString(),
                Status = campaign.Status.ToString(),
                TotalRecipients = campaign.TotalRecipients,
                SentCount = campaign.SentCount,
                DeliveredCount = campaign.DeliveredCount,
                FailedCount = campaign.FailedCount,
                ScheduledAt = campaign.ScheduledAt,
                CreatedAt = campaign.CreatedAt
            };

            return Ok(Result<WhatsAppCampaignResponse>.SuccessResult(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting campaign {CampaignId}: {Message}", id, ex.Message);
            // Return 200 with error result to ensure CORS headers are set
            return Ok(Result<WhatsAppCampaignResponse>.FailureResult("SERVER_ERROR", $"An error occurred: {ex.Message}"));
        }
    }

    private async Task<List<string>> ParsePhoneNumberFileAsync(IFormFile file)
    {
        var phoneNumbers = new List<string>();
        var extension = Path.GetExtension(file.FileName).ToLower();

        try
        {
            if (extension == ".csv")
            {
                using var reader = new StreamReader(file.OpenReadStream());
                string? line;
                bool isFirstLine = true;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // Skip header row
                    if (isFirstLine && (line.ToLower().Contains("phone") || line.ToLower().Contains("number")))
                    {
                        isFirstLine = false;
                        continue;
                    }
                    isFirstLine = false;

                    // Parse CSV line
                    var values = line.Split(',').Select(v => v.Trim().Replace("\"", "")).ToList();
                    foreach (var value in values)
                    {
                        var phone = value.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
                        if (!string.IsNullOrEmpty(phone) && (phone.StartsWith("+") || phone.All(char.IsDigit)))
                        {
                            if (!phone.StartsWith("+"))
                            {
                                phone = "+" + phone;
                            }
                            if (phone.Length >= 10 && phone.Length <= 15)
                            {
                                phoneNumbers.Add(phone);
                            }
                        }
                    }
                }
            }
            else
            {
                // For Excel files, we'd need a library like EPPlus or ClosedXML
                // For now, return error suggesting CSV
                _logger.LogWarning("Excel file format not yet supported. Please use CSV format.");
                throw new NotSupportedException("Excel files are not yet supported. Please convert to CSV format.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing phone number file");
            throw;
        }

        // Remove duplicates
        return phoneNumbers.Distinct().ToList();
    }

    private async Task SendCampaignAsync(Guid campaignId, List<string> phoneNumbers, TargetAudience targetAudience, ApplicationDbContext context, IWhatsAppService whatsAppService)
    {
        try
        {
            var campaign = await context.WhatsAppCampaigns.FindAsync(campaignId);
            if (campaign == null) return;

            // Update status to Sending
            campaign.Status = CampaignStatus.Sending;
            campaign.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            // Get phone numbers based on target audience
            List<string> recipients = new();
            if (targetAudience == TargetAudience.Specific)
            {
                recipients = phoneNumbers;
            }
            else
            {
                var query = context.Users.AsQueryable();
                if (targetAudience == TargetAudience.Students)
                {
                    query = query.Where(u => u.Role == UserRole.Student);
                }
                else if (targetAudience == TargetAudience.Tutors)
                {
                    query = query.Where(u => u.Role == UserRole.Tutor);
                }

                recipients = await query
                    .Where(u => !string.IsNullOrEmpty(u.PhoneNumber))
                    .Select(u => u.PhoneNumber!)
                    .ToListAsync();
            }

            // Send messages
            int sent = 0;
            int delivered = 0;
            int failed = 0;

            foreach (var phoneNumber in recipients)
            {
                try
                {
                    await whatsAppService.SendMessageAsync(phoneNumber, campaign.MessageTemplate);
                    sent++;
                    
                    // Note: Delivery status comes from WhatsApp webhooks, not immediately
                    // For now, we mark as "sent" (accepted by API) but not "delivered"
                    // Delivery status should be updated via webhook when WhatsApp confirms delivery
                    // TODO: Implement webhook handler to update delivery status
                    await Task.Delay(100); // Rate limiting
                    
                    // Don't mark as delivered immediately - wait for webhook confirmation
                    // For now, only mark as sent (accepted by API)
                    // delivered++; // Commented out - delivery comes from webhook
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send message to {PhoneNumber}: {Message}", phoneNumber, ex.Message);
                    failed++;
                }

                // Update progress periodically
                if ((sent + failed) % 10 == 0)
                {
                    // Re-fetch campaign to get latest state
                    campaign = await context.WhatsAppCampaigns.FindAsync(campaignId);
                    if (campaign != null)
                    {
                        campaign.SentCount = sent;
                        campaign.DeliveredCount = delivered;
                        campaign.FailedCount = failed;
                        campaign.UpdatedAt = DateTime.UtcNow;
                        await context.SaveChangesAsync();
                    }
                }
            }

            // Final update - re-fetch campaign to ensure we have latest state
            campaign = await context.WhatsAppCampaigns.FindAsync(campaignId);
            if (campaign != null)
            {
                campaign.SentCount = sent;
                campaign.DeliveredCount = delivered; // Will be updated via webhook when messages are actually delivered
                campaign.FailedCount = failed;
                campaign.Status = failed == recipients.Count ? CampaignStatus.Failed : CampaignStatus.Sent;
                campaign.CompletedAt = DateTime.UtcNow;
                campaign.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();
                
                _logger.LogInformation("Campaign {CampaignId} completed. Sent: {Sent}, Delivered: {Delivered} (via webhook), Failed: {Failed}", 
                    campaignId, sent, delivered, failed);
            }

            _logger.LogInformation("Campaign {CampaignId} completed. Sent: {Sent}, Delivered: {Delivered}, Failed: {Failed}", 
                campaignId, sent, delivered, failed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending campaign {CampaignId}", campaignId);
            try
            {
                var campaign = await context.WhatsAppCampaigns.FindAsync(campaignId);
                if (campaign != null)
                {
                    campaign.Status = CampaignStatus.Failed;
                    campaign.UpdatedAt = DateTime.UtcNow;
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception updateEx)
            {
                _logger.LogError(updateEx, "Failed to update campaign status to Failed for {CampaignId}", campaignId);
            }
        }
    }
}

public class CreateCampaignRequest
{
    public string Name { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? TargetAudience { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public IFormFile? PhoneNumberFile { get; set; }
    public string? PhoneNumbers { get; set; } // JSON array string
}

public class WhatsAppCampaignResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalRecipients { get; set; }
    public int SentCount { get; set; }
    public int DeliveredCount { get; set; }
    public int FailedCount { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

