import { apiGet, apiPut, apiPatch, apiDelete, apiPostFormData } from './api'

export interface ApiResponse<T> {
  success: boolean
  data?: T
  error?: {
    code: string
    message: string
  }
}

export interface UserDto {
  id: string
  username: string
  email: string
  firstName: string
  lastName: string
  role: string
  profileImageUrl?: string
  isActive: boolean
  isEmailVerified: boolean
  createdAt: string
  updatedAt: string
}

export interface UserProfileDto extends UserDto {
  phoneNumber?: string
  whatsAppNumber?: string
  bio?: string
  dateOfBirth?: string
  location?: string
  profilePictureBase64?: string
  profilePictureFileName?: string
  language?: string
  timezone?: string
  tutorProfile?: {
    hourlyRate: number
    verificationStatus: string
    totalSessions: number
    rating: number
  }
  studentProfile?: {
    referralCode: string
    preferredSubjects?: string
  }
}

export interface UpdateProfileRequest {
  firstName?: string
  lastName?: string
  phoneNumber?: string
  whatsAppNumber?: string
  bio?: string
  profileImageUrl?: string
  profilePictureBase64?: string
  profilePictureFileName?: string
  dateOfBirth?: string
  location?: string
  language?: string
  timezone?: string
}

/**
 * Get current user profile
 */
export const getCurrentUserProfile = async (): Promise<UserProfileDto> => {
  const response = await apiGet<ApiResponse<UserProfileDto>>('/users/profile')
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch user profile')
  }
  return response.data
}

/**
 * Update current user profile
 */
export const updateProfile = async (data: UpdateProfileRequest): Promise<void> => {
  const response = await apiPut<ApiResponse<void>>('/users/profile', data)
  // Backend returns Result.SuccessResult with no data field, so we just check success
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to update profile')
  }
}

/**
 * Get user by ID (Admin only)
 */
export const getUserById = async (userId: string): Promise<UserDto> => {
  const response = await apiGet<ApiResponse<UserDto>>(`/admin/users/${userId}`)
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch user')
  }
  return response.data
}

/**
 * Get all users (Admin only)
 */
export const getUsers = async (params?: {
  page?: number
  pageSize?: number
  role?: string
  search?: string
}): Promise<{ items: UserDto[]; pagination: any }> => {
  const queryParams = new URLSearchParams()
  if (params?.page) queryParams.append('page', params.page.toString())
  if (params?.pageSize) queryParams.append('pageSize', params.pageSize.toString())
  if (params?.role) queryParams.append('role', params.role)
  if (params?.search) queryParams.append('search', params.search)
  
  const query = queryParams.toString() ? `?${queryParams.toString()}` : ''
  const response = await apiGet<ApiResponse<{ items: UserDto[]; pagination: any }>>(`/admin/users${query}`)
  
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch users')
  }
  return response.data
}

/**
 * Activate user (Admin only)
 */
export const activateUser = async (userId: string): Promise<void> => {
  const response = await apiPatch<ApiResponse<void>>(`/admin/users/${userId}/activate`)
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to activate user')
  }
}

/**
 * Deactivate user (Admin only)
 */
export const deactivateUser = async (userId: string): Promise<void> => {
  const response = await apiPatch<ApiResponse<void>>(`/admin/users/${userId}/deactivate`)
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to deactivate user')
  }
}

/**
 * Upload profile image
 */
export const uploadProfileImage = async (file: File): Promise<string> => {
  const formData = new FormData()
  formData.append('File', file)
  const response = await apiPostFormData<ApiResponse<string>>('/users/profile/image', formData)
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to upload profile image')
  }
  return response.data
}

/**
 * Delete user (Admin only)
 */
export const deleteUser = async (userId: string): Promise<void> => {
  const response = await apiDelete<ApiResponse<void>>(`/admin/users/${userId}`)
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to delete user')
  }
}
