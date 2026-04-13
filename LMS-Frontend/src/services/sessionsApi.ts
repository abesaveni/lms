import { apiGet, apiPost, apiGetPublic, ApiResponse } from './api'
import { getCurrentUser } from '../utils/auth'

export interface SessionDto {
  id: string
  tutorId: string
  tutorName: string
  tutorImage?: string
  sessionType: string
  pricingType: string
  title: string
  description: string
  subjectId: string
  subjectName?: string
  subject?: string
  scheduledAt: string
  duration: number
  basePrice: number
  maxStudents: number
  currentStudents: number
  status: string
  meetingLink?: string
  isBooked?: boolean
  isReviewed?: boolean
  createdAt: string
}

export interface TeacherSubjectDto {
  id: string
  name: string
}

export interface CreateSessionDto {
  title: string
  description: string
  sessionType: string
  subjectId: string
  scheduledAt: string
  duration: number
  basePrice: number
  maxStudents: number
  pricingType: string
}

/**
 * Get all sessions with filters
 */
export const getSessions = async (params?: {
  page?: number
  pageSize?: number
  tutorId?: string
  status?: string
  upcoming?: boolean
}): Promise<{ items: SessionDto[]; pagination: any }> => {
  const queryParams = new URLSearchParams()
  if (params?.page) queryParams.append('page', params.page.toString())
  if (params?.pageSize) queryParams.append('pageSize', params.pageSize.toString())
  if (params?.tutorId) queryParams.append('tutorId', params.tutorId)
  if (params?.status) queryParams.append('status', params.status)
  if (params?.upcoming !== undefined) queryParams.append('upcoming', params.upcoming.toString())

  const query = queryParams.toString() ? `?${queryParams.toString()}` : ''

  // Use public endpoint without auth headers
  const response = await apiGetPublic<ApiResponse<{ items: SessionDto[]; pagination: any }>>(`/sessions${query}`)

  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch sessions')
  }
  return response.data
}

/**
 * Get session by ID
 */
export const getSessionById = async (id: string): Promise<SessionDto> => {
  const response = await apiGetPublic<ApiResponse<SessionDto>>(`/sessions/${id}`)
  
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch session')
  }
  return response.data
}

/**
 * Get meeting link for a session
 */
export const getSessionMeetingLink = async (id: string): Promise<{ meetingLink: string, scheduledAt: string, duration: number }> => {
  const response = await apiGet<ApiResponse<{ meetingLink: string, scheduledAt: string, duration: number }>>(`/sessions/${id}/meeting-link`)
  
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to get meeting link')
  }
  return response.data
}

/**
 * Create a new session (Tutor only)
 */
export const createSession = async (data: CreateSessionDto): Promise<SessionDto> => {
  const response = await apiPost<ApiResponse<SessionDto>>('/sessions', data)
  
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to create session')
  }
  return response.data
}

/**
 * Get tutor's sessions (Tutor only)
 */
export const getTutorSessions = async (params?: {
  page?: number
  pageSize?: number
  status?: string
  upcoming?: boolean
  past?: boolean
}): Promise<{ items: SessionDto[]; pagination: any }> => {
  const queryParams = new URLSearchParams()
  if (params?.page) queryParams.append('page', params.page.toString())
  if (params?.pageSize) queryParams.append('pageSize', params.pageSize.toString())
  if (params?.status) queryParams.append('status', params.status)
  if (params?.upcoming !== undefined) queryParams.append('upcoming', params.upcoming.toString())
  if (params?.past !== undefined) queryParams.append('past', params.past.toString())

  const query = queryParams.toString() ? `?${queryParams.toString()}` : ''
  const response = await apiGet<ApiResponse<any[]>>(`/tutor/sessions/my-sessions${query}`)

  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch sessions')
  }

  // Handle the backend Result<List<MySessionDto>> format
  const items = response.data.map((item: any) => ({
    id: item.sessionId || item.Id || '',
    tutorId: '', 
    tutorName: '',
    title: item.title || '',
    scheduledAt: item.scheduledAt || '',
    status: item.status || '',
    duration: 60,
    basePrice: item.price || 0,
    maxStudents: item.maxParticipants || 1,
    meetingLink: item.meetingLink || '',
    createdAt: item.scheduledAt || '',
  })) as SessionDto[]

  return {
    items,
    pagination: { currentPage: 1, pageSize: 50, totalRecords: items.length }
  }
}

