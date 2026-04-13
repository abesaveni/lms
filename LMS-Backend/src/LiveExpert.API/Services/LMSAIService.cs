using LiveExpert.API.Services;
using LiveExpert.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using LiveExpert.Domain.Entities;

namespace LiveExpert.API.Services
{
    public class LMSAIService
    {
        private readonly ClaudeAIService _claudeService;
        private readonly ApplicationDbContext _context;

        public LMSAIService(ClaudeAIService claudeService, ApplicationDbContext context)
        {
            _claudeService = claudeService;
            _context = context;
        }

        public async Task<string> TutorMatch(string subject, string level)
        {
            var tutors = await _context.TutorProfiles
                .Include(tp => tp.User)
                .Where(tp => tp.IsVisible && tp.IsProfileComplete)
                .OrderByDescending(tp => tp.AverageRating)
                .ToListAsync();

            var tutorListBuilder = new StringBuilder();
            if (tutors.Any())
            {
                foreach (var tp in tutors)
                {
                    var name = $"{tp.User.FirstName} {tp.User.LastName}";
                    var skills = string.IsNullOrEmpty(tp.Skills) ? "General expertise" : tp.Skills;
                    var education = string.IsNullOrEmpty(tp.Education) ? "" : $", Qualifications: {tp.Education}";
                    tutorListBuilder.AppendLine($"- {name}: {skills} specialist with {tp.YearsOfExperience} years experience, rating {tp.AverageRating:F1}{education}");
                }
            }
            else
            {
                tutorListBuilder.AppendLine("No specific tutors found matching this criteria in our database yet.");
            }

            var tutorList = tutorListBuilder.ToString();

            var prompt = $@"You are the LiveExpert.AI Matching Engine. Your only source of truth for tutors is the list provided below.

STUDENT SUBJECT: {subject}
STUDENT LEVEL: {level}
AVAILABLE TUTORS FROM DATABASE:
{tutorList}

INSTRUCTIONS:
1. Start directly with 'BEST TUTOR: [Name]'.
2. Provide a 'REASONING' section explaining why this specific tutor is the best match for the student's level and subject.
3. If no tutors in the list have any relevance to the requested subjects, state exactly: 'Currently, there are no tutors in our database matching your specific criteria. We recommend checking back soon or broadening your search.'
4. If a tutor matches some but not all requested subjects, recommend them but explicitly note which subjects they do NOT cover.
5. NEVER invent names or skills. If a tutor is not in the list above, they do not exist.
6. Provide a concise, professional response with no generic placeholders or templates.";

            return await _claudeService.GenerateContentAsync(prompt);
        }

        public async Task<string> StudyPlan(string subject, string goal, string time, string duration)
        {
            var prompt = $@"You are the LiveExpert.AI Academic Planner. Create a precise, actionable study schedule.

SUBJECT: {subject}
GOAL: {goal}
TIME PER SESSION: {time}
DURATION: {duration}

Return a complete, structured weekly study plan immediately. Do not ask for more details. If input is generic, generate a professional 4-week plan as a demonstration.";

            return await _claudeService.GenerateContentAsync(prompt);
        }

        public async Task<string> SessionSummary(string transcript)
        {
            var prompt = $@"You are the LiveExpert.AI Transcription Analyst.

TRANSCRIPT: {transcript}

Return exactly three clearly labeled sections:
1. SUMMARY — Key concepts and topics covered
2. WEAK AREAS — Specific gaps or struggles identified
3. NEXT STEPS — Concrete follow-up actions for the student

If the transcript is empty or very brief, explain how a typical tutoring session would be summarized using those three sections.";

            return await _claudeService.GenerateContentAsync(prompt);
        }

        public async Task<string> QuizGenerator(string topic, string difficulty)
        {
            var prompt = $@"You are the LiveExpert.AI Assessment Creator.

TOPIC: {topic}
DIFFICULTY: {difficulty}

Generate exactly 10 Multiple Choice Questions (MCQs) with 4 options each. Mark the correct answer clearly. Start immediately — do not ask for more information. If topic is generic, generate a quiz on general science or logic.";

            return await _claudeService.GenerateContentAsync(prompt);
        }

