namespace LiveExpert.Application.Common;

public static class NotificationTemplates
{
    public static (string Subject, string Body) TutorWelcomeEmail(string tutorName, string profileLink)
    {
        return (
            "Welcome to LiveExpert.ai – Complete Your Tutor Profile",
            $@"Hi {tutorName},

Welcome to LiveExpert.ai 🎉

Your tutor account has been created successfully.
Please complete your profile and submit it for verification to start accepting sessions and students.

👉 Complete Profile: {profileLink}

We’re excited to have you onboard. Let’s build your teaching journey together 🚀

— Team LiveExpert.ai"
        );
    }

    public static string TutorWelcomeWhatsApp(string tutorName, string profileLink)
    {
        return $@"👋 Hi {tutorName}!

Welcome to LiveExpert.ai.
Your tutor account is created successfully.

Please complete your profile and submit it for verification to start teaching.

🔗 {profileLink}";
    }

    public static (string Subject, string Body) EmailVerification(string userName, string verificationCode, int expiryMinutes)
    {
        return (
            "Verify Your Email – LiveExpert.ai",
            $@"Hi {userName},

Please use the verification code below to verify your email address:

🔐 {verificationCode}

This code is valid for {expiryMinutes} minutes.

— Team LiveExpert.ai"
        );
    }

    public static (string Subject, string Body) ForgotPasswordEmail(string userName, string resetLink, int expiryMinutes)
    {
        return (
            "Reset Your Password – LiveExpert.ai",
            $@"Hi {userName},

We received a request to reset your password. Click the link below to set a new password:

👉 Reset Password: {resetLink}

This link is valid for {expiryMinutes} minutes. If you didn't request this, you can safely ignore this email.

— Team LiveExpert.ai"
        );
    }

    public static (string Subject, string Body) TutorVerificationApproved(string tutorName, string dashboardLink)
    {
        return (
            "Your Tutor Profile Is Verified 🎉",
            $@"Hi {tutorName},

Great news! 🎉
Your tutor profile has been verified and approved.

You can now log in, connect your Google Calendar, and start accepting sessions.

👉 Go to Dashboard: {dashboardLink}

Let’s grow together 🚀

— Team LiveExpert.ai"
        );
    }

    public static (string Subject, string Body) StudentRequestedSessionEmail(string tutorName, string studentName, string sessionTopic, string sessionTime, string requestLink)
    {
        return (
            $"New Session Request from {studentName}",
            $@"Hi {tutorName},

{studentName} has requested a session with you.

Session Topic: {sessionTopic}
Preferred Time: {sessionTime}

👉 Review Request: {requestLink}

— Team LiveExpert.ai"
        );
    }

    public static string StudentRequestedChatWhatsApp(string tutorName, string studentName, string chatLink)
    {
        return $@"💬 Hi {tutorName},

{studentName} has requested to chat with you on LiveExpert.ai.

👉 View Request: {chatLink}";
    }

    public static string StudentRequestedSessionWhatsApp(string tutorName, string studentName, string sessionTime, string requestLink)
    {
        return $@"🧑‍🏫 Hi {tutorName},

{studentName} requested a class session on {sessionTime}.

👉 Review request: {requestLink}";
    }

    public static (string Subject, string Body) SessionBookedTutorEmail(string tutorName, string studentName, string sessionDateTime, string sessionLink)
    {
        return (
            $"Session Booked with {studentName}",
            $@"Hi {tutorName},

Your session with {studentName} has been booked successfully.

🗓 Date & Time: {sessionDateTime}
🎥 Platform: Google Meet

👉 View Session: {sessionLink}

— Team LiveExpert.ai"
        );
    }

    public static (string Subject, string Body) SessionCreatedTutorEmail(string tutorName, string sessionTitle, string sessionDateTime)
    {
        return (
            "Session Created Successfully",
            $@"Hi {tutorName},

Your session “{sessionTitle}” has been created successfully.

🗓 {sessionDateTime}

Students can now book this session.

— Team LiveExpert.ai"
        );
    }

    public static (string Subject, string Body) SessionCancelledEmail(string userName, string sessionTitle, string sessionDateTime, string cancelledBy)
    {
        return (
            $"Session Cancelled: {sessionTitle}",
            $@"Hi {userName},

The session ""{sessionTitle}"" scheduled for {sessionDateTime} has been cancelled by {cancelledBy}.

If any credits were deducted, they have been refunded to your account.

— Team LiveExpert.ai"
        );
    }

    public static (string Subject, string Body) SessionReminderEmail(string userName, string sessionTitle, string sessionTime, string joinLink)
    {
        return (
            $"Reminder: Your Session starts in 15 minutes",
            $@"Hi {userName},

Your session ""{sessionTitle}"" is starting soon.

🗓 Time: {sessionTime}
🎥 Join via: {joinLink}

Please be on time for the best learning experience.

— Team LiveExpert.ai"
        );
    }

    public static (string Subject, string Body) SessionFeedbackEmail(string studentName, string tutorName, string sessionTitle, string feedbackLink)
    {
        return (
            $"How was your session with {tutorName}?",
            $@"Hi {studentName},

We hope you enjoyed your session ""{sessionTitle}"" with {tutorName}.

Your feedback helps us maintain high quality on LiveExpert.ai. Please take a moment to rate your experience:

👉 Give Feedback: {feedbackLink}

Thank you for being part of our community!

— Team LiveExpert.ai"
        );
    }

