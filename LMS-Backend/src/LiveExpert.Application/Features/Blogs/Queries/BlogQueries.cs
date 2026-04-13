using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Blogs.Commands;
using MediatR;

namespace LiveExpert.Application.Features.Blogs.Queries;

// Get All Blogs (Admin - all blogs, Public - only published)
public class GetBlogsQuery : IRequest<Result<List<BlogDto>>>
{
    public bool IncludeUnpublished { get; set; } = false; // Admin can see all
}

// Get Blog By ID
public class GetBlogByIdQuery : IRequest<Result<BlogDto>>
{
    public Guid Id { get; set; }
}

// Get Blogs By Category
public class GetBlogsByCategoryQuery : IRequest<Result<List<BlogDto>>>
{
    public Guid CategoryId { get; set; }
    public bool IncludeUnpublished { get; set; } = false;
}
