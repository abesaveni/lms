namespace LiveExpert.Application.Common;

public static class EmailTemplates
{
    public static string VerificationEmail(string recipientName, string code, int expiresMinutes)
    {
        var safeName = string.IsNullOrWhiteSpace(recipientName) ? "there" : recipientName;

        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
  <title>Email Verification</title>
</head>
<body style=""margin:0;padding:0;background:#f6f7fb;font-family:Arial,Helvetica,sans-serif;color:#1f2937;"">
  <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
    <tr>
      <td align=""center"" style=""padding:24px;"">
        <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""600"" style=""background:#ffffff;border-radius:12px;overflow:hidden;border:1px solid #e5e7eb;"">
          <tr>
            <td style=""padding:24px 32px;background:#0f172a;color:#ffffff;"">
              <h1 style=""margin:0;font-size:20px;font-weight:600;"">LiveExpert.AI</h1>
              <p style=""margin:6px 0 0;font-size:14px;color:#cbd5f5;"">Email verification</p>
            </td>
          </tr>
          <tr>
            <td style=""padding:32px;"">
              <p style=""margin:0 0 12px;font-size:16px;"">Hi {safeName},</p>
              <p style=""margin:0 0 20px;font-size:14px;line-height:1.6;"">
                Use the verification code below to complete your signup. This code expires in {expiresMinutes} minutes.
              </p>
              <div style=""text-align:center;margin:24px 0;"">
                <span style=""display:inline-block;padding:16px 24px;border-radius:8px;background:#f3f4f6;font-size:24px;letter-spacing:6px;font-weight:700;color:#111827;"">
                  {code}
                </span>
              </div>
              <p style=""margin:0;font-size:12px;color:#6b7280;"">
                If you didn’t request this, you can safely ignore this email.
              </p>
            </td>
          </tr>
          <tr>
            <td style=""padding:20px 32px;border-top:1px solid #e5e7eb;font-size:12px;color:#9ca3af;"">
              © 2026 LiveExpert.AI. All rights reserved.
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }

    public static string WelcomeEmail(string recipientName, string role)
    {
        var safeName = string.IsNullOrWhiteSpace(recipientName) ? "there" : recipientName;
        var safeRole = string.IsNullOrWhiteSpace(role) ? "member" : role;

        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
  <title>Welcome to LiveExpert.AI</title>
</head>
<body style=""margin:0;padding:0;background:#f6f7fb;font-family:Arial,Helvetica,sans-serif;color:#1f2937;"">
  <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
    <tr>
      <td align=""center"" style=""padding:24px;"">
        <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""600"" style=""background:#ffffff;border-radius:12px;overflow:hidden;border:1px solid #e5e7eb;"">
          <tr>
            <td style=""padding:24px 32px;background:#0f172a;color:#ffffff;"">
              <h1 style=""margin:0;font-size:20px;font-weight:600;"">LiveExpert.AI</h1>
              <p style=""margin:6px 0 0;font-size:14px;color:#cbd5f5;"">Welcome</p>
            </td>
          </tr>
          <tr>
            <td style=""padding:32px;"">
              <p style=""margin:0 0 12px;font-size:16px;"">Hi {safeName},</p>
              <p style=""margin:0 0 20px;font-size:14px;line-height:1.6;"">
                Your {safeRole} account is now active. We’re excited to have you on LiveExpert.AI.
              </p>
              <p style=""margin:0;font-size:14px;line-height:1.6;"">
                If you have any questions, just reply to this email and our support team will help.
              </p>
            </td>
          </tr>
          <tr>
            <td style=""padding:20px 32px;border-top:1px solid #e5e7eb;font-size:12px;color:#9ca3af;"">
              © 2026 LiveExpert.AI. All rights reserved.
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }

