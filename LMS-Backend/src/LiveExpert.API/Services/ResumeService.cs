using LiveExpert.Infrastructure.Data;
using LiveExpert.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using UglyToad.PdfPig;
using DocumentFormat.OpenXml.Packaging;
using System.Text;

namespace LiveExpert.API.Services;

public class ResumeService
{
    private readonly ClaudeAIService _claudeService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ResumeService> _logger;

    public ResumeService(ClaudeAIService claudeService, ApplicationDbContext context, ILogger<ResumeService> logger)
    {
        _claudeService = claudeService;
        _context = context;
        _logger = logger;
    }

    public async Task<string> GenerateFresherResumeAsync(Guid userId, FresherResumeRequest request)
    {
        var prompt = $@"You are a world-class resume writer who transforms raw candidate inputs into stunning, recruiter-ready resumes that stand out and pass ATS systems.

CANDIDATE RAW DATA:
Name: {request.FullName}
Email: {request.Email}
Phone: {request.Phone}
Degree: {request.Degree}
College: {request.College}
Graduation Year: {request.GraduationYear}
CGPA: {request.Cgpa}
Skills: {request.Skills}
Projects: {request.Projects}
Internships: {request.Internships ?? "None"}
Certifications: {request.Certifications ?? "None"}
Career Objective (draft): {request.CareerObjective ?? ""}
Target Role: {request.TargetRole}

YOUR JOB — DO ALL OF THESE:
1. DO NOT just copy the raw inputs. Transform and ENHANCE every section with professional resume language.
2. Career Objective: Write 3 powerful, role-specific sentences. Mention the target role, key skills, and value the candidate brings. Make it specific, not generic.
3. Technical Skills: Organise into clear categories (Programming Languages, Frameworks, Tools, Databases, etc.). Add commonly expected skills for {request.TargetRole} that are implied by their stack.
4. Projects: For each project, write 3-4 bullet points using strong action verbs (Engineered, Developed, Implemented, Optimised, Designed, Deployed). Add estimated impact where possible (e.g. ""reduced processing time by 30%"", ""served 500+ users""). Include the full tech stack.
5. Internships: Expand each internship with 3 achievement-oriented bullets. Use metrics wherever logical.
6. Add a STRENGTHS or KEY HIGHLIGHTS section with 4-5 points tailored to {request.TargetRole}.
7. Add 1-2 relevant industry certifications that a {request.TargetRole} fresher should pursue (mark as ""In Progress"" or suggest them in the certifications section if none provided).
8. ATS keywords: Naturally embed 8-10 keywords a recruiter for {request.TargetRole} would search for.

OUTPUT FORMAT (use this exact markdown structure):
**[CANDIDATE FULL NAME]**
[Email] | [Phone]
[Location if available]

**CAREER OBJECTIVE**
[3 powerful sentences]

**EDUCATION**
**[Degree]** | [Year]
[College Name]
CGPA: [value]

**TECHNICAL SKILLS**
* **[Category]:** [skills]
* **[Category]:** [skills]

**PROJECTS**
**[Project Name]** | [Tech Stack]
* [Action verb + what you built + impact]
* [Action verb + technical detail]
* [Action verb + outcome/metric]

**INTERNSHIP EXPERIENCE** (skip section if none)
**[Role]** | [Company] | [Duration]
* [Achievement bullet]

**CERTIFICATIONS & ACHIEVEMENTS**
* [Certification or achievement]

**KEY HIGHLIGHTS**
* [Strength relevant to target role]

RULES:
- Use ** for bold (section headers and sub-headers only)
- Use * for bullet points
- Do NOT use --- horizontal lines or === dividers
- Do NOT add placeholder text like [Your Name] — use actual values
- Keep total length to ~1 page (600-800 words)";

        var resumeText = await _claudeService.GenerateContentAsync(prompt, 3000);
        await SaveResumeToProfile(userId, resumeText, "fresher");
        return resumeText;
    }

