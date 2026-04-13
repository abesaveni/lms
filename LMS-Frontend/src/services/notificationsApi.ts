import { apiGet, apiPut, apiDelete } from './api'

export interface ApiResponse<T> {
  success: boolean
  data?: T
  error?: {
    code: string
    message: string
  }
}

export interface NotificationDto {
  id: string
  title: string
  message: string
  notificationType: string
  priority: string
  actionUrl?: string
  isRead: boolean
  createdAt: string
}

export interface PaginatedResponse<T> {
  items: T[]
  pagination: {
    currentPage: number
    pageSize: number
    totalRecords: number
    totalPages: number
  }
}

export const getNotifications = async (params?: {
  page?: number
  pageSize?: number
  isRead?: boolean
}): Promise<PaginatedResponse<NotificationDto>> => {
  const queryParams = new URLSearchParams()
  if (params?.page) queryParams.append('page', params.page.toString())
  if (params?.pageSize) queryParams.append('pageSize', params.pageSize.toString())
  if (typeof params?.isRead === 'boolean') queryParams.append('isRead', params.isRead.toString())

  const query = queryParams.toString() ? `?${queryParams.toString()}` : ''
  const response = await apiGet<ApiResponse<PaginatedResponse<NotificationDto>>>(`/notifications${query}`)
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch notifications')
  }
  return response.data
}

export const getUnreadCount = async (): Promise<number> => {
  const response = await apiGet<ApiResponse<number>>('/notifications/unread-count')
  if (!response.success || response.data === undefined) {
    throw new Error(response.error?.message || 'Failed to fetch unread count')
  }
  return response.data
}

export const markNotificationRead = async (notificationId: string): Promise<void> => {
  const response = await apiPut<ApiResponse<void>>(`/notifications/${notificationId}/read`)
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to mark notification read')
  }
}

export const markAllNotificationsRead = async (): Promise<void> => {
  const response = await apiPut<ApiResponse<void>>('/notifications/read-all')
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to mark all notifications read')
  }
}

export const deleteNotification = async (notificationId: string): Promise<void> => {
  const response = await apiDelete<ApiResponse<void>>(`/notifications/${notificationId}`)
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to delete notification')
  }
}
