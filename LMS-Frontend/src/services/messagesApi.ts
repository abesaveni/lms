import { apiGet, apiPost, apiPut } from './api'

export interface ApiResponse<T> {
  success: boolean
  data?: T
  error?: {
    code: string
    message: string
  }
}

export interface ConversationDto {
  id: string
  user1Id: string
  user2Id: string
  user1Name?: string
  user2Name?: string
  user1Avatar?: string
  user2Avatar?: string
  lastMessage?: string
  lastMessageTime?: string
  otherUserId?: string
  otherUserName?: string
  otherUserImage?: string
  lastMessageContent?: string
  lastMessageAt?: string
  unreadCount: number
  createdAt: string
  updatedAt: string
}

export interface MessageDto {
  id: string
  conversationId: string
  senderId: string
  senderName?: string
  senderAvatar?: string
  content: string
  messageType: string
  isRead: boolean
  readAt?: string
  createdAt: string
  updatedAt: string
}

export interface CreateConversationRequest {
  userId: string // The other user's ID to start conversation with
}

export interface SendMessageRequest {
  content: string
  messageType?: string
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

/**
 * Get all conversations for current user
 */
export const getConversations = async (params?: {
  page?: number
  pageSize?: number
}): Promise<PaginatedResponse<ConversationDto>> => {
  const queryParams = new URLSearchParams()
  if (params?.page) queryParams.append('page', params.page.toString())
  if (params?.pageSize) queryParams.append('pageSize', params.pageSize.toString())
  
  const query = queryParams.toString() ? `?${queryParams.toString()}` : ''
  const response = await apiGet<ApiResponse<PaginatedResponse<ConversationDto>>>(`/messages/conversations${query}`)
  
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch conversations')
  }
  return response.data
}

/**
 * Create or get existing conversation with another user
 */
export const createConversation = async (data: CreateConversationRequest): Promise<ConversationDto> => {
  const response = await apiPost<ApiResponse<ConversationDto>>('/messages/conversations', data)
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to create conversation')
  }
  return response.data
}

/**
 * Get messages in a conversation
 */
export const getMessages = async (
  conversationId: string,
  params?: {
    page?: number
    pageSize?: number
  }
): Promise<PaginatedResponse<MessageDto>> => {
  const queryParams = new URLSearchParams()
  if (params?.page) queryParams.append('page', params.page.toString())
  if (params?.pageSize) queryParams.append('pageSize', params.pageSize.toString())
  
  const query = queryParams.toString() ? `?${queryParams.toString()}` : ''
  const response = await apiGet<ApiResponse<PaginatedResponse<MessageDto>>>(`/messages/conversations/${conversationId}/messages${query}`)
  
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch messages')
  }
  return response.data
}

/**
 * Send a message in a conversation (REST API - SignalR is used for real-time)
 */
export const sendMessage = async (
  conversationId: string,
  data: SendMessageRequest
): Promise<MessageDto> => {
  const response = await apiPost<ApiResponse<MessageDto>>(`/messages/conversations/${conversationId}/messages`, data)
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to send message')
  }
  return response.data
}

/**
 * Mark a message as read
 */
export const markMessageAsRead = async (messageId: string): Promise<void> => {
  const response = await apiPut<ApiResponse<void>>(`/messages/messages/${messageId}/read`)
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to mark message as read')
  }
}
