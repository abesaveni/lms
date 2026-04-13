import { apiGet, apiPost } from './api'

export interface ApiResponse<T> {
  success: boolean
  data?: T
  error?: {
    code: string
    message: string
  }
}

export interface ChatRequestDto {
  id: string
  studentId: string
  studentName: string
  studentAvatar?: string
  tutorId: string
  tutorName: string
  tutorAvatar?: string
  status: string
  conversationId?: string
  createdAt: string
  updatedAt: string
}

export const createChatRequest = async (tutorId: string): Promise<ChatRequestDto> => {
  const response = await apiPost<ApiResponse<ChatRequestDto>>('/chat-requests', { tutorId })
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to create chat request')
  }
  return response.data
}

export const getStudentChatRequests = async (): Promise<ChatRequestDto[]> => {
  const response = await apiGet<ApiResponse<ChatRequestDto[]>>('/chat-requests/student')
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch chat requests')
  }
  return response.data
}

export const getTutorChatRequests = async (): Promise<ChatRequestDto[]> => {
  const response = await apiGet<ApiResponse<ChatRequestDto[]>>('/chat-requests/tutor')
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch chat requests')
  }
  return response.data
}

export const respondToChatRequest = async (
  requestId: string,
  status: 'Accepted' | 'Rejected' | 'Hold'
): Promise<ChatRequestDto> => {
  const response = await apiPost<ApiResponse<ChatRequestDto>>(`/chat-requests/${requestId}/respond`, { status })
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to update chat request')
  }
  return response.data
}
