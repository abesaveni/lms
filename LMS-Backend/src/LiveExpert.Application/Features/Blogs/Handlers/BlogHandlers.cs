using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Blogs.Commands;
using LiveExpert.Application.Features.Blogs.Queries;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace LiveExpert.Application.Features.Blogs.Handlers;

// Create Blog Handler
public class CreateBlogCommandHandler : IRequestHandler<CreateBlogCommand, Result<BlogDto>>
{
    private readonly IRepository<Blog> _blogRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBlogCommandHandler(
        IRepository<Blog> blogRepository,
        IRepository<Category> categoryRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _blogRepository = blogRepository;
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BlogDto>> Handle(CreateBlogCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result<BlogDto>.FailureResult("UNAUTHORIZED", "User must be authenticated");
        }

        // Validate category exists
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
        {
            return Result<BlogDto>.FailureResult("NOT_FOUND", "Category not found");
        }

        // Generate slug from title
        var slug = GenerateSlug(request.Title);

        // Check if slug already exists
        var existingBlog = await _blogRepository.FirstOrDefaultAsync(b => b.Slug == slug, cancellationToken);
        if (existingBlog != null)
        {
            slug = $"{slug}-{DateTime.UtcNow.Ticks}";
        }

        var blog = new Blog
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Slug = slug,
            Content = request.Content,
            Summary = request.Summary,
            ThumbnailUrl = request.ThumbnailUrl,
            AuthorId = userId.Value,
            CategoryId = request.CategoryId,
            Tags = request.Tags,
            IsPublished = request.IsPublished,
            PublishedAt = request.IsPublished ? DateTime.UtcNow : null,
            ViewCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _blogRepository.AddAsync(blog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = await MapToDto(blog, cancellationToken);
        return Result<BlogDto>.SuccessResult(dto);
    }

    private string GenerateSlug(string title)
    {
        var slug = title.ToLower();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        return slug.Trim('-');
    }

    private async Task<BlogDto> MapToDto(Blog blog, CancellationToken cancellationToken)
    {
        var author = await _blogRepository.GetQueryable()
            .Where(b => b.Id == blog.Id)
            .Select(b => b.Author)
            .FirstOrDefaultAsync(cancellationToken);

        var category = await _categoryRepository.GetByIdAsync(blog.CategoryId, cancellationToken);

        return new BlogDto
        {
            Id = blog.Id,
            Title = blog.Title,
            Slug = blog.Slug,
            Content = blog.Content,
            Summary = blog.Summary,
            ThumbnailUrl = blog.ThumbnailUrl,
            AuthorId = blog.AuthorId,
            AuthorName = author?.Username ?? "Unknown",
            CategoryId = blog.CategoryId,
            CategoryName = category?.Name ?? "Uncategorized",
            Tags = blog.Tags,
            ViewCount = blog.ViewCount,
            IsPublished = blog.IsPublished,
            PublishedAt = blog.PublishedAt,
            CreatedAt = blog.CreatedAt,
            UpdatedAt = blog.UpdatedAt
        };
    }
}

// Update Blog Handler
public class UpdateBlogCommandHandler : IRequestHandler<UpdateBlogCommand, Result<BlogDto>>
{
    private readonly IRepository<Blog> _blogRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBlogCommandHandler(
        IRepository<Blog> blogRepository,
        IRepository<Category> categoryRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _blogRepository = blogRepository;
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BlogDto>> Handle(UpdateBlogCommand request, CancellationToken cancellationToken)
    {
        var blog = await _blogRepository.GetByIdAsync(request.Id, cancellationToken);
        if (blog == null)
        {
            return Result<BlogDto>.FailureResult("NOT_FOUND", "Blog not found");
        }

        // Validate category exists
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
        {
            return Result<BlogDto>.FailureResult("NOT_FOUND", "Category not found");
        }

        // Update properties
        blog.Title = request.Title;
        blog.Content = request.Content;
        blog.Summary = request.Summary;
        blog.ThumbnailUrl = request.ThumbnailUrl;
        blog.CategoryId = request.CategoryId;
        blog.Tags = request.Tags;
        
        // Update published status
        if (request.IsPublished && !blog.IsPublished)
        {
            blog.PublishedAt = DateTime.UtcNow;
        }
        else if (!request.IsPublished && blog.IsPublished)
        {
            blog.PublishedAt = null;
        }
        
        blog.IsPublished = request.IsPublished;
        blog.UpdatedAt = DateTime.UtcNow;

        // Regenerate slug if title changed
        var newSlug = GenerateSlug(request.Title);
        if (blog.Slug != newSlug)
        {
            var existingBlog = await _blogRepository.FirstOrDefaultAsync(b => b.Slug == newSlug && b.Id != blog.Id, cancellationToken);
            if (existingBlog != null)
            {
                newSlug = $"{newSlug}-{DateTime.UtcNow.Ticks}";
            }
            blog.Slug = newSlug;
        }

        await _blogRepository.UpdateAsync(blog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = await MapToDto(blog, cancellationToken);
        return Result<BlogDto>.SuccessResult(dto);
    }

    private string GenerateSlug(string title)
    {
        var slug = title.ToLower();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        return slug.Trim('-');
    }

    private async Task<BlogDto> MapToDto(Blog blog, CancellationToken cancellationToken)
    {
        var author = await _blogRepository.GetQueryable()
            .Where(b => b.Id == blog.Id)
            .Select(b => b.Author)
            .FirstOrDefaultAsync(cancellationToken);

        var category = await _categoryRepository.GetByIdAsync(blog.CategoryId, cancellationToken);

        return new BlogDto
        {
            Id = blog.Id,
            Title = blog.Title,
            Slug = blog.Slug,
            Content = blog.Content,
            Summary = blog.Summary,
            ThumbnailUrl = blog.ThumbnailUrl,
            AuthorId = blog.AuthorId,
            AuthorName = author?.Username ?? "Unknown",
            CategoryId = blog.CategoryId,
            CategoryName = category?.Name ?? "Uncategorized",
            Tags = blog.Tags,
            ViewCount = blog.ViewCount,
            IsPublished = blog.IsPublished,
            PublishedAt = blog.PublishedAt,
            CreatedAt = blog.CreatedAt,
            UpdatedAt = blog.UpdatedAt
        };
    }
}

// Update Blog Status Handler
public class UpdateBlogStatusCommandHandler : IRequestHandler<UpdateBlogStatusCommand, Result>
{
    private readonly IRepository<Blog> _blogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBlogStatusCommandHandler(
        IRepository<Blog> blogRepository,
        IUnitOfWork unitOfWork)
    {
        _blogRepository = blogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateBlogStatusCommand request, CancellationToken cancellationToken)
    {
        var blog = await _blogRepository.GetByIdAsync(request.Id, cancellationToken);
        if (blog == null)
        {
            return Result.FailureResult("NOT_FOUND", "Blog not found");
        }

        blog.IsPublished = request.IsPublished;
        if (request.IsPublished && blog.PublishedAt == null)
        {
            blog.PublishedAt = DateTime.UtcNow;
        }
        else if (!request.IsPublished)
        {
            blog.PublishedAt = null;
        }
        
        blog.UpdatedAt = DateTime.UtcNow;

        await _blogRepository.UpdateAsync(blog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.SuccessResult();
    }
}

// Delete Blog Handler
public class DeleteBlogCommandHandler : IRequestHandler<DeleteBlogCommand, Result>
{
    private readonly IRepository<Blog> _blogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBlogCommandHandler(
        IRepository<Blog> blogRepository,
        IUnitOfWork unitOfWork)
    {
        _blogRepository = blogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteBlogCommand request, CancellationToken cancellationToken)
    {
        var blog = await _blogRepository.GetByIdAsync(request.Id, cancellationToken);
        if (blog == null)
        {
            return Result.FailureResult("NOT_FOUND", "Blog not found");
        }

        await _blogRepository.DeleteAsync(blog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.SuccessResult();
    }
}

// Get Blogs Handler
public class GetBlogsQueryHandler : IRequestHandler<GetBlogsQuery, Result<List<BlogDto>>>
{
    private readonly IRepository<Blog> _blogRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<User> _userRepository;

    public GetBlogsQueryHandler(
        IRepository<Blog> blogRepository,
        IRepository<Category> categoryRepository,
        IRepository<User> userRepository)
    {
        _blogRepository = blogRepository;
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<List<BlogDto>>> Handle(GetBlogsQuery request, CancellationToken cancellationToken)
    {
        var blogs = request.IncludeUnpublished
            ? await _blogRepository.GetAllAsync(cancellationToken)
            : await _blogRepository.FindAsync(b => b.IsPublished, cancellationToken);

        var dtos = new List<BlogDto>();
        foreach (var blog in blogs.OrderByDescending(b => b.CreatedAt))
        {
            var author = await _userRepository.GetByIdAsync(blog.AuthorId, cancellationToken);
            var category = await _categoryRepository.GetByIdAsync(blog.CategoryId, cancellationToken);

            dtos.Add(new BlogDto
            {
                Id = blog.Id,
                Title = blog.Title,
                Slug = blog.Slug,
                Content = blog.Content,
                Summary = blog.Summary,
                ThumbnailUrl = blog.ThumbnailUrl,
                AuthorId = blog.AuthorId,
                AuthorName = author?.Username ?? "Unknown",
                CategoryId = blog.CategoryId,
                CategoryName = category?.Name ?? "Uncategorized",
                Tags = blog.Tags,
                ViewCount = blog.ViewCount,
                IsPublished = blog.IsPublished,
                PublishedAt = blog.PublishedAt,
                CreatedAt = blog.CreatedAt,
                UpdatedAt = blog.UpdatedAt
            });
        }

        return Result<List<BlogDto>>.SuccessResult(dtos);
    }
}

// Get Blog By ID Handler
public class GetBlogByIdQueryHandler : IRequestHandler<GetBlogByIdQuery, Result<BlogDto>>
{
    private readonly IRepository<Blog> _blogRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<User> _userRepository;

    public GetBlogByIdQueryHandler(
        IRepository<Blog> blogRepository,
        IRepository<Category> categoryRepository,
        IRepository<User> userRepository)
    {
        _blogRepository = blogRepository;
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<BlogDto>> Handle(GetBlogByIdQuery request, CancellationToken cancellationToken)
    {
        var blog = await _blogRepository.GetByIdAsync(request.Id, cancellationToken);
        if (blog == null)
        {
            return Result<BlogDto>.FailureResult("NOT_FOUND", "Blog not found");
        }

        var author = await _userRepository.GetByIdAsync(blog.AuthorId, cancellationToken);
        var category = await _categoryRepository.GetByIdAsync(blog.CategoryId, cancellationToken);

        var dto = new BlogDto
        {
            Id = blog.Id,
            Title = blog.Title,
            Slug = blog.Slug,
            Content = blog.Content,
            Summary = blog.Summary,
            ThumbnailUrl = blog.ThumbnailUrl,
            AuthorId = blog.AuthorId,
            AuthorName = author?.Username ?? "Unknown",
            CategoryId = blog.CategoryId,
            CategoryName = category?.Name ?? "Uncategorized",
            Tags = blog.Tags,
            ViewCount = blog.ViewCount,
            IsPublished = blog.IsPublished,
            PublishedAt = blog.PublishedAt,
            CreatedAt = blog.CreatedAt,
            UpdatedAt = blog.UpdatedAt
        };

        return Result<BlogDto>.SuccessResult(dto);
    }
}
