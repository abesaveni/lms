import { apiPost, getDefaultHeaders } from './api'

const API_BASE_URL = (import.meta as any).env?.VITE_API_BASE_URL || 'http://localhost:5128/api'

// ---------------------------------------------------------------------------
// Custom error for subscription / trial expiry (HTTP 402)
// ---------------------------------------------------------------------------
export class SubscriptionRequiredError extends Error {
  code = 'SUBSCRIPTION_REQUIRED'
  detail: Record<string, unknown>

  constructor(detail: Record<string, unknown>) {
    super('Subscription required')
    this.name = 'SubscriptionRequiredError'
    this.detail = detail
  }
}

/**
 * Core fetch wrapper that:
 * 1. Attaches Bearer token
 * 2. Handles 402 → dispatches 'subscription:required' event + throws SubscriptionRequiredError
 */
async function apiFetch<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
  const normalizedEndpoint = endpoint.startsWith('/') ? endpoint : `/${endpoint}`
  const url = `${API_BASE_URL}${normalizedEndpoint}`
  const headers = {
    ...getDefaultHeaders(),
    ...options.headers,
  }

  const response = await fetch(url, { ...options, headers })

  if (response.status === 402) {
    let detail: Record<string, unknown> = {}
    try {
      detail = await response.json()
    } catch {
      detail = { error: 'trial_expired', message: 'Your trial has expired. Please subscribe.' }
    }
    window.dispatchEvent(new CustomEvent('subscription:required', { detail }))
    throw new SubscriptionRequiredError(detail)
  }

  if (!response.ok) {
    let error: any
    try { error = await response.json() } catch { error = {} }
    throw new Error(error?.error?.message || error?.message || `HTTP ${response.status}`)
  }

  return response.json()
}

// ---------------------------------------------------------------------------
// Existing AI endpoints (kept intact)
// ---------------------------------------------------------------------------

export interface TutorMatchRequest { subject: string; level: string }
export interface StudyPlanRequest { subject: string; goal: string; time: string; duration: string }
export interface SessionSummaryRequest { transcript: string }
export interface SessionNotesRequest { transcript: string; focusAreas: string }
export interface LessonPlanRequest { subject: string; level: string; topic: string; learningObjectives: string }
export interface ProgressReportRequest { studentName: string; feedbackHistory: string }
export interface QuizRequest { topic: string; difficulty: string }
export interface FlashcardRequest { topic: string }
export interface HomeworkRequest { question: string }
export interface ChurnPredictionRequest { studentUsageData: string }
export interface RevenueAnalyticsRequest { financialData: string; period: string }
export interface SupportTriageRequest { ticketContent: string }
export interface FraudDetectionRequest { transactionData: string; userIp: string }
export interface DisputeResolutionRequest { disputeDetails: string; sessionTranscript: string }
export interface AIChatRequest { message: string; userContext?: string }

export const tutorMatch = (data: TutorMatchRequest) =>
  apiPost<{ recommendation: string }>('/ai/tutor-match', data)

export const generateStudyPlan = (data: StudyPlanRequest) =>
  apiPost<{ plan: string }>('/ai/study-plan', data)

export const generateSessionSummary = (data: SessionSummaryRequest) =>
  apiPost<{ summary: string }>('/ai/session-summary', data)

export const generateSessionNotes = (data: SessionNotesRequest) =>
  apiPost<{ notes: string }>('/ai/session-notes', data)

export const generateLessonPlan = (data: LessonPlanRequest) =>
  apiPost<{ plan: string }>('/ai/lesson-plan', data)

export const generateProgressReport = (data: ProgressReportRequest) =>
  apiPost<{ report: string }>('/ai/progress-report', data)

export const generateQuiz = (data: QuizRequest) =>
  apiPost<{ quiz: string }>('/ai/quiz', data)

export const generateFlashcards = (data: FlashcardRequest) =>
  apiPost<{ flashcards: string }>('/ai/flashcards', data)

export const homeworkHelper = (data: HomeworkRequest) =>
  apiPost<{ explanation: string }>('/ai/homework-help', data)

export const churnPrediction = (data: ChurnPredictionRequest) =>
  apiPost<{ prediction: string }>('/admin/ai/churn-prediction', data)

export const revenueAnalytics = (data: RevenueAnalyticsRequest) =>
  apiPost<{ analytics: string }>('/admin/ai/revenue-analytics', data)

export const supportTriage = (data: SupportTriageRequest) =>
  apiPost<{ triage: string }>('/admin/ai/support-triage', data)

export const fraudDetection = (data: FraudDetectionRequest) =>
  apiPost<{ assessment: string }>('/admin/ai/fraud-detection', data)

export const disputeResolution = (data: DisputeResolutionRequest) =>
  apiPost<{ resolution: string }>('/admin/ai/dispute-resolution', data)

export const sendAiChatMessage = (data: AIChatRequest) =>
  apiPost<{ response: string }>('/ai/chat', data)

// ---------------------------------------------------------------------------
// Chatbot (Lexi) — uses apiFetch for 402 awareness
// ---------------------------------------------------------------------------

