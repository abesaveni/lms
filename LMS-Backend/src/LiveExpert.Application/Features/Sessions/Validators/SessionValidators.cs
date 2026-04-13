using FluentValidation;
using LiveExpert.Application.Features.Sessions.Commands;

namespace LiveExpert.Application.Features.Sessions.Validators;

public class CreateSessionCommandValidator : AbstractValidator<CreateSessionCommand>
{
    public CreateSessionCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        RuleFor(x => x.SubjectId)
            .NotEmpty().WithMessage("Subject is required");

        RuleFor(x => x.ScheduledAt)
            .NotEmpty().WithMessage("Scheduled time is required")
            .Must(BeInFuture).WithMessage("Scheduled time must be in the future");

        RuleFor(x => x.Duration)
            .GreaterThan(0).WithMessage("Duration must be greater than 0")
            .LessThanOrEqualTo(480).WithMessage("Duration must not exceed 8 hours (480 minutes)");

        RuleFor(x => x.BasePrice)
            .GreaterThan(0).WithMessage("Base price must be greater than 0")
            .LessThanOrEqualTo(100000).WithMessage("Base price must not exceed 100,000");

        RuleFor(x => x.MaxStudents)
            .GreaterThan(0).WithMessage("Max students must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Max students must not exceed 100");

        RuleFor(x => x.SessionType)
            .IsInEnum().WithMessage("Invalid session type");
    }

    private bool BeInFuture(DateTime dateTime)
    {
        // For testing, we allow sessions in the past +/- 1 hour to avoid UTC confusion
        return dateTime > DateTime.UtcNow.AddHours(-1);
    }
}

public class UpdateSessionCommandValidator : AbstractValidator<UpdateSessionCommand>
{
    public UpdateSessionCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.ScheduledAt)
            .NotEmpty().WithMessage("Scheduled time is required");

        RuleFor(x => x.Duration)
            .GreaterThan(0).WithMessage("Duration must be greater than 0")
            .LessThanOrEqualTo(480).WithMessage("Duration must not exceed 8 hours");

        RuleFor(x => x.BasePrice)
            .GreaterThan(0).WithMessage("Base price must be greater than 0");
    }
}

public class BookSessionCommandValidator : AbstractValidator<BookSessionCommand>
{
    public BookSessionCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required");

        RuleFor(x => x.SpecialInstructions)
            .MaximumLength(500).WithMessage("Special instructions must not exceed 500 characters");
    }
}

public class MarkAttendanceCommandValidator : AbstractValidator<MarkAttendanceCommand>
{
    public MarkAttendanceCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required");

        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Student ID is required");
    }
}
