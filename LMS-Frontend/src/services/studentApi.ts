import { apiGet, apiPut, apiPost, ApiResponse } from './api'

export interface UpcomingSessionDto {
  sessionId: string
  title: string
  tutorName: string
  scheduledAt: string
  meetingLink: string
  duration?: number
  subject?: string
  bookingStatus?: string
}

export interface RecentActivityDto {
  type: string
  description: string
  timestamp: string
}

export interface StudentDashboardDto {
  totalBookings: number
  completedSessions: number
  upcomingSessionsCount: number
  totalBonusPoints: number
  upcomingSessions: UpcomingSessionDto[]
  recentActivity: RecentActivityDto[]
}

export interface MonthlyActivityDto {
  month: string
  sessions: number
  amountSpent: number
}

export interface StudentStatsDto {
  totalSessionsAttended: number
  totalHoursLearned: number
  totalAmountSpent: number
  sessionsBySubject: Record<string, number>
  monthlyActivity: MonthlyActivityDto[]
}

export const getStudentDashboard = async (): Promise<StudentDashboardDto> => {
  const response = await apiGet<ApiResponse<StudentDashboardDto>>('/student/dashboard')
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch student dashboard')
  }
  return response.data
}

export const getStudentStats = async (): Promise<StudentStatsDto> => {
  const response = await apiGet<ApiResponse<StudentStatsDto>>('/student/dashboard/stats')
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch student stats')
  }
  return response.data
}

export interface StudentProfileDto {
  userId: string
  username: string
  firstName: string
  lastName: string
  email: string
  phoneNumber: string
  bio?: string
  dateOfBirth?: string
  location?: string
  profilePictureUrl?: string
  preferredSubjects?: string
  createdAt?: string
  updatedAt?: string
}


export interface UpdateStudentProfileRequest {
  firstName?: string
  lastName?: string
  phoneNumber?: string
  bio?: string
  dateOfBirth?: string
  location?: string
  profilePictureUrl?: string
  profilePictureBase64?: string
  profilePictureFileName?: string
}

/**
 * Get current student profile
 */
export const getStudentProfile = async (): Promise<StudentProfileDto> => {
  const response = await apiGet<ApiResponse<StudentProfileDto>>('/student/profile')
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch student profile')
  }
  return response.data
}

/**
 * Update student profile
 */
export const updateStudentProfile = async (data: UpdateStudentProfileRequest): Promise<void> => {
  const response = await apiPut<ApiResponse<void>>('/student/profile', data)
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to update student profile')
  }
}

/**
 * Change student password
 */
export const changePassword = async (data: any): Promise<void> => {
  const response = await apiPost<ApiResponse<void>>('/student/change-password', data)
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to change password')
  }
}

