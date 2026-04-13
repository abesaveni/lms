namespace LiveExpert.Application.Common;

public class SignupEmailVerificationState
{
    public string Code { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool Verified { get; set; }
    public string? VerificationToken { get; set; }
}