export interface ChatMessage {
  role: 'user' | 'assistant'
  content: string
}

export interface SendChatbotMessageResponse {
  reply: string
  role: string
  timestamp: string
}

export const sendChatbotMessage = (history: ChatMessage[], message: string) =>
  apiFetch<SendChatbotMessageResponse>('/chatbot/message', {
    method: 'POST',
    body: JSON.stringify({ history, message }),
  })

export interface SuggestCoursesRequest {
  studentGoals: string
  currentLevel: string
  subjects: string
}

export const suggestCourses = (data: SuggestCoursesRequest) =>
  apiFetch<{ suggestions: string; generatedAt: string }>('/chatbot/suggest-courses', {
    method: 'POST',
    body: JSON.stringify(data),
  })

export interface MockInterviewRequest {
  role: string
  level: string
  previousAnswer: string
}

export const startMockInterview = (data: MockInterviewRequest) =>
  apiFetch<{ response: string; role: string; level: string; timestamp: string }>('/chatbot/mock-interview', {
    method: 'POST',
    body: JSON.stringify(data),
  })

// ---------------------------------------------------------------------------
// Resume Builder — uses apiFetch for 402 awareness
// ---------------------------------------------------------------------------

export interface FresherResumeRequest {
  fullName: string
  email: string
  phone: string
  degree: string
  college: string
  graduationYear: string
  cgpa: string
  skills: string
  projects: string
  internships?: string
  certifications?: string
  careerObjective?: string
  targetRole: string
}

export interface ExperiencedResumeRequest {
  fullName: string
  email: string
  phone: string
  totalExperience: number
  currentRole: string
  currentCompany: string
  currentCtc: string
  expectedCtc: string
  noticePeriod: string
  skills: string
  workHistory: string
  keyAchievements: string
  education: string
  certifications?: string
  targetRole: string
  professionalSummary?: string
}

export interface ResumeResponse {
  resume: string
  type: string
  generatedAt: string
}

export const generateFresherResume = (data: FresherResumeRequest) =>
  apiFetch<ResumeResponse>('/resume/fresher', {
    method: 'POST',
    body: JSON.stringify(data),
  })

export const generateExperiencedResume = (data: ExperiencedResumeRequest) =>
  apiFetch<ResumeResponse>('/resume/experienced', {
    method: 'POST',
    body: JSON.stringify(data),
  })

export interface ResumeReviewRequest { resumeText: string; targetRole: string }

export const reviewResume = (data: ResumeReviewRequest) =>
  apiFetch<{ review: string; targetRole: string; reviewedAt: string }>('/resume/review', {
    method: 'POST',
    body: JSON.stringify(data),
  })

export interface TechRoadmapRequest {
  currentSkills: string
  targetRole: string
  timeframe: string
}

export const generateRoadmap = (data: TechRoadmapRequest) =>
  apiFetch<{ roadmap: string; targetRole: string; timeframe: string; generatedAt: string }>('/resume/roadmap', {
    method: 'POST',
    body: JSON.stringify(data),
  })

export const getMyResume = () =>
  apiFetch<{ resume: string; type: string; lastUpdatedAt: string }>('/resume/my-resume', {
    method: 'GET',
  })

/**
 * Downloads the student's saved resume as a proper vector PDF generated server-side
 * by QuestPDF. Returns a Blob that the caller should trigger as a browser download.
 */
export const downloadResumePdfBlob = async (): Promise<Blob> => {
  const url = `${API_BASE_URL}/resume/download-pdf`
  const token = localStorage.getItem('token') ?? ''

  const response = await fetch(url, {
    method: 'GET',
    headers: token ? { Authorization: `Bearer ${token}` } : {},
  })

  if (response.status === 402) {
    let detail: Record<string, unknown> = {}
    try { detail = await response.json() } catch { detail = {} }
    window.dispatchEvent(new CustomEvent('subscription:required', { detail }))
    throw new SubscriptionRequiredError(detail)
  }

  if (!response.ok) {
    let error: any
    try { error = await response.json() } catch { error = {} }
    throw new Error(error?.error || error?.message || `HTTP ${response.status}`)
  }

  return response.blob()
}

export const enhanceUploadedResume = async (file: File, targetRole: string): Promise<{ resume: string; type: string; generatedAt: string }> => {
  const form = new FormData()
  form.append('file', file)
  form.append('targetRole', targetRole)

  const API_BASE = (import.meta as any).env?.VITE_API_BASE_URL || 'http://localhost:5128/api'
  const token = localStorage.getItem('token') ?? ''

  const response = await fetch(`${API_BASE}/resume/enhance-upload`, {
    method: 'POST',
    body: form,
    // Only set Authorization — let browser set Content-Type with correct multipart boundary
    headers: token ? { Authorization: `Bearer ${token}` } : {},
  })

  if (response.status === 402) {
    let detail: Record<string, unknown> = {}
    try { detail = await response.json() } catch { detail = {} }
    window.dispatchEvent(new CustomEvent('subscription:required', { detail }))
    throw new SubscriptionRequiredError(detail)
  }
  if (!response.ok) {
    let error: any
    try { error = await response.json() } catch { error = {} }
    throw new Error(error?.error || error?.message || `HTTP ${response.status}`)
  }
  return response.json()
}

