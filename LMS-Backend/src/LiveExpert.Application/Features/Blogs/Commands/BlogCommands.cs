using LiveExpert.Application.Common;
using MediatR;

namespace LiveExpert.Application.Features.Blogs.Commands;

// Create Blog Command
public class CreateBlogCommand : IRequest<Result<BlogDto>>
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? ThumbnailUrl { get; set; }
    public Guid CategoryId { get; set; }
    public string? Tags { get; set; }
    public bool IsPublished { get; set; } = false;
}

// Update Blog Command
public class UpdateBlogCommand : IRequest<Result<BlogDto>>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? ThumbnailUrl { get; set; }
    public Guid CategoryId { get; set; }
    public string? Tags { get; set; }
    public bool IsPublished { get; set; }
}

// Update Blog Status Command
public class UpdateBlogStatusCommand : IRequest<Result>
{
    public Guid Id { get; set; }
    public bool IsPublished { get; set; }
}

// Delete Blog Command
public class DeleteBlogCommand : IRequest<Result>
{
    public Guid Id { get; set; }
}

// Blog DTOs
public class BlogDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? ThumbnailUrl { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Tags { get; set; }
    public int ViewCount { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