    public async Task<string> GenerateExperiencedResumeAsync(Guid userId, ExperiencedResumeRequest request)
    {
        var prompt = $@"You are a world-class resume writer specialising in senior professional resumes that win interviews at top companies.

CANDIDATE RAW DATA:
Name: {request.FullName}
Email: {request.Email}
Phone: {request.Phone}
Total Experience: {request.TotalExperience} years
Current Role: {request.CurrentRole} at {request.CurrentCompany}
Current CTC: {request.CurrentCtc} | Expected CTC: {request.ExpectedCtc}
Notice Period: {request.NoticePeriod}
Skills: {request.Skills}
Work History: {request.WorkHistory}
Key Achievements: {request.KeyAchievements}
Education: {request.Education}
Certifications: {request.Certifications ?? "None"}
Target Role: {request.TargetRole}
Professional Summary (draft): {request.ProfessionalSummary ?? ""}

YOUR JOB — DO ALL OF THESE:
1. Professional Summary: Write 3-4 high-impact sentences. Start with years of experience + specialisation. Include a unique value proposition. End with what you bring to {request.TargetRole}.
2. Core Competencies: Create a compact grid or categorised list of 12-16 skills. Include skills implied by their experience that a {request.TargetRole} recruiter would look for.
3. Professional Experience: For EACH role, write 5-6 achievement-oriented bullets in CAR format (Challenge → Action → Result). ALWAYS add numbers: percentages, team sizes, revenue impact, performance improvements, scale. Transform weak statements like ""worked on X"" into ""Spearheaded X resulting in Y% improvement"".
4. Key Achievements: Highlight 4-5 career-defining wins with metrics. Make them bold and impressive.
5. Add a NOTABLE PROJECTS section with 2-3 projects that showcase technical leadership.
6. ATS: Embed 10+ keywords a recruiter for {request.TargetRole} would search. Place them naturally in bullets.
7. Make every bullet start with a powerful action verb: Led, Architected, Delivered, Scaled, Optimised, Drove, Transformed, etc.

OUTPUT FORMAT (use this exact markdown structure):
**[CANDIDATE FULL NAME]**
[Email] | [Phone] | Notice Period: [value]
Location: India

**PROFESSIONAL SUMMARY**
[3-4 high-impact sentences]

**CORE COMPETENCIES**
* **[Category]:** [skills]
* **[Category]:** [skills]

**PROFESSIONAL EXPERIENCE**
**[Role]** | [Company] | [Duration]
* [Quantified achievement bullet]
* [Quantified achievement bullet]

**KEY ACHIEVEMENTS**
* [Metric-backed achievement]

**NOTABLE PROJECTS**
**[Project Name]** | [Tech Stack]
* [Impact bullet]

**EDUCATION**
**[Degree]** | [University] | [Year]

**CERTIFICATIONS**
* [Certification]

RULES:
- Use ** for bold text only
- Use * for all bullet points
- Do NOT use --- horizontal lines or === dividers
- Never use placeholder text — use real values
- Target 1.5-2 pages (900-1200 words)
- Every single bullet must contain a number, percentage, or measurable outcome";

        var resumeText = await _claudeService.GenerateContentAsync(prompt, 4000);
        await SaveResumeToProfile(userId, resumeText, "experienced");
        return resumeText;
    }

    public async Task<string> ReviewResumeAsync(string resumeText, string targetRole)
    {
        var prompt = $@"You are a senior HR professional and resume expert. Review the following resume critically.

TARGET ROLE: {targetRole}

RESUME:
{resumeText}

Provide a structured review with:

1. OVERALL SCORE: X/10 (with brief justification)

2. STRENGTHS (3-5 points):
   - What the resume does well

3. CRITICAL IMPROVEMENTS NEEDED (prioritised list):
   - Specific changes with examples of how to rewrite weak sections

4. ATS OPTIMISATION:
   - Missing keywords for {targetRole}
   - Formatting issues that could hurt ATS parsing

5. IMPACT & QUANTIFICATION:
   - Bullet points that need numbers/metrics added (provide rewritten examples)

6. OVERALL RECOMMENDATION:
   - Is this resume ready to submit? What are the 3 most important changes to make first?

Be direct, specific, and actionable. No generic advice.";

        return await _claudeService.GenerateContentAsync(prompt);
    }