// ---------------------------------------------------------------------------
// Subscription — uses apiFetch
// ---------------------------------------------------------------------------

export interface SubscriptionStatus {
  isSubscribed: boolean
  subscribedUntil: string | null
  plan: string
  amount: number
  currency: string
}

export interface TrialStatus {
  trialStartDate: string | null
  trialEndDate: string | null
  trialActive: boolean
  trialExpired: boolean
  daysRemaining: number
  isSubscribed: boolean
  requiresSubscription: boolean
}

export const getSubscriptionStatus = () =>
  apiFetch<SubscriptionStatus>('/subscription/status', { method: 'GET' })

export const getTrialStatus = () =>
  apiFetch<TrialStatus>('/trial/status', { method: 'GET' })

export interface CreateOrderResponse {
  orderId: string
  amount: number
  currency: string
  keyId: string
  description: string
}

export const createSubscriptionOrder = () =>
  apiFetch<CreateOrderResponse>('/subscription/create-order', { method: 'POST' })

export interface ActivateSubscriptionRequest {
  razorpayOrderId: string
  razorpayPaymentId: string
  razorpaySignature: string
}

export interface ActivateSubscriptionResponse {
  success: boolean
  isSubscribed: boolean
  subscribedUntil: string
  message: string
}

export const activateSubscription = (data: ActivateSubscriptionRequest) =>
  apiFetch<ActivateSubscriptionResponse>('/subscription/activate', {
    method: 'POST',
    body: JSON.stringify(data),
  })

// ---------------------------------------------------------------------------
// Student Features — uses apiFetch for 402 awareness
// ---------------------------------------------------------------------------

export const getCareerPath = (data: { interest: string; currentLevel: string; careerGoal: string }) =>
  apiFetch<{ success: boolean; roadmap: string }>('/student-features/career-path', {
    method: 'POST',
    body: JSON.stringify(data),
  })

export const optimiseLinkedIn = (data: { currentAbout: string; currentHeadline: string; skills: string; targetRole: string }) =>
  apiFetch<{ success: boolean; data?: any; rawResponse?: string }>('/student-features/linkedin-optimizer', {
    method: 'POST',
    body: JSON.stringify(data),
  })

export const getProjectIdeas = (data: { techStack: string; experienceLevel: string; interestedDomain: string }) =>
  apiFetch<{ success: boolean; projects?: any; rawResponse?: string }>('/student-features/project-ideas', {
    method: 'POST',
    body: JSON.stringify(data),
  })

export const reviewCode = (data: { code: string; language: string; context?: string }) =>
  apiFetch<{ success: boolean; data?: any; rawResponse?: string }>('/student-features/code-review', {
    method: 'POST',
    body: JSON.stringify(data),
  })

export const generatePortfolio = (data: {
  name: string; role: string; bio: string; skills: string;
  projects: string; email: string; github?: string
}) =>
  apiFetch<{ success: boolean; html?: string; rawResponse?: string }>('/student-features/portfolio-generator', {
    method: 'POST',
    body: JSON.stringify(data),
  })

export const getDailyQuiz = (data: { subject: string; difficulty: string }) =>
  apiFetch<{ success: boolean; questions?: any; rawResponse?: string }>('/student-features/daily-quiz', {
    method: 'POST',
    body: JSON.stringify(data),
  })

export const generateFlashcardsNew = (data: { topic: string; count?: number }) =>
  apiFetch<{ success: boolean; flashcards?: any; rawResponse?: string }>('/student-features/flashcards', {
    method: 'POST',
    body: JSON.stringify(data),
  })

export const getAssignmentHelp = (data: { title: string; description: string; subjectType: string; deadline?: string }) =>
  apiFetch<{ success: boolean; data?: any; rawResponse?: string }>('/student-features/assignment-helper', {
    method: 'POST',
    body: JSON.stringify(data),
  })

export const generateStudySchedule = (data: {
  subject: string; examDate: string; hoursPerDay: number; currentLevel: string; topics?: string
}) =>
  apiFetch<{ success: boolean; schedule?: any; rawResponse?: string }>('/student-features/study-schedule', {
    method: 'POST',
    body: JSON.stringify(data),
  })

export const getWeeklyDigest = (data: { interest: string; careerGoal: string }) =>
  apiFetch<{ success: boolean; digest?: any; rawResponse?: string }>('/student-features/weekly-digest', {
    method: 'POST',
    body: JSON.stringify(data),
  })

export const submitWellnessCheckin = (data: {
  energyLevel: number; stressLevel: number; mood?: string; challenges?: string
}) =>
  apiFetch<{ success: boolean; data?: any; rawResponse?: string }>('/student-features/wellness-checkin', {
    method: 'POST',
    body: JSON.stringify(data),
  })
