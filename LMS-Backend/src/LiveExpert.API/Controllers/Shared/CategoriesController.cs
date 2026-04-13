using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers.Shared;

/// <summary>
/// Public categories endpoint (used for blog categories, etc.)
/// </summary>
[Route("api/shared/categories")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly IRepository<Category> _categoryRepository;

    public CategoriesController(IRepository<Category> categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result<List<CategoryDto>>), 200)]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.FindAsync(c => c.IsActive, cancellationToken);
        var results = categories
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToList();

        return Ok(Result<List<CategoryDto>>.SuccessResult(results));
    }
}

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