    public async Task<string> GenerateTechRoadmapAsync(string currentSkills, string targetRole, string timeframe)
    {
        var prompt = $@"You are a senior technology mentor and career coach.

Create a detailed, actionable learning roadmap for the following:

CURRENT SKILLS: {currentSkills}
TARGET ROLE: {targetRole}
TIMEFRAME: {timeframe}

Provide:

1. SKILL GAP ANALYSIS:
   - What skills the person already has that are relevant
   - Critical gaps that must be filled
   - Nice-to-have skills for competitive edge

2. LEARNING ROADMAP (week-by-week or month-by-month based on timeframe):
   - Phase 1: Foundation (specific topics, resources, projects)
   - Phase 2: Intermediate (specific topics, resources, projects)
   - Phase 3: Advanced / Job-Ready (specific topics, mock projects)

3. RECOMMENDED FREE RESOURCES for each skill:
   - Specific YouTube channels, documentation, free courses

4. PORTFOLIO PROJECTS TO BUILD:
   - 3 project ideas that will impress recruiters for {targetRole}
   - Tech stack for each project

5. JOB READINESS CHECKLIST:
   - Skills to demonstrate confidently in interviews
   - Common interview questions for {targetRole} to prepare for

Make this highly specific to {targetRole} in the current Indian/global tech market (2025-2026).";

        return await _claudeService.GenerateContentAsync(prompt);
    }

    // ── Upload & Enhance ───────────────────────────────────────────────────────

    public async Task<string> EnhanceUploadedResumeAsync(Guid userId, Stream fileStream, string fileName, string targetRole)
    {
        // 1. Extract raw text from the uploaded file
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        string rawText = ext switch
        {
            ".pdf"  => ExtractPdfText(fileStream),
            ".docx" => ExtractDocxText(fileStream),
            ".doc"  => ExtractDocxText(fileStream),
            ".txt"  => await new StreamReader(fileStream).ReadToEndAsync(),
            _ => throw new InvalidOperationException($"Unsupported file type: {ext}. Upload a PDF, DOCX, or TXT file.")
        };

        if (string.IsNullOrWhiteSpace(rawText) || rawText.Length < 50)
            throw new InvalidOperationException("Could not extract enough text from the uploaded file. Make sure it is not a scanned image.");

        // 2. Build ATS-enhancement prompt
        var prompt = $@"You are a world-class resume writer and ATS optimisation expert. A candidate has uploaded their existing resume and wants it transformed into a perfect 10/10 ATS-optimised resume.

TARGET ROLE: {(string.IsNullOrWhiteSpace(targetRole) ? "Not specified — infer from resume" : targetRole)}

EXISTING RESUME TEXT (extracted from uploaded file):
---
{rawText}
---

YOUR TASK — FULLY REWRITE AND ENHANCE THE RESUME:

1. CONTACT HEADER: Keep all contact details exactly as provided. Do NOT add LinkedIn, GitHub, or any other field that was not in the original resume.

2. PROFESSIONAL SUMMARY / CAREER OBJECTIVE:
   - Rewrite with 3-4 high-impact sentences specific to the target role.
   - Include years of experience, key strengths, and unique value proposition.

3. SKILLS SECTION:
   - Reorganise into labelled categories (Programming Languages, Frameworks, Tools, Cloud, Databases, etc.)
   - Add commonly expected ATS keywords for the target role that are implied by their experience.
   - Remove vague entries, keep specific technologies.

4. EXPERIENCE / PROJECTS:
   - Rewrite EVERY bullet using strong action verbs (Engineered, Developed, Optimised, Led, Delivered, Reduced, Increased).
   - Add QUANTIFIED METRICS to every bullet (%, numbers, team size, business impact). If exact numbers are missing, use reasonable estimates based on context (e.g., ""reduced load time by ~35%"", ""served 500+ users"").
   - Ensure each role has 4-6 achievement-oriented bullets.
   - Use CAR format (Challenge → Action → Result).

5. EDUCATION: Keep as-is, format cleanly.

6. CERTIFICATIONS: Keep existing. Add 1-2 relevant ones the candidate should pursue (mark ""Recommended"").

7. ADD THESE MISSING SECTIONS IF NOT PRESENT:
   - KEY ACHIEVEMENTS (3-5 standout career wins with metrics)
   - KEY HIGHLIGHTS (4-5 strengths tailored to target role)

8. ATS OPTIMISATION:
   - Embed 10-15 high-value keywords that recruiters for {(string.IsNullOrWhiteSpace(targetRole) ? "this role" : targetRole)} search for.
   - Place keywords naturally in bullets and skills — never stuff them awkwardly.
   - Ensure section headers match standard ATS-parseable names.

OUTPUT FORMAT (use this exact markdown):
**[FULL NAME]**
[Email] | [Phone] | [Location]

**PROFESSIONAL SUMMARY**
[3-4 power sentences]

**TECHNICAL SKILLS**
* **[Category]:** [skills]

**PROFESSIONAL EXPERIENCE** or **PROJECTS** (whichever applies)
**[Role/Project]** | [Company/Tech Stack] | [Duration]
* [Quantified achievement]
* [Quantified achievement]

**EDUCATION**
**[Degree]** | [University] | [Year]

**CERTIFICATIONS & ACHIEVEMENTS**
* [item]

**KEY ACHIEVEMENTS**
* [metric-backed win]

**KEY HIGHLIGHTS**
* **[Strength]:** [explanation]

RULES:
- Use ** for bold only
- Use * for bullets only
- Do NOT use --- dividers or === lines
- Every single bullet must contain a measurable outcome or strong action verb
- Make this resume stand out — go beyond what was given, elevate it significantly";

        var enhancedResume = await _claudeService.GenerateContentAsync(prompt, 4000);
        await SaveResumeToProfile(userId, enhancedResume, "enhanced");
        return enhancedResume;
    }

