import { apiGet, apiPost } from './api'

export interface ApiResponse<T> {
  success: boolean
  data?: T
  error?: {
    code: string
    message: string
  }
}

/**
 * Get Google Calendar OAuth authorization URL
 */
export const getCalendarAuthUrl = async (): Promise<string> => {
  const redirectUri = `${window.location.origin}/calendar/connect`
  const response = await apiGet<ApiResponse<string>>(`/calendar/oauth/authorize?redirectUri=${encodeURIComponent(redirectUri)}`)
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to get authorization URL')
  }
  return response.data
}

/**
 * Check if Google Calendar is connected
 */
export const checkCalendarConnection = async (): Promise<boolean> => {
  // Google Calendar is no longer required — Jitsi is used for video conferencing.
  return true
}

/**
 * Mock Google Calendar connection (FOR TESTING ONLY)
 */
export const mockConnectCalendar = async (): Promise<boolean> => {
  const response = await apiPost<ApiResponse<boolean>>('/calendar/oauth/mock-connect', {})
  return response.success && response.data === true
}
