using LiveExpert.Application.Common;
using LiveExpert.Domain.Enums;
using MediatR;

namespace LiveExpert.Application.Features.Inquiries.Commands;

// Feature 12: Pre-booking inquiry

public class SendInquiryCommand : IRequest<Result<TutorInquiryDto>>
{
    public Guid TutorId { get; set; }
    public Guid? SubjectId { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ReplyToInquiryCommand : IRequest<Result<TutorInquiryDto>>
{
    public Guid InquiryId { get; set; }
    public string Reply { get; set; } = string.Empty;
}

public class CloseInquiryCommand : IRequest<Result>
{
    public Guid InquiryId { get; set; }
}

public class GetSentInquiriesQuery : IRequest<Result<List<TutorInquiryDto>>>
{
}

public class GetReceivedInquiriesQuery : IRequest<Result<List<TutorInquiryDto>>>
{
}

public class TutorInquiryDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public Guid TutorId { get; set; }
    public string TutorName { get; set; } = string.Empty;
    public Guid? SubjectId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? TutorReply { get; set; }
    public DateTime? RepliedAt { get; set; }
    public InquiryStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
