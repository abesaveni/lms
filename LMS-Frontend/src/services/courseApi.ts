import { apiGet, apiPost, apiPut, apiPatch, apiDelete } from './api'

// ─────────────────────────────────────────────────────────────────────────────
// Types
// ─────────────────────────────────────────────────────────────────────────────

export interface SyllabusItem {
  sessionNumber: number
  title: string
  topics?: string
  description?: string
}

export interface CreateCoursePayload {
  title: string
  shortDescription?: string
  fullDescription?: string
  subjectId?: string
  subjectName?: string
  categoryName?: string
  level?: string
  language?: string
  thumbnailUrl?: string
  tags?: string[]
  totalSessions: number
  sessionDurationMinutes: number
  deliveryType?: string
  maxStudentsPerBatch?: number
  pricePerSession: number
  bundlePrice?: number | null
  allowPartialBooking?: boolean
  minSessionsForPartial?: number
  refundPolicy?: string
  trialAvailable?: boolean
  trialDurationMinutes?: number
  trialPrice?: number
  prerequisites?: string
  materialsRequired?: string
  whatYouWillLearn?: string
  syllabus?: SyllabusItem[]
}

export interface CourseListItem {
  id: string
  title: string
  shortDescription?: string
  subjectName?: string
  categoryName?: string
  level: string
  language: string
  thumbnailUrl?: string
  totalSessions: number
  sessionDurationMinutes: number
  deliveryType: string
  pricePerSession: number
  bundlePrice?: number
  trialAvailable: boolean
  trialPrice: number
  trialDurationMinutes: number
  averageRating: number
  totalReviews: number
  totalEnrollments: number
  tutorName?: string
  tutorId?: string
  status?: string
  publishedAt?: string
  createdAt?: string
}

export interface CourseDetail extends CourseListItem {
  fullDescription?: string
  maxStudentsPerBatch: number
  allowPartialBooking: boolean
  minSessionsForPartial: number
  refundPolicy?: string
  prerequisites?: string
  materialsRequired?: string
  whatYouWillLearn?: string
  syllabusJson?: string
  tagsJson?: string
  tutor: {
    tutorId: string
    name: string
    bio?: string
    headline?: string
    averageRating?: number
    totalSessions?: number
    yearsOfExperience?: number
  }
  sessions: Array<{
    sessionNumber: number
    title: string
    description?: string
    topicsCovered?: string
    scheduledAt?: string
    durationMinutes: number
    status: string
  }>
}

export interface CourseSession {
  id: string
  sessionNumber: number
  title: string
  description?: string
  topicsCovered?: string
  scheduledAt?: string
  durationMinutes: number
  status: string
  tutorNotes?: string
  homeworkAssigned?: string
}

export interface SubjectRate {
  subjectName: string
  hourlyRate: number
  trialRate?: number
  subjectId?: string
}

// ─────────────────────────────────────────────────────────────────────────────
// Public / Browse
// ─────────────────────────────────────────────────────────────────────────────

export const browseCourses = (params?: {
  subject?: string; level?: string; q?: string; maxPrice?: number
  page?: number; pageSize?: number
}) => {
  const qs = new URLSearchParams()
  if (params?.subject) qs.set('subject', params.subject)
  if (params?.level) qs.set('level', params.level)
  if (params?.q) qs.set('q', params.q)
  if (params?.maxPrice) qs.set('maxPrice', String(params.maxPrice))
  if (params?.page) qs.set('page', String(params.page))
  if (params?.pageSize) qs.set('pageSize', String(params.pageSize))
  return apiGet<{ data: CourseListItem[]; total: number; page: number; pageSize: number }>(
    `/courses?${qs.toString()}`
  )
}

export const getCourseById = (id: string) =>
  apiGet<CourseDetail>(`/courses/${id}`)

export const getCoursesByTutor = (tutorId: string) =>
  apiGet<{ courses: CourseListItem[]; subjectRates: SubjectRate[] }>(`/courses/by-tutor/${tutorId}`)

// ─────────────────────────────────────────────────────────────────────────────
// Tutor: Course Management
// ─────────────────────────────────────────────────────────────────────────────

export const getMyCourses = () =>
  apiGet<{ data: CourseListItem[] }>('/courses/my')

export const createCourse = (payload: CreateCoursePayload) =>
  apiPost<{ courseId: string; message: string }>('/courses', payload)

export const updateCourse = (id: string, payload: CreateCoursePayload) =>
  apiPut<{ message: string }>(`/courses/${id}`, payload)

export const updateCourseStatus = (id: string, status: string) =>
  apiPatch<{ message: string }>(`/courses/${id}/status`, { status })

export const deleteCourse = (id: string) =>
  apiDelete<{ message: string }>(`/courses/${id}`)

export const upsertCourseSession = (courseId: string, payload: {
  sessionNumber: number; title: string; description?: string; topicsCovered?: string
  scheduledAt?: string; durationMinutes?: number; homeworkAssigned?: string
}) => apiPost<{ message: string }>(`/courses/${courseId}/sessions`, payload)

export const getCourseSessions = (courseId: string) =>
  apiGet<{ data: CourseSession[] }>(`/courses/${courseId}/sessions`)

// ─────────────────────────────────────────────────────────────────────────────
// Subject Rates
// ─────────────────────────────────────────────────────────────────────────────

export const getMySubjectRates = () =>
  apiGet<{ data: SubjectRate[] }>('/courses/subject-rates')

export const updateSubjectRates = (rates: SubjectRate[]) =>
  apiPut<{ message: string }>('/courses/subject-rates', { rates })
