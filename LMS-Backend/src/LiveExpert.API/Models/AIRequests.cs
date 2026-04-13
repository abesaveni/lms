namespace LiveExpert.API.Models
{
    public class TutorMatchRequest
    {
        public string Subject { get; set; }
        public string Level { get; set; }
    }

    public class StudyPlanRequest
    {
        public string Subject { get; set; }
        public string Goal { get; set; }
        public string Time { get; set; }
        public string Duration { get; set; }
    }

    public class SessionSummaryRequest
    {
        public string Transcript { get; set; }
    }

    public class QuizRequest
    {
        public string Topic { get; set; }
        public string Difficulty { get; set; }
    }

    public class FlashcardRequest
    {
        public string Topic { get; set; }
    }

    public class HomeworkRequest
    {
        public string Question { get; set; }
    }

    public class SessionNotesRequest
    {
        public string Transcript { get; set; }
        public string FocusAreas { get; set; }
    }

    public class LessonPlanRequest
    {
        public string Subject { get; set; }
        public string Level { get; set; }
        public string Topic { get; set; }
        public string LearningObjectives { get; set; }
    }

    public class ProgressReportRequest
    {
        public string StudentName { get; set; }
        public string FeedbackHistory { get; set; }
    }

    public class ChurnPredictionRequest
    {
        public string StudentUsageData { get; set; }
    }

    public class RevenueAnalyticsRequest
    {
        public string FinancialData { get; set; }
        public string Period { get; set; }
    }

    public class SupportTriageRequest
    {
        public string TicketContent { get; set; }
    }

    public class FraudDetectionRequest
    {
        public string TransactionData { get; set; }
        public string UserIp { get; set; }
    }

    public class DisputeResolutionRequest
    {
        public string DisputeDetails { get; set; }
        public string SessionTranscript { get; set; }
    }
}