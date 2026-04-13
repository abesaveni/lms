import { apiGet, apiPost, apiPut, apiDelete, apiPatch, apiPostFormData } from './api'

export interface AdminPayoutRequest {
  id: string
  tutorId: string
  tutorName: string
  tutorEmail: string
  amount: number
  requestedAt: string
  bankAccount: {
    accountHolderName: string
    accountNumber: string
    bankName: string
    ifscCode: string
    branchName?: string
    accountType: string
  }
  earningsHistory: Array<{
    amount: number
    netAmount: number
    status: string
    createdAt: string
  }>
}

export interface ApiResponse<T> {
  success: boolean
  data?: T
  error?: {
    code: string
    message: string
  }
}

export interface AdminDashboardDto {
  totalUsers: number
  totalStudents: number
  totalTutors: number
  totalAdmins: number
  activeUsers: number
  totalSessions: number
  completedSessions: number
  upcomingSessions: number
  totalRevenue: number
  totalEarnings: number
  totalWithdrawals: number
  pendingTutorVerifications: number
  pendingWithdrawals: number
  recentUsers: Array<{
    userId: string
    username: string
    email: string
    role: string
    createdAt: string
  }>
  recentSessions: Array<{
    sessionId: string
    title: string
    tutorName: string
    scheduledAt: string
    status: string
  }>
}

export interface AdminUser {
  id: string
  username: string
  email: string
  phoneNumber?: string
  role: string
  isActive: boolean
  isEmailVerified: boolean
  isPhoneVerified: boolean
  createdAt: string
  lastLoginAt?: string
  verificationStatus?: string
}

export interface PaginatedResponse<T> {
  items: T[]
  pagination: {
    currentPage: number
    pageSize: number
    totalRecords: number
    totalPages?: number
  }
}

export interface TutorVerificationDto {
  id: string
  tutorId: string
  tutorName: string
  tutorEmail: string
  skills: string
  experience: number
  education: string
  certifications: string
  resumeUrl?: string
  introVideoUrl?: string
  govtIdUrl?: string
  submittedAt: string
}

export interface VerifiedTutorDto {
  id: string
  name: string
  email: string
  verifiedAt: string
  verifiedBy: string
}

export interface FinancialsResponse {
  summary: {
    totalRevenue: number
    totalSessionBookings: number
    totalWithdrawals: number
    totalPlatformFees: number
    totalTutorEarnings: number
    netProfit: number
  }
  transactions: Array<{
    id: string
    type: string
    userId: string
    sessionId?: string
    amount: number
    status: string
    createdAt: string
  }>
  pagination: {
    currentPage: number
    pageSize: number
    totalRecords: number
  }
}

export interface WhatsAppCampaign {
  id: string
  name: string
  message: string
  targetAudience?: string
  status: string
  totalRecipients: number
  sentCount: number
  deliveredCount: number
  failedCount: number
  scheduledAt?: string
  createdAt: string
  createdBy?: string
}

/**
 * Get pending payouts
 */
export const getPendingPayouts = async (): Promise<AdminPayoutRequest[]> => {
  const response = await apiGet<ApiResponse<AdminPayoutRequest[]>>('/admin/payouts/pending')
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch pending payouts')
  }
  return response.data
}

/**
 * Approve payout
 */
export const approvePayout = async (id: string, notes?: string): Promise<void> => {
  const response = await apiPost<ApiResponse<void>>(`/admin/payouts/${id}/approve`, { notes })
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to approve payout')
  }
}

/**
 * Reject payout
 */
export const rejectPayout = async (id: string, reason: string): Promise<void> => {
  const response = await apiPost<ApiResponse<void>>(`/admin/payouts/${id}/reject`, { reason })
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to reject payout')
  }
}

/**
 * Mark payout as paid
 */
export const markPayoutAsPaid = async (id: string, transactionReference?: string): Promise<void> => {
  const response = await apiPost<ApiResponse<void>>(`/admin/payouts/${id}/mark-paid`, {
    transactionReference,
  })
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to mark payout as paid')
  }
}

/**
 * Revenue summary for admin
 */
export const getRevenueSummary = async (period?: string): Promise<any> => {
  const url = period ? `/admin/payouts/revenue-summary?period=${period}` : '/admin/payouts/revenue-summary'
  return apiGet<any>(url)
}

