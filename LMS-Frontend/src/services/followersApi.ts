import { apiDelete, apiGet, apiPost } from './api'

export interface ApiResponse<T> {
  success: boolean
  data?: T
  error?: {
    code: string
    message: string
  }
}

export const followTutor = async (tutorId: string): Promise<void> => {
  const response = await apiPost<ApiResponse<void>>(`/tutors/${tutorId}/follow`)
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to follow tutor')
  }
}

export const unfollowTutor = async (tutorId: string): Promise<void> => {
  const response = await apiDelete<ApiResponse<void>>(`/tutors/${tutorId}/follow`)
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to unfollow tutor')
  }
}

export const getFollowStatus = async (tutorId: string): Promise<boolean> => {
  const response = await apiGet<ApiResponse<boolean>>(`/tutors/${tutorId}/follow-status`)
  if (!response.success || response.data === undefined) {
    throw new Error(response.error?.message || 'Failed to fetch follow status')
  }
  return response.data
}

export const getFollowerCount = async (tutorId: string): Promise<number> => {
  const response = await apiGet<ApiResponse<number>>(`/tutors/${tutorId}/followers/count`)
  if (!response.success || response.data === undefined) {
    throw new Error(response.error?.message || 'Failed to fetch follower count')
  }
  return response.data
}