        public async Task<string> Flashcards(string topic)
        {
            var prompt = $@"You are the LiveExpert.AI Flashcard Generator.

TOPIC: {topic}

Generate 10 flashcards in this format:
Front: [Question]
Back: [Answer]

Start immediately. If topic is generic, use 'World History' as the default topic.";

            return await _claudeService.GenerateContentAsync(prompt);
        }

        public async Task<string> HomeworkHelper(string question)
        {
            var prompt = $@"You are the LiveExpert.AI Subject Expert helping a student.

STUDENT QUESTION: {question}

Provide a clear, step-by-step academic explanation. Show your reasoning process. If the question is ambiguous, provide a thorough explanation of the most likely interpretation. Do not ask for more context — answer immediately.";

            return await _claudeService.GenerateContentAsync(prompt);
        }

        public async Task<string> GenerateSessionNotes(string transcript, string focusAreas)
        {
            var prompt = $@"You are the LiveExpert.AI Tutor Assistant. Create professional session notes for the tutor.

TRANSCRIPT: {transcript}
FOCUS AREAS: {focusAreas}

Provide:
1. A detailed summary of key concepts covered
2. Student breakthroughs or strengths observed
3. Specific challenges the student faced
4. Suggested homework or follow-up tasks

No templates or example text — write directly from the content provided.";

            return await _claudeService.GenerateContentAsync(prompt);
        }

        public async Task<string> GenerateLessonPlan(string subject, string level, string topic, string objectives)
        {
            var prompt = $@"You are the LiveExpert.AI Pedagogy Expert. Create a structured lesson plan.

SUBJECT: {subject}
LEVEL: {level}
TOPIC: {topic}
LEARNING OBJECTIVES: {objectives}

Provide a step-by-step breakdown:
- Introduction (with time estimate)
- Core Content (with time estimate)
- Practice Activities (with time estimate)
- Wrap-up and Assessment

Include suggested questions to check for understanding. Generate the plan immediately.";

            return await _claudeService.GenerateContentAsync(prompt);
        }

        public async Task<string> GenerateProgressReport(string studentName, string feedbackHistory)
        {
            var prompt = $@"You are the LiveExpert.AI Performance Analyst. Generate a comprehensive student progress report.

STUDENT NAME: {studentName}
RECENT FEEDBACK HISTORY: {feedbackHistory}

Provide:
1. Performance trend analysis
2. Consistent strengths identified
3. Recurring weaknesses requiring attention
4. Professional 'Teacher's Comment' section
5. Recommended focus areas for the next month of study";

            return await _claudeService.GenerateContentAsync(prompt);
        }

        public async Task<string> ChurnPrediction(string usageData)
        {
            var prompt = $@"You are the LiveExpert.AI Retention Strategist. Analyze usage data to predict student churn risk.

USAGE DATA: {usageData}

Provide:
1. CHURN RISK LEVEL: Low / Medium / High
2. TOP 3 RISK FACTORS identified from the data
3. RETENTION ACTION PLAN with specific interventions to keep this user active

Keep the analysis strictly data-driven.";

            return await _claudeService.GenerateContentAsync(prompt);
        }

        public async Task<string> RevenueAnalytics(string financialData, string period)
        {
            var prompt = $@"You are the LiveExpert.AI Financial Analyst. Provide revenue insights.

PERIOD: {period}
FINANCIAL DATA: {financialData}

Provide:
1. Total revenue summary and profit margins
2. Highest-growing subject categories
3. REVENUE FORECAST for the next period
4. Any fiscal anomalies or concerns identified";

            return await _claudeService.GenerateContentAsync(prompt);
        }

        public async Task<string> SupportTriage(string ticketContent)
        {
            var prompt = $@"You are the LiveExpert.AI Support Lead. Categorize and prioritize this support ticket.

TICKET: {ticketContent}

Provide:
1. PRIORITY LEVEL: P0 (Critical) / P1 (High) / P2 (Medium) / P3 (Low)
2. CATEGORY: Technical / Billing / Academic / Account
3. SUGGESTED INITIAL RESPONSE for the support agent to send";

            return await _claudeService.GenerateContentAsync(prompt);
        }