/**
 * Get all payout requests (with optional status filter)
 */
export const getAllPayouts = async (status?: string): Promise<any[]> => {
  const url = status ? `/admin/payouts?status=${status}` : '/admin/payouts'
  const response = await apiGet<{ data: any[]; total: number }>(url)
  return response?.data ?? []
}

/**
 * Admin dashboard overview
 */
export const getAdminDashboard = async (): Promise<AdminDashboardDto> => {
  const response = await apiGet<ApiResponse<AdminDashboardDto>>('/admin/dashboard')
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch dashboard')
  }
  return response.data
}

/**
 * Admin users list
 */
export const getAdminUsers = async (params?: {
  page?: number
  pageSize?: number
  role?: string
}): Promise<PaginatedResponse<AdminUser>> => {
  const queryParams = new URLSearchParams()
  if (params?.page) queryParams.append('page', params.page.toString())
  if (params?.pageSize) queryParams.append('pageSize', params.pageSize.toString())
  if (params?.role) queryParams.append('role', params.role)
  const query = queryParams.toString() ? `?${queryParams.toString()}` : ''

  const response = await apiGet<ApiResponse<any>>(`/admin/users${query}`)
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch users')
  }
  return response.data
}

/**
 * Create user (admin)
 */
export const createAdminUser = async (data: {
  username: string
  email: string
  phoneNumber?: string
  role: string
  password?: string
  firstName?: string
  lastName?: string
}): Promise<void> => {
  const response = await apiPost<ApiResponse<void>>('/admin/users', data)
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to create user')
  }
}

export const activateAdminUser = async (userId: string): Promise<void> => {
  const response = await apiPut<ApiResponse<void>>(`/admin/users/${userId}/activate`, {})
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to activate user')
  }
}

export const deactivateAdminUser = async (userId: string): Promise<void> => {
  const response = await apiPut<ApiResponse<void>>(`/admin/users/${userId}/deactivate`, {})
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to deactivate user')
  }
}

export const deleteAdminUser = async (userId: string): Promise<void> => {
  const response = await apiDelete<ApiResponse<void>>(`/admin/users/${userId}`)
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to delete user')
  }
}

/**
 * Tutor verification
 */
export const getPendingTutorVerifications = async (): Promise<TutorVerificationDto[]> => {
  const response = await apiGet<ApiResponse<TutorVerificationDto[]>>('/admin/tutors/verification/pending')
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch pending tutors')
  }
  return response.data
}

export const getVerifiedTutors = async (): Promise<VerifiedTutorDto[]> => {
  const response = await apiGet<ApiResponse<VerifiedTutorDto[]>>('/admin/tutors/verification/verified')
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch verified tutors')
  }
  return response.data
}

export const approveTutorVerification = async (id: string, notes?: string): Promise<void> => {
  const response = await apiPost<ApiResponse<void>>(`/admin/tutors/verification/${id}/approve`, { notes })
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to approve tutor')
  }
}

export const rejectTutorVerification = async (id: string, reason: string, notes?: string): Promise<void> => {
  const response = await apiPost<ApiResponse<void>>(`/admin/tutors/verification/${id}/reject`, { reason, notes })
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to reject tutor')
  }
}

/**
 * Financials
 */
export const getAdminFinancials = async (params?: { page?: number; pageSize?: number }): Promise<FinancialsResponse> => {
  const query = params
    ? `?page=${params.page || 1}&pageSize=${params.pageSize || 20}`
    : ''
  const response = await apiGet<ApiResponse<FinancialsResponse>>(`/admin/financials${query}`)
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch financials')
  }
  return response.data
}

/**
 * WhatsApp campaigns
 */
export const getWhatsAppCampaigns = async (): Promise<WhatsAppCampaign[]> => {
  try {
    const response = await apiGet<ApiResponse<WhatsAppCampaign[]>>('/admin/campaigns')
    if (!response.success) {
      throw new Error(response.error?.message || 'Failed to fetch campaigns')
    }
    // Return empty array if data is undefined/null, otherwise return the data
    return response.data || []
  } catch (error: any) {
    console.error('Error fetching WhatsApp campaigns:', error)
    // If it's a network error, provide a more helpful message
    if (error.message?.includes('Failed to fetch') || error.message?.includes('NetworkError')) {
      throw new Error('Unable to connect to server. Please check your connection and try again.')
    }
    throw error
  }
}