    public static string SessionReminderTutorWhatsApp(string sessionTitle, string reminderTime)
    {
        return $@"⏰ Reminder:

Your session {sessionTitle} starts in {reminderTime}.

👉 Join via LiveExpert.ai dashboard.";
    }

    public static (string Subject, string Body) PayoutSuccessfulTutorEmail(string tutorName, string amount, string bankName, string payoutDate)
    {
        return (
            "Payout Successful 💰",
            $@"Hi {tutorName},

Your payout of ₹{amount} has been processed successfully.

💳 Bank: {bankName}
📅 Date: {payoutDate}

Keep teaching and earning 🚀

— Team LiveExpert.ai"
        );
    }

    public static string NewFollowerTutorWhatsApp(string tutorName, string studentName)
    {
        return $@"🎉 Hi {tutorName}!

{studentName} has started following you on LiveExpert.ai.

Your profile is getting noticed 👀🔥";
    }

    public static (string Subject, string Body) StudentWelcomeEmail(string studentName, string dashboardLink)
    {
        return (
            "Welcome to LiveExpert.ai 🎉",
            $@"Hi {studentName},

Welcome to LiveExpert.ai 🎉

You’re all set to explore tutors, book sessions, and start learning.

👉 Start Learning: {dashboardLink}

— Team LiveExpert.ai"
        );
    }

    public static string StudentWelcomeWhatsApp(string studentName, string dashboardLink)
    {
        return $@"👋 Hi {studentName}!

Welcome to LiveExpert.ai.

Start exploring tutors and book your first session today 🚀
🔗 {dashboardLink}";
    }

    public static (string Subject, string Body) SessionBookedStudentEmail(string studentName, string tutorName, string sessionDateTime)
    {
        return (
            "Session Booked Successfully",
            $@"Hi {studentName},

Your session with {tutorName} is confirmed 🎉

🗓 Date & Time: {sessionDateTime}
🎥 Platform: Google Meet

👉 Join from dashboard when the session starts.

— Team LiveExpert.ai"
        );
    }

    public static string TutorAcceptedRequestWhatsApp(string studentName, string tutorName, string requestLink)
    {
        return $@"✅ Good news {studentName}!

{tutorName} has accepted your request on LiveExpert.ai.

👉 Check details: {requestLink}";
    }

    public static (string Subject, string Body) ThanksForBookingEmail(string studentName)
    {
        return (
            "Thanks for Booking Your Session",
            $@"Hi {studentName},

Thanks for booking a session on LiveExpert.ai 🙌

We hope you have a great learning experience.

— Team LiveExpert.ai"
        );
    }

    public static string RegistrationBonusWhatsApp(string studentName, string bonusPoints)
    {
        return $@"🎁 Hi {studentName}!

Your registration bonus of {bonusPoints} points has been added to your account.

Start booking sessions now 🚀";
    }

    public static string ReferralBonusWhatsApp(string referredStudentName, string bonusPoints)
    {
        return $@"🎉 Referral Success!

{referredStudentName} booked a session using your referral.

You’ve earned {bonusPoints} points 🎁

Keep sharing & earning 🚀";
    }

    public static string StudentInactiveReminderWhatsApp(string studentName, string exploreLink)
    {
        return $@"👋 Hi {studentName}

We noticed you haven’t booked a session recently.

Discover new tutors and continue learning 🚀
👉 {exploreLink}";
    }

    public static (string Subject, string Body) TutorInactiveReminderEmail(string tutorName, string createSessionLink)
    {
        return (
            "Your students are waiting 👀",
            $@"Hi {tutorName},

You haven’t created any sessions recently.

Create a session and start earning on LiveExpert.ai 💰

👉 {createSessionLink}"
        );
    }

    public static (string Subject, string Body) TutorMilestoneFollowersEmail(string tutorName, string followerCount)
    {
        return (
            $"🎉 Congrats {tutorName}!",
            $@"🎉 Congrats {tutorName}!

You’ve crossed {followerCount} followers on LiveExpert.ai.

Keep growing your audience 🚀"
        );
    }

    public static string StudentLearningMilestoneWhatsApp(string studentName, string sessionCount)
    {
        return $@"🎉 Nice work {studentName}!

You’ve completed {sessionCount} sessions on LiveExpert.ai.

Keep learning 💪";
    }

    public static string LowCreditsReminderWhatsApp(string studentName, string pointsLeft, string addCreditsLink)
    {
        return $@"⚠️ Hi {studentName}

You have only {pointsLeft} points left.

Add points to continue booking sessions 🚀
👉 {addCreditsLink}";
    }

    public static (string Subject, string Body) MonthlyEarningsUpdateEmail(string tutorName, string earnings)
    {
        return (
            $"Monthly earnings update 💰",
            $@"💰 Hi {tutorName},

You earned ₹{earnings} this month on LiveExpert.ai.

Keep the momentum going 🚀"
        );
    }

    // WhatsApp Cloud API Template Parameters
    public static List<string> WelcomeWhatsAppParameters(string userName) => new() { userName };

    public static List<string> SessionScheduledWhatsAppParameters(
        string userName, 
        string subject, 
        string date, 
        string time, 
        string tutorName, 
        string sessionLink) => new() 
    { 
        userName, 
        subject, 
        date, 
        time, 
        tutorName, 
        sessionLink 
    };
}