/**
 * Get student's booked sessions (Student only)
 */
export const getStudentSessions = async (params?: {
  page?: number
  pageSize?: number
  status?: string
  upcoming?: boolean
  past?: boolean
}): Promise<{ items: SessionDto[]; pagination: any }> => {
  const user = getCurrentUser()
  const queryParams = new URLSearchParams()
  if (params?.page) queryParams.append('page', params.page.toString())
  if (params?.pageSize) queryParams.append('pageSize', params.pageSize.toString())
  if (params?.status) queryParams.append('status', params.status)
  if (params?.upcoming !== undefined) queryParams.append('upcoming', params.upcoming.toString())
  if (params?.past !== undefined) queryParams.append('past', params.past.toString())
  if (user?.id) queryParams.append('studentId', user.id)

  const query = `?${queryParams.toString()}`
  const response = await apiGet<ApiResponse<{ items: SessionDto[]; pagination: any }>>(`/sessions${query}`)

  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch student sessions')
  }
  return response.data
}

/**
 * Session Detail DTO
 */
export interface SessionDetailDto extends SessionDto {}

/**
 * Session Pricing DTO
 */
export interface SessionPricingDto {
  baseAmount: number;
  platformFee: number;
  totalAmount: number;
}

/**
 * Get all available subjects
 */
export const getSubjects = async (): Promise<TeacherSubjectDto[]> => {
  const response = await apiGetPublic<ApiResponse<TeacherSubjectDto[]>>('/tutors/subjects')
  
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch subjects')
  }
  return response.data
}

/**
 * Get pricing breakdown for a session
 */
export const getSessionPricing = async (sessionId: string, hours?: number): Promise<SessionPricingDto> => {
  const response = await apiPost<ApiResponse<SessionPricingDto>>(`/sessions/${sessionId}/pricing`, { hours })
  
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to get session pricing')
  }
  return response.data
}

/**
 * Book a session
 */
export const bookSession = async (data: { sessionId: string; hours?: number }): Promise<any> => {
  const response = await apiPost<ApiResponse<any>>(`/sessions/${data.sessionId}/book`, data)
  
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to book session')
  }
  return response.data
}
/**
 * Start a session (Tutor only)
 */
export const startSession = async (sessionId: string): Promise<{ meetUrl: string, expiresAt: string, sessionId: string }> => {
  const response = await apiPost<ApiResponse<{ meetUrl: string, expiresAt: string, sessionId: string }>>(`/sessions/${sessionId}/start`, {})
  
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to start session')
  }
  return response.data
}

/**
 * Join a session Response interface
 */
export interface JoinSessionResponse {
  meetUrl: string
  expiresAt: string
  sessionId: string
}

/**
 * Join a session (Student only)
 */
export const joinSession = async (sessionId: string): Promise<JoinSessionResponse> => {
  const response = await apiPost<ApiResponse<JoinSessionResponse>>(`/sessions/${sessionId}/join`, {})
  
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to join session')
  }
  return response.data
}

/**
 * Cancel a session booking (Student only)
 */
export const cancelBooking = async (sessionId: string, reason?: string): Promise<void> => {
  const response = await apiPost<ApiResponse<void>>(`/sessions/${sessionId}/cancel-booking`, { reason })
  
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to cancel booking')
  }
}

/**
 * Mark a session as complete (Tutor only)
 */
export const markSessionComplete = async (sessionId: string): Promise<void> => {
  const response = await apiPost<ApiResponse<void>>(`/sessions/${sessionId}/complete`, {})
  
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to mark session as complete')
  }
}