    public static string ForgotPasswordEmail(string recipientName, string resetLink, int expiresMinutes)
    {
        var safeName = string.IsNullOrWhiteSpace(recipientName) ? "there" : recipientName;

        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
  <title>Reset Your Password</title>
</head>
<body style=""margin:0;padding:0;background:#f6f7fb;font-family:Arial,Helvetica,sans-serif;color:#1f2937;"">
  <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
    <tr>
      <td align=""center"" style=""padding:24px;"">
        <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""600"" style=""background:#ffffff;border-radius:12px;overflow:hidden;border:1px solid #e5e7eb;"">
          <tr>
            <td style=""padding:24px 32px;background:#0f172a;color:#ffffff;"">
              <h1 style=""margin:0;font-size:20px;font-weight:600;"">LiveExpert.AI</h1>
              <p style=""margin:6px 0 0;font-size:14px;color:#cbd5f5;"">Password Reset</p>
            </td>
          </tr>
          <tr>
            <td style=""padding:32px;"">
              <p style=""margin:0 0 12px;font-size:16px;"">Hi {safeName},</p>
              <p style=""margin:0 0 20px;font-size:14px;line-height:1.6;"">
                We received a request to reset your password. Click the button below to set a new password. This link expires in {expiresMinutes} minutes.
              </p>
              <div style=""text-align:center;margin:32px 0;"">
                <a href=""{resetLink}"" style=""display:inline-block;padding:14px 28px;background:#3b82f6;color:#ffffff;text-decoration:none;border-radius:8px;font-weight:600;font-size:14px;"">Reset Password</a>
              </div>
              <p style=""margin:0;font-size:12px;color:#6b7280;"">
                If you didn’t request this, you can safely ignore this email.
              </p>
            </td>
          </tr>
          <tr>
            <td style=""padding:20px 32px;border-top:1px solid #e5e7eb;font-size:12px;color:#9ca3af;"">
              © 2026 LiveExpert.AI. All rights reserved.
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }

    public static string SessionScheduledEmail(string recipientName, string sessionTitle, string sessionTime, string joinLink, string otherPartyName, string role)
    {
        var safeName = string.IsNullOrWhiteSpace(recipientName) ? "there" : recipientName;
        var roleText = role == "Tutor" ? $"Student: {otherPartyName}" : $"Tutor: {otherPartyName}";

        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
  <title>Session Scheduled</title>
</head>
<body style=""margin:0;padding:0;background:#f6f7fb;font-family:Arial,Helvetica,sans-serif;color:#1f2937;"">
  <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
    <tr>
      <td align=""center"" style=""padding:24px;"">
        <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""600"" style=""background:#ffffff;border-radius:12px;overflow:hidden;border:1px solid #e5e7eb;"">
          <tr>
            <td style=""padding:24px 32px;background:#059669;color:#ffffff;"">
              <h1 style=""margin:0;font-size:20px;font-weight:600;"">LiveExpert.AI</h1>
              <p style=""margin:6px 0 0;font-size:14px;color:#d1fae5;"">Session Confirmation</p>
            </td>
          </tr>
          <tr>
            <td style=""padding:32px;"">
              <p style=""margin:0 0 12px;font-size:16px;"">Hi {safeName},</p>
              <p style=""margin:0 0 20px;font-size:14px;line-height:1.6;"">
                Your session has been scheduled successfully. Here are the details:
              </p>
              <div style=""background:#f9fafb;padding:20px;border-radius:8px;margin-bottom:24px;"">
                 <p style=""margin:0 0 8px;font-size:14px;""><strong>Title:</strong> {sessionTitle}</p>
                 <p style=""margin:0 0 8px;font-size:14px;""><strong>Time:</strong> {sessionTime}</p>
                 <p style=""margin:0;font-size:14px;""><strong>{roleText}</strong></p>
              </div>
              <div style=""text-align:center;margin:32px 0;"">
                <a href=""{joinLink}"" style=""display:inline-block;padding:14px 28px;background:#059669;color:#ffffff;text-decoration:none;border-radius:8px;font-weight:600;font-size:14px;"">Join Session</a>
              </div>
            </td>
          </tr>
          <tr>
            <td style=""padding:20px 32px;border-top:1px solid #e5e7eb;font-size:12px;color:#9ca3af;"">
              © 2026 LiveExpert.AI. All rights reserved.
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }

    public static string SessionCancelledEmail(string recipientName, string sessionTitle, string sessionTime, string cancelledBy)
    {
        var safeName = string.IsNullOrWhiteSpace(recipientName) ? "there" : recipientName;

        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
  <title>Session Cancelled</title>
</head>
<body style=""margin:0;padding:0;background:#f6f7fb;font-family:Arial,Helvetica,sans-serif;color:#1f2937;"">
  <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
    <tr>
      <td align=""center"" style=""padding:24px;"">
        <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""600"" style=""background:#ffffff;border-radius:12px;overflow:hidden;border:1px solid #e5e7eb;"">
          <tr>
            <td style=""padding:24px 32px;background:#dc2626;color:#ffffff;"">
              <h1 style=""margin:0;font-size:20px;font-weight:600;"">LiveExpert.AI</h1>
              <p style=""margin:6px 0 0;font-size:14px;color:#fecaca;"">Cancellation Notice</p>
            </td>
          </tr>
          <tr>
            <td style=""padding:32px;"">
              <p style=""margin:0 0 12px;font-size:16px;"">Hi {safeName},</p>
              <p style=""margin:0 0 20px;font-size:14px;line-height:1.6;"">
                The session ""{sessionTitle}"" scheduled for {sessionTime} has been cancelled by {cancelledBy}. If any credits were deducted, they have been refunded to your account.
              </p>
            </td>
          </tr>
          <tr>
            <td style=""padding:20px 32px;border-top:1px solid #e5e7eb;font-size:12px;color:#9ca3af;"">
              © 2026 LiveExpert.AI. All rights reserved.
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }

    public static string SessionFeedbackEmail(string studentName, string tutorName, string sessionTitle, string feedbackLink)
    {
        var safeName = string.IsNullOrWhiteSpace(studentName) ? "there" : studentName;

        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
  <title>Session Feedback</title>
</head>
<body style=""margin:0;padding:0;background:#f6f7fb;font-family:Arial,Helvetica,sans-serif;color:#1f2937;"">
  <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
    <tr>
      <td align=""center"" style=""padding:24px;"">
        <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""600"" style=""background:#ffffff;border-radius:12px;overflow:hidden;border:1px solid #e5e7eb;"">
          <tr>
            <td style=""padding:24px 32px;background:#4f46e5;color:#ffffff;"">
              <h1 style=""margin:0;font-size:20px;font-weight:600;"">LiveExpert.AI</h1>
              <p style=""margin:6px 0 0;font-size:14px;color:#c7d2fe;"">Share Your Experience</p>
            </td>
          </tr>
          <tr>
            <td style=""padding:32px;"">
              <p style=""margin:0 0 12px;font-size:16px;"">Hi {safeName},</p>
              <p style=""margin:0 0 20px;font-size:14px;line-height:1.6;"">
                We hope you had a great learning session ""{sessionTitle}"" with {tutorName}. Your feedback helps us maintain high quality on LiveExpert.AI.
              </p>
              <div style=""text-align:center;margin:32px 0;"">
                <a href=""{feedbackLink}"" style=""display:inline-block;padding:14px 28px;background:#4f46e5;color:#ffffff;text-decoration:none;border-radius:8px;font-weight:600;font-size:14px;"">Rate Experience</a>
              </div>
            </td>
          </tr>
          <tr>
            <td style=""padding:20px 32px;border-top:1px solid #e5e7eb;font-size:12px;color:#9ca3af;"">
              © 2026 LiveExpert.AI. All rights reserved.
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }

    public static string SessionReminderEmail(string recipientName, string sessionTitle, string sessionTime, string joinLink)
    {
        var safeName = string.IsNullOrWhiteSpace(recipientName) ? "there" : recipientName;

        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
  <title>Session Reminder</title>
</head>
<body style=""margin:0;padding:0;background:#f6f7fb;font-family:Arial,Helvetica,sans-serif;color:#1f2937;"">
  <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
    <tr>
      <td align=""center"" style=""padding:24px;"">
        <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""600"" style=""background:#ffffff;border-radius:12px;overflow:hidden;border:1px solid #e5e7eb;"">
          <tr>
            <td style=""padding:24px 32px;background:#f59e0b;color:#ffffff;"">
              <h1 style=""margin:0;font-size:20px;font-weight:600;"">LiveExpert.AI</h1>
              <p style=""margin:6px 0 0;font-size:14px;color:#fef3c7;"">Reminder</p>
            </td>
          </tr>
          <tr>
            <td style=""padding:32px;"">
              <p style=""margin:0 0 12px;font-size:16px;"">Hi {safeName},</p>
              <p style=""margin:0 0 20px;font-size:14px;line-height:1.6;"">
                This is a friendly reminder that your session ""{sessionTitle}"" is starting in 15 minutes.
              </p>
              <div style=""background:#fff7ed;padding:20px;border-radius:8px;margin-bottom:24px;"">
                 <p style=""margin:0 0 8px;font-size:14px;""><strong>Time:</strong> {sessionTime}</p>
                 <p style=""margin:0;font-size:14px;""><strong>Join via:</strong> <a href=""{joinLink}"" style=""color:#d97706;"">{joinLink}</a></p>
              </div>
              <div style=""text-align:center;margin:32px 0;"">
                <a href=""{joinLink}"" style=""display:inline-block;padding:14px 28px;background:#f59e0b;color:#ffffff;text-decoration:none;border-radius:8px;font-weight:600;font-size:14px;"">Join Now</a>
              </div>
            </td>
          </tr>
          <tr>
            <td style=""padding:20px 32px;border-top:1px solid #e5e7eb;font-size:12px;color:#9ca3af;"">
              © 2026 LiveExpert.AI. All rights reserved.
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }

    public static string ThanksForBookingEmail(string recipientName)
    {
        var safeName = string.IsNullOrWhiteSpace(recipientName) ? "there" : recipientName;

        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
  <title>Thanks for Booking</title>
</head>
<body style=""margin:0;padding:0;background:#f6f7fb;font-family:Arial,Helvetica,sans-serif;color:#1f2937;"">
  <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
    <tr>
      <td align=""center"" style=""padding:24px;"">
        <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""600"" style=""background:#ffffff;border-radius:12px;overflow:hidden;border:1px solid #e5e7eb;"">
          <tr>
            <td style=""padding:24px 32px;background:#0f172a;color:#ffffff;"">
              <h1 style=""margin:0;font-size:20px;font-weight:600;"">LiveExpert.AI</h1>
              <p style=""margin:6px 0 0;font-size:14px;color:#cbd5f5;"">Booking Submitted</p>
            </td>
          </tr>
          <tr>
            <td style=""padding:32px;"">
              <p style=""margin:0 0 12px;font-size:16px;"">Hi {safeName},</p>
              <p style=""margin:0 0 20px;font-size:14px;line-height:1.6;"">
                Thanks for booking a session on LiveExpert.AI. We've sent your request to the tutor for approval. You'll be notified once they respond.
              </p>
              <p style=""margin:0;font-size:14px;line-height:1.6;"">
                You can view your booking status in your student dashboard.
              </p>
            </td>
          </tr>
          <tr>
            <td style=""padding:20px 32px;border-top:1px solid #e5e7eb;font-size:12px;color:#9ca3af;"">
              © 2026 LiveExpert.AI. All rights reserved.
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }

