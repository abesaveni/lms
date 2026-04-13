import { apiGetPublic, apiPost, ApiResponse } from './api'

export interface ReviewDto {
  id: string
  studentId: string
  studentName: string
  studentImage?: string
  rating: number
  comment: string
  response?: string
  createdAt: string
}

export interface ReviewListResponse {
  items: ReviewDto[]
  pagination: {
    currentPage: number
    pageSize: number
    totalRecords: number
    totalPages: number
  }
}

/**
 * Get reviews for a tutor
 */
export const getTutorReviews = async (tutorId: string, page = 1, pageSize = 10): Promise<ReviewListResponse> => {
  const response = await apiGetPublic<ApiResponse<ReviewListResponse>>(`/reviews?tutorId=${tutorId}&page=${page}&pageSize=${pageSize}`)
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch reviews')
  }
  return response.data
}

/**
 * Submit a review
 */
export const submitReview = async (data: { sessionId: string; rating: number; comment: string }): Promise<void> => {
  const response = await apiPost<ApiResponse<void>>('/reviews', data)
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to submit review')
  }
}