export const createWhatsAppCampaign = async (data: FormData): Promise<void> => {
  const response = await apiPostFormData<ApiResponse<void>>('/admin/campaigns/whatsapp', data)
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to create campaign')
  }
}

/**
 * System settings
 */
export const getSystemSettings = async (): Promise<Record<string, any>> => {
  const response = await apiGet<ApiResponse<Record<string, any>>>('/admin/settings')
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch settings')
  }
  return response.data
}

export const updateSystemSetting = async (key: string, value: any): Promise<void> => {
  const response = await apiPut<ApiResponse<void>>(`/admin/settings/${key}`, { value })
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to update setting')
  }
}

// Blog Management APIs
export interface Blog {
  id: string
  title: string
  slug: string
  content: string
  summary?: string
  thumbnailUrl?: string
  authorId: string
  authorName: string
  categoryId: string
  categoryName: string
  tags?: string
  viewCount: number
  isPublished: boolean
  publishedAt?: string
  createdAt: string
  updatedAt: string
}

export interface CreateBlogRequest {
  title: string
  content: string
  summary?: string
  thumbnailUrl?: string
  categoryId: string
  tags?: string
  isPublished: boolean
}

export interface UpdateBlogRequest {
  title: string
  content: string
  summary?: string
  thumbnailUrl?: string
  categoryId: string
  tags?: string
  isPublished: boolean
}

/**
 * Get all blogs (admin - includes unpublished)
 */
export const getAdminBlogs = async (): Promise<Blog[]> => {
  const response = await apiGet<ApiResponse<Blog[]>>('/admin/blogs')
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch blogs')
  }
  return response.data
}

/**
 * Get blog by ID
 */
export const getAdminBlog = async (id: string): Promise<Blog> => {
  const response = await apiGet<ApiResponse<Blog>>(`/admin/blogs/${id}`)
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch blog')
  }
  return response.data
}

/**
 * Create a new blog
 */
export const createBlog = async (blog: CreateBlogRequest): Promise<Blog> => {
  const response = await apiPost<ApiResponse<Blog>>('/admin/blogs', blog)
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to create blog')
  }
  return response.data
}

/**
 * Update a blog
 */
export const updateBlog = async (id: string, blog: UpdateBlogRequest): Promise<Blog> => {
  const response = await apiPut<ApiResponse<Blog>>(`/admin/blogs/${id}`, blog)
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to update blog')
  }
  return response.data
}

/**
 * Update blog status (publish/unpublish)
 */
export const updateBlogStatus = async (id: string, isPublished: boolean): Promise<void> => {
  const response = await apiPatch<ApiResponse<void>>(`/admin/blogs/${id}/status`, { isPublished })
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to update blog status')
  }
}

/**
 * Delete a blog
 */
export const deleteBlog = async (id: string): Promise<void> => {
  const response = await apiDelete<ApiResponse<void>>(`/admin/blogs/${id}`)
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to delete blog')
  }
}

// Subject Management APIs
export interface Subject {
  id: string
  name: string
  description?: string
  isActive: boolean
  createdAt: string
}

export interface CreateSubjectRequest {
  name: string
  description?: string
  isActive: boolean
}

/**
 * Get all subjects (admin)
 */
export const getAdminSubjects = async (): Promise<Subject[]> => {
  const response = await apiGet<ApiResponse<Subject[]>>('/admin/subjects')
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch subjects')
  }
  return response.data
}

/**
 * Create a new subject
 */
export const createSubject = async (subject: CreateSubjectRequest): Promise<Subject> => {
  const response = await apiPost<ApiResponse<Subject>>('/admin/subjects', subject)
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to create subject')
  }
  return response.data
}

/**
 * Update a subject
 */
export const updateSubject = async (id: string, subject: CreateSubjectRequest): Promise<Subject> => {
  const response = await apiPut<ApiResponse<Subject>>(`/admin/subjects/${id}`, subject)
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to update subject')
  }
  return response.data
}

/**
 * Delete a subject
 */
export const deleteSubject = async (id: string): Promise<void> => {
  const response = await apiDelete<ApiResponse<void>>(`/admin/subjects/${id}`)
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to delete subject')
  }
}
