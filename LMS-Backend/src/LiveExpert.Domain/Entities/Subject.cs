using LiveExpert.Domain.Common;

namespace LiveExpert.Domain.Entities;

public class Subject : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public string? IconUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    // Navigation Properties
    public Category? Category { get; set; }
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    // Navigation Properties
    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    public ICollection<Blog> Blogs { get; set; } = new List<Blog>();
    public ICollection<FAQ> FAQs { get; set; } = new List<FAQ>();
}
