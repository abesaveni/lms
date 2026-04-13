using LiveExpert.Application.Features.ApiSettings.Commands;
using LiveExpert.Application.Features.ApiSettings.Handlers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers.Admin;

/// <summary>
/// Admin API for managing API settings (Google OAuth, Calendar, Email, etc.)
/// </summary>
[Route("api/admin/api-settings")]
[Authorize(Roles = "Admin")]
[ApiController]
public class ApiSettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ApiSettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all API settings for a provider
    /// </summary>
    [HttpGet("{provider}")]
    [ProducesResponseType(typeof(Dictionary<string, string>), 200)]
    public async Task<IActionResult> GetApiSettings(string provider)
    {
        var query = new GetApiSettingsCommand { Provider = provider };
        var result = await _mediator.Send(query);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    /// <summary>
    /// Update an API setting (updates both database and .env file)
    /// </summary>
    [HttpPut("{provider}/{keyName}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateApiSetting(
        string provider,
        string keyName,
        [FromBody] UpdateApiSettingRequest request)
    {
        var command = new UpdateApiSettingCommand
        {
            Provider = provider,
            KeyName = keyName,
            Value = request.Value,
            Description = request.Description,
            UpdateEnvFile = request.UpdateEnvFile ?? true
        };
        
        var result = await _mediator.Send(command);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    /// <summary>
    /// Delete an API setting
    /// </summary>
    [HttpDelete("{provider}/{keyName}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteApiSetting(string provider, string keyName)
    {
        var command = new DeleteApiSettingCommand
        {
            Provider = provider,
            KeyName = keyName
        };
        
        var result = await _mediator.Send(command);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return NotFound(result);
    }
}

public class UpdateApiSettingRequest
{
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool? UpdateEnvFile { get; set; }
}
