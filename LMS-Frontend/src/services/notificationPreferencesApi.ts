import { apiGet, apiPut } from './api'

export interface ApiResponse<T> {
  success: boolean
  data?: T
  error?: {
    code: string
    message: string
  }
}

export interface NotificationPreferenceDto {
  category: string
  emailEnabled: boolean
  whatsAppEnabled: boolean
  inAppEnabled: boolean
}

export const getNotificationPreferences = async (): Promise<NotificationPreferenceDto[]> => {
  const response = await apiGet<ApiResponse<NotificationPreferenceDto[]>>('/notification-preferences')
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to load preferences')
  }
  return response.data
}

export const updateNotificationPreferences = async (preferences: NotificationPreferenceDto[]): Promise<void> => {
  const response = await apiPut<ApiResponse<void>>('/notification-preferences', preferences)
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to update preferences')
  }
}