        public async Task<string> FraudDetection(string transactionData, string userIp)
        {
            var prompt = $@"You are the LiveExpert.AI Security Officer. Analyze this transaction for potential fraud.

TRANSACTION: {transactionData}
USER IP: {userIp}

Provide:
1. FRAUD RISK SCORE: 0-100 (0 = clean, 100 = definite fraud)
2. RED FLAGS: List any suspicious indicators found
3. RECOMMENDED ACTION: Approve / Flag for Review / Block";

            return await _claudeService.GenerateContentAsync(prompt);
        }

        public async Task<string> DisputeResolution(string details, string transcript)
        {
            var prompt = $@"You are the LiveExpert.AI Mediator. Analyze this session dispute neutrally.

DISPUTE DETAILS: {details}
SESSION TRANSCRIPT: {transcript}

Provide:
1. FACTUAL SUMMARY: Neutral summary based only on the transcript
2. POLICY VIOLATIONS: Any violations identified (by tutor or student), or 'None found'
3. RECOMMENDED RESOLUTION: Refund / Partial Refund / No Refund / Warning Issued — with rationale";

            return await _claudeService.GenerateContentAsync(prompt);
        }

        public async Task<string> GeneralChat(string message, string? userContext = null)
        {
            var isAdmin = userContext?.ToLower().Contains("role: admin") ?? false;

            string tutorContext = "";
            bool isAskingForTutor = message.ToLower().Contains("tutor") || message.ToLower().Contains("teacher")
                || message.ToLower().Contains("match") || message.ToLower().Contains("find");

            if (isAskingForTutor && !isAdmin)
            {
                var tutors = await _context.TutorProfiles
                    .Include(tp => tp.User)
                    .Where(tp => tp.IsVisible && tp.IsProfileComplete)
                    .Take(10)
                    .ToListAsync();

                var sb = new StringBuilder();
                sb.AppendLine("REAL TUTORS IN OUR DATABASE:");
                foreach (var tp in tutors)
                {
                    sb.AppendLine($"- {tp.User.FirstName} {tp.User.LastName}: {tp.Skills ?? "General"} ({tp.Headline ?? ""})");
                }
                tutorContext = sb.ToString();
            }

            var personaPrompt = isAdmin
                ? @"You are the LiveExpert.AI Platform Command Center — a smart, direct administrative AI assistant for platform managers. You handle security analysis, financial analytics, tutor verifications, and user behaviour reports. Be concise, data-focused, and professional. When asked for a report or analysis, generate it immediately without asking unnecessary follow-up questions."
                : @"You're the AI assistant built into LiveExpert.AI — a smart, warm, and genuinely helpful companion for students and tutors. You talk like a real person: conversational, natural, using contractions and casual phrasing. You're knowledgeable about a wide range of topics and you help with whatever the user needs — not just platform-specific things.

Your personality:
- Friendly and direct — get to the point without being robotic
- Use contractions (I'm, you're, let's, don't, that's) — sound human, not corporate
- Match the user's energy: casual when they're casual, focused when they're serious
- Never start with 'Sure!' or 'Great question!' — just respond naturally
- Mix short punchy lines with fuller explanations — don't always dump bullet lists
- Show genuine interest in what the user is asking

You help with: study plans, quiz generation, flashcards, homework help, tutor matching, lesson planning, coding questions, career advice, general knowledge — anything the user brings up.";

            var capabilitiesPrompt = isAdmin
                ? "Admin tools available: fraud detection, revenue forecasting, churn prediction, support triage, dispute mediation, tutor verification reports."
                : "";

            var tutorRule = isAskingForTutor && !isAdmin
                ? $@"TUTOR MATCHING RULE — only recommend tutors from this real list:
{tutorContext}
If none match or the list is empty, honestly say no matching tutor is available right now. Never make up names."
                : "";

            var prompt = $@"{personaPrompt}
{capabilitiesPrompt}

{tutorRule}

USER CONTEXT: {userContext ?? "None"}
USER MESSAGE: {message}

Respond naturally. If the user asks for a quiz, plan, report, or any specific output — generate it immediately without asking unnecessary follow-up questions unless you genuinely need more info to do it right.";

            return await _claudeService.GenerateContentAsync(prompt);
        }

        public async Task<List<string>> GetAvailableModelsAsync()
        {
            return await _claudeService.ListAvailableModelsAsync();
        }
    }
}
