namespace LiveExpert.Application.Interfaces;

public interface IResumeParserService
{
    Task<ResumeParseResult> ParseResumeAsync(
        Stream fileStream, 
        string fileName, 
        CancellationToken cancellationToken = default);
}

public class ResumeParseResult
{
    // Personal Information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    
    // Professional Information
    public string Headline { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public int TotalExperience { get; set; }
    
    // Education
    public string HighestEducation { get; set; } = string.Empty;
    public string University { get; set; } = string.Empty;
    
    // Skills & Languages
    public List<string> Skills { get; set; } = new();
    public List<string> Languages { get; set; } = new();
    public List<string> Certifications { get; set; } = new();
    
    // Detailed Work Experience
    public List<WorkExperienceDto> WorkExperience { get; set; } = new();
    
    // Detailed Education
    public List<EducationDto> EducationHistory { get; set; } = new();
    
    // Location
    public string Location { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    
    // Social Links
    public string LinkedInUrl { get; set; } = string.Empty;
    public string GitHubUrl { get; set; } = string.Empty;
    public string PortfolioUrl { get; set; } = string.Empty;
    
    // Uploaded file URL (set by the controller after storing the file)
    public string ResumeUrl { get; set; } = string.Empty;

    // Raw data for debugging
    public string RawData { get; set; } = string.Empty;
}

public class WorkExperienceDto
{
    public string JobTitle { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
}

public class EducationDto
{
    public string Degree { get; set; } = string.Empty;
    public string Institution { get; set; } = string.Empty;
    public string FieldOfStudy { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Grade { get; set; } = string.Empty;
}