    public static string PaymentSuccessEmail(string recipientName, string sessionTitle, decimal amount, string transactionId)
    {
        var safeName = string.IsNullOrWhiteSpace(recipientName) ? "there" : recipientName;

        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
  <title>Payment Successful</title>
</head>
<body style=""margin:0;padding:0;background:#f6f7fb;font-family:Arial,Helvetica,sans-serif;color:#1f2937;"">
  <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
    <tr>
      <td align=""center"" style=""padding:24px;"">
        <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""600"" style=""background:#ffffff;border-radius:12px;overflow:hidden;border:1px solid #e5e7eb;"">
          <tr>
            <td style=""padding:24px 32px;background:#059669;color:#ffffff;"">
              <h1 style=""margin:0;font-size:20px;font-weight:600;"">LiveExpert.AI</h1>
              <p style=""margin:6px 0 0;font-size:14px;color:#d1fae5;"">Payment Successful</p>
            </td>
          </tr>
          <tr>
            <td style=""padding:32px;"">
              <p style=""margin:0 0 12px;font-size:16px;"">Hi {safeName},</p>
              <p style=""margin:0 0 20px;font-size:14px;line-height:1.6;"">
                Your payment for the session ""{sessionTitle}"" was successful. We have notified the tutor to prepare for the session.
              </p>
              <div style=""background:#f0fdf4;padding:20px;border-radius:8px;margin-bottom:24px;border:1px solid #bbf7d0;"">
                 <p style=""margin:0 0 8px;font-size:14px;""><strong>Amount Paid:</strong> ₹{amount:N2}</p>
                 <p style=""margin:0;font-size:14px;""><strong>Transaction ID:</strong> {transactionId}</p>
              </div>
              <p style=""margin:0;font-size:14px;line-height:1.6;"">
                You can join the session from your dashboard at the scheduled time.
              </p>
            </td>
          </tr>
          <tr>
            <td style=""padding:20px 32px;border-top:1px solid #e5e7eb;font-size:12px;color:#9ca3af;"">
              © 2026 LiveExpert.AI. All rights reserved.
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }
    public static string TutorProfileUnderReviewEmail(string tutorName)
    {
        var safeName = string.IsNullOrWhiteSpace(tutorName) ? "there" : tutorName;

        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
  <title>Profile Under Review</title>
</head>
<body style=""margin:0;padding:0;background:#f6f7fb;font-family:Arial,Helvetica,sans-serif;color:#1f2937;"">
  <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
    <tr>
      <td align=""center"" style=""padding:24px;"">
        <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""600"" style=""background:#ffffff;border-radius:12px;overflow:hidden;border:1px solid #e5e7eb;"">
          <tr>
            <td style=""padding:24px 32px;background:#4f46e5;color:#ffffff;"">
              <h1 style=""margin:0;font-size:20px;font-weight:600;"">LiveExpert.AI</h1>
              <p style=""margin:6px 0 0;font-size:14px;color:#e0e7ff;"">Profile Under Review</p>
            </td>
          </tr>
          <tr>
            <td style=""padding:32px;"">
              <p style=""margin:0 0 12px;font-size:16px;"">Hi {safeName},</p>
              <p style=""margin:0 0 20px;font-size:14px;line-height:1.6;"">
                Thank you for submitting your tutor profile! Our application team has received your information and is currently reviewing it.
              </p>
              <p style=""margin:0 0 20px;font-size:14px;line-height:1.6;"">
                This process typically takes 1-2 business days. We'll send you an email as soon as your account is verified and ready to go.
              </p>
              <p style=""margin:0;font-size:14px;line-height:1.6;"">
                In the meantime, you can explore the platform and prepare your first session materials.
              </p>
            </td>
          </tr>
          <tr>
            <td style=""padding:20px 32px;border-top:1px solid #e5e7eb;font-size:12px;color:#9ca3af;"">
              © 2026 LiveExpert.AI. All rights reserved.
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }
}
