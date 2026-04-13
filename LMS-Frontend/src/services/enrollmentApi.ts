import { apiGet, apiPost } from './api'

// ─────────────────────────────────────────────────────────────────────────────
// Types
// ─────────────────────────────────────────────────────────────────────────────

export interface CreateEnrollmentOrderPayload {
  courseId: string
  enrollmentType?: 'Full' | 'Partial'
  sessionsToPurchase?: number
}

export interface CreateEnrollmentOrderResponse {
  orderId: string
  keyId: string
  amount: number
  currency: string
  courseTitle: string
  sessionsPurchased: number
  enrollmentType: string
  description: string
}

export interface VerifyEnrollmentPaymentPayload {
  courseId: string
  razorpayOrderId: string
  razorpayPaymentId: string
  razorpaySignature: string
  enrollmentType: string
  sessionsPurchased: number
  amountPaid: number
}

export interface VerifyEnrollmentPaymentResponse {
  enrollmentId: string
  message: string
  sessionsPurchased: number
  expiresAt: string
}

export interface MyEnrollment {
  id: string
  courseId: string
  courseTitle: string
  courseThumbnail?: string
  subjectName?: string
  tutorName: string
  enrollmentType: string
  sessionsPurchased: number
  sessionsCompleted: number
  sessionsRemaining: number
  amountPaid: number
  status: string
  enrolledAt?: string
  expiresAt?: string
  completedAt?: string
  progressPercent: number
}

export interface EnrollmentCheck {
  isEnrolled: boolean
  enrollment?: {
    id: string
    sessionsPurchased: number
    sessionsCompleted: number
    sessionsRemaining: number
    expiresAt?: string
  }
}

export interface BookTrialPayload {
  tutorId: string
  courseId?: string
  scheduledAt?: string
}

export interface BookTrialResponse {
  trialId: string
  price: number
  message: string
}

export interface MyTrial {
  id: string
  tutorId: string
  tutorName: string
  courseTitle?: string
  scheduledAt?: string
  durationMinutes: number
  price: number
  status: string
  convertedToEnrollment: boolean
}

export interface IncomingTrial {
  id: string
  studentId: string
  studentName: string
  studentEmail: string
  courseTitle?: string
  scheduledAt?: string
  durationMinutes: number
  price: number
  status: string
  convertedToEnrollment: boolean
}

// ─────────────────────────────────────────────────────────────────────────────
// Enrollment
// ─────────────────────────────────────────────────────────────────────────────

export const createEnrollmentOrder = (payload: CreateEnrollmentOrderPayload) =>
  apiPost<CreateEnrollmentOrderResponse>('/enrollments/create-order', payload)

export const verifyEnrollmentPayment = (payload: VerifyEnrollmentPaymentPayload) =>
  apiPost<VerifyEnrollmentPaymentResponse>('/enrollments/verify-payment', payload)

export const getMyEnrollments = () =>
  apiGet<{ data: MyEnrollment[] }>('/enrollments/my')

export const checkEnrollment = (courseId: string) =>
  apiGet<EnrollmentCheck>(`/enrollments/check/${courseId}`)

// ─────────────────────────────────────────────────────────────────────────────
// Trials
// ─────────────────────────────────────────────────────────────────────────────

export const bookTrial = (payload: BookTrialPayload) =>
  apiPost<BookTrialResponse>('/enrollments/trial', payload)

export const getMyTrials = () =>
  apiGet<{ data: MyTrial[] }>('/enrollments/trials/my')

export const getIncomingTrials = () =>
  apiGet<{ data: IncomingTrial[] }>('/enrollments/trials/incoming')

// ─────────────────────────────────────────────────────────────────────────────
// Tutor: course enrollments
// ─────────────────────────────────────────────────────────────────────────────

export interface CourseEnrollmentItem {
  id: string
  studentId: string
  studentName: string
  studentEmail: string
  enrollmentType: string
  sessionsPurchased: number
  sessionsCompleted: number
  amountPaid: number
  tutorEarningAmount: number
  status: string
  enrolledAt?: string
  expiresAt?: string
}

export const getCourseEnrollments = (courseId: string) =>
  apiGet<{ data: CourseEnrollmentItem[]; total: number }>(`/enrollments/course/${courseId}`)
