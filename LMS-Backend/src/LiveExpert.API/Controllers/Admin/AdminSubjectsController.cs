using LiveExpert.Application.Common;
using LiveExpert.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LiveExpert.Infrastructure.Data;

namespace LiveExpert.API.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("api/admin/subjects")]
    public class AdminSubjectsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminSubjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetSubjects()
        {
            var subjects = await _context.Subjects
                .OrderBy(s => s.Name)
                .ToListAsync();

            return Ok(Result<List<Subject>>.SuccessResult(subjects));
        }

        [HttpPost]
        public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectRequest request)
        {
            if (string.IsNullOrEmpty(request.Name))
                return BadRequest(Result.FailureResult("INVALID_REQUEST", "Subject name is required"));

            var exists = await _context.Subjects.AnyAsync(s => s.Name.ToLower() == request.Name.ToLower());
            if (exists)
                return BadRequest(Result.FailureResult("ALREADY_EXISTS", "A subject with this name already exists"));

            var subject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Slug = request.Name.ToLower().Replace(" ", "-").Replace("/", "-"),
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Subjects.AddAsync(subject);
            await _context.SaveChangesAsync();

            return Ok(Result<Subject>.SuccessResult(subject));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSubject(Guid id, [FromBody] CreateSubjectRequest request)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null)
                return NotFound(Result.FailureResult("NOT_FOUND", "Subject not found"));

            if (!string.IsNullOrEmpty(request.Name))
            {
                var exists = await _context.Subjects.AnyAsync(s => s.Name.ToLower() == request.Name.ToLower() && s.Id != id);
                if (exists)
                    return BadRequest(Result.FailureResult("ALREADY_EXISTS", "Another subject with this name already exists"));
                
                subject.Name = request.Name;
            }

            subject.Description = request.Description;
            subject.IsActive = request.IsActive;

            _context.Subjects.Update(subject);
            await _context.SaveChangesAsync();

            return Ok(Result<Subject>.SuccessResult(subject));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubject(Guid id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null)
                return NotFound(Result.FailureResult("NOT_FOUND", "Subject not found"));

            // Check if any tutors or sessions are using this subject
            var inUse = await _context.TutorProfiles.AnyAsync(t => t.Skills.Contains(subject.Name));
            if (inUse)
            {
                // Soft delete instead
                subject.IsActive = false;
                _context.Subjects.Update(subject);
                await _context.SaveChangesAsync();
                return Ok(Result.SuccessResult("Subject deactivated because it is in use by tutors."));
            }

            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();

            return Ok(Result.SuccessResult("Subject deleted successfully"));
        }
    }

    public class CreateSubjectRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
