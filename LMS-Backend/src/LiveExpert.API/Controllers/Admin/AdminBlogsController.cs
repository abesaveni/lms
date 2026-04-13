using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Blogs.Commands;
using LiveExpert.Application.Features.Blogs.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers.Admin;

/// <summary>
/// Admin blog management endpoints
/// </summary>
[Authorize(Roles = "Admin")]
[Route("api/admin/blogs")]
[ApiController]
public class AdminBlogsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminBlogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all blogs (including unpublished)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<BlogDto>>), 200)]
    public async Task<IActionResult> GetBlogs()
    {
        var query = new GetBlogsQuery { IncludeUnpublished = true };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get blog by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Result<BlogDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetBlog(Guid id)
    {
        var query = new GetBlogByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }

    /// <summary>
    /// Create a new blog
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Result<BlogDto>), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateBlog([FromBody] CreateBlogRequest request)
    {
        var command = new CreateBlogCommand
        {
            Title = request.Title,
            Content = request.Content,
            Summary = request.Summary,
            ThumbnailUrl = request.ThumbnailUrl,
            CategoryId = request.CategoryId,
            Tags = request.Tags,
            IsPublished = request.IsPublished
        };

        var result = await _mediator.Send(command);
        
        if (!result.Success)
            return BadRequest(result);
            
        return CreatedAtAction(nameof(GetBlog), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update a blog
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Result<BlogDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateBlog(Guid id, [FromBody] UpdateBlogRequest request)
    {
        var command = new UpdateBlogCommand
        {
            Id = id,
            Title = request.Title,
            Content = request.Content,
            Summary = request.Summary,
            ThumbnailUrl = request.ThumbnailUrl,
            CategoryId = request.CategoryId,
            Tags = request.Tags,
            IsPublished = request.IsPublished
        };

        var result = await _mediator.Send(command);
        
        if (!result.Success)
        {
            return result.Error?.Code == "NOT_FOUND" ? NotFound(result) : BadRequest(result);
        }
            
        return Ok(result);
    }

    /// <summary>
    /// Update blog status (publish/unpublish)
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(Result), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateBlogStatus(Guid id, [FromBody] UpdateBlogStatusRequest request)
    {
        var command = new UpdateBlogStatusCommand
        {
            Id = id,
            IsPublished = request.IsPublished
        };

        var result = await _mediator.Send(command);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }

    /// <summary>
    /// Delete a blog
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(Result), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteBlog(Guid id)
    {
        var command = new DeleteBlogCommand { Id = id };
        var result = await _mediator.Send(command);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }
}

// Request DTOs
public class CreateBlogRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? ThumbnailUrl { get; set; }
    public Guid CategoryId { get; set; }
    public string? Tags { get; set; }
    public bool IsPublished { get; set; } = false;
}

public class UpdateBlogRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? ThumbnailUrl { get; set; }
    public Guid CategoryId { get; set; }
    public string? Tags { get; set; }
    public bool IsPublished { get; set; }
}

public class UpdateBlogStatusRequest
{
    public bool IsPublished { get; set; }
}