    // ── Text extraction helpers ────────────────────────────────────────────────

    private static string ExtractPdfText(Stream stream)
    {
        var sb = new StringBuilder();
        using var doc = PdfDocument.Open(stream);
        foreach (var page in doc.GetPages())
        {
            foreach (var word in page.GetWords())
                sb.Append(word.Text).Append(' ');
            sb.AppendLine();
        }
        return sb.ToString().Trim();
    }

    private static string ExtractDocxText(Stream stream)
    {
        var sb = new StringBuilder();
        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart?.Document?.Body;
        if (body == null) return string.Empty;
        foreach (var para in body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
        {
            sb.AppendLine(para.InnerText);
        }
        return sb.ToString().Trim();
    }

    private async Task SaveResumeToProfile(Guid userId, string resumeText, string resumeType)
    {
        try
        {
            var profile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile != null)
            {
                profile.ResumeData = resumeText;
                profile.ResumeType = resumeType;
                profile.ResumeLastUpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to save resume to student profile for user {UserId}", userId);
        }
    }
}

public class FresherResumeRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string College { get; set; } = string.Empty;
    public string GraduationYear { get; set; } = string.Empty;
    public string Cgpa { get; set; } = string.Empty;
    public string Skills { get; set; } = string.Empty;
    public string Projects { get; set; } = string.Empty;
    public string? Internships { get; set; }
    public string? Certifications { get; set; }
    public string? CareerObjective { get; set; }
    public string TargetRole { get; set; } = string.Empty;
}

public class ExperiencedResumeRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int TotalExperience { get; set; }
    public string CurrentRole { get; set; } = string.Empty;
    public string CurrentCompany { get; set; } = string.Empty;
    public string CurrentCtc { get; set; } = string.Empty;
    public string ExpectedCtc { get; set; } = string.Empty;
    public string NoticePeriod { get; set; } = string.Empty;
    public string Skills { get; set; } = string.Empty;
    public string WorkHistory { get; set; } = string.Empty;
    public string KeyAchievements { get; set; } = string.Empty;
    public string Education { get; set; } = string.Empty;
    public string? Certifications { get; set; }
    public string TargetRole { get; set; } = string.Empty;
    public string? ProfessionalSummary { get; set; }
}

public class ResumeReviewRequest
{
    public string ResumeText { get; set; } = string.Empty;
    public string TargetRole { get; set; } = string.Empty;
}

public class TechRoadmapRequest
{
    public string CurrentSkills { get; set; } = string.Empty;
    public string TargetRole { get; set; } = string.Empty;
    public string Timeframe { get; set; } = "6 months";
}
