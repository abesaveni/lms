import { apiGetPublic } from './api'

export interface ApiResponse<T> {
  success: boolean
  data?: T
  error?: {
    code: string
    message: string
  }
}

export interface TutorDto {
  id: string
  userId: string
  name: string
  email: string
  bio?: string
  headline?: string
  hourlyRate: number
  hourlyRateGroup?: number
  yearsOfExperience?: number
  averageRating?: number
  totalReviews?: number
  totalSessions?: number
  followerCount?: number
  verificationStatus: string
  profileImage?: string
  subjects?: string[]
  location?: string
  available?: boolean
  hasBackgroundCheck?: boolean
  trialAvailable?: boolean
  trialDurationMinutes?: number
  trialPrice?: number
}

export interface TutorListResponse {
  items: TutorDto[]
  pagination?: {
    currentPage: number
    pageSize: number
    totalRecords: number
    totalPages?: number
  }
}

/**
 * Get all tutors (public endpoint)
 */
export const getTutors = async (params?: {
  page?: number
  pageSize?: number
  search?: string
  subject?: string
  minRating?: number
  maxPrice?: number
  minPrice?: number
}): Promise<TutorListResponse> => {
  const queryParams = new URLSearchParams()
  if (params?.page) queryParams.append('page', params.page.toString())
  if (params?.pageSize) queryParams.append('pageSize', params.pageSize.toString())
  if (params?.search) queryParams.append('search', params.search)
  if (params?.subject) queryParams.append('subject', params.subject)
  if (params?.minRating) queryParams.append('minRating', params.minRating.toString())
  if (params?.maxPrice) queryParams.append('maxPrice', params.maxPrice.toString())
  if (params?.minPrice) queryParams.append('minPrice', params.minPrice.toString())
  
  const query = queryParams.toString() ? `?${queryParams.toString()}` : ''
  
  // Use /tutors endpoint (AllowAnonymous)
  const response = await apiGetPublic<ApiResponse<any>>(`/tutors${query}`)
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch tutors')
  }

  const data = response.data as any
  if (data.Items && Array.isArray(data.Items)) {
    return {
      items: data.Items.map((item: any) => ({
        id: (item.Id || item.id)?.toString() || '',
        userId: (item.UserId || item.userId)?.toString() || '',
        name: item.Name || item.name || '',
        email: item.Email || item.email || '',
        bio: item.Bio || item.bio || '',
        headline: item.Headline || item.headline || '',
        hourlyRate: item.HourlyRate || item.hourlyRate || 0,
        yearsOfExperience: item.YearsOfExperience || item.yearsOfExperience,
        averageRating: item.AverageRating || item.averageRating || 0,
        totalReviews: item.TotalReviews || item.totalReviews || 0,
        totalSessions: item.TotalSessions || item.totalSessions || 0,
        followerCount: item.FollowerCount || item.followerCount || 0,
        verificationStatus: item.VerificationStatus || item.verificationStatus || '',
        profileImage: item.ProfileImage || item.profileImage,
        subjects: item.Subjects || item.subjects || [],
        available: true,
      })) as TutorDto[],
      pagination: data.Pagination || {
        currentPage: params?.page || 1,
        pageSize: params?.pageSize || 10,
        totalRecords: data.Items.length
      }
    }
  }

  return response.data
}

/**
 * Get tutor by ID
 */
export const getTutorById = async (tutorId: string): Promise<TutorDto> => {
  const response = await apiGetPublic<ApiResponse<TutorDto>>(`/tutors/${tutorId}/profile`)
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch tutor')
  }
  const data: any = response.data
  return {
    id: (data.Id || data.id)?.toString() || '',
    userId: (data.UserId || data.userId)?.toString() || '',
    name: data.Name || data.name || '',
    email: data.Email || data.email || '',
    bio: data.Bio || data.bio || '',
    headline: data.Headline || data.headline || '',
    hourlyRate: data.HourlyRate || data.hourlyRate || 0,
    yearsOfExperience: data.YearsOfExperience || data.yearsOfExperience,
    averageRating: data.AverageRating || data.averageRating || 0,
    totalReviews: data.TotalReviews || data.totalReviews || 0,
    totalSessions: data.TotalSessions || data.totalSessions || 0,
    followerCount: data.FollowerCount || data.followerCount || 0,
    verificationStatus: data.VerificationStatus || data.verificationStatus || '',
    profileImage: data.ProfileImage || data.profileImage,
    subjects: data.Subjects || data.subjects || [],
    location: data.Location || data.location,
    available: data.Available ?? data.available ?? true,
  }
}

/**
 * Search tutors
 */
export const searchTutors = async (query: string): Promise<TutorDto[]> => {
  const response = await apiGetPublic<ApiResponse<TutorDto[]>>(`/shared/tutors/search?query=${encodeURIComponent(query)}`)
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to search tutors')
  }
  return response.data
}

/**
 * Public platform stats for landing page (no auth required)
 */
export interface PlatformStats {
  studentCount: number
  tutorCount: number
}

export const getPlatformStats = async (): Promise<PlatformStats> => {
  const response = await apiGetPublic<PlatformStats>('/platform/stats')
  return response
}
