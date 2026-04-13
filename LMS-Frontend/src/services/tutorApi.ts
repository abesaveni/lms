import { apiGet, apiPost, apiPut, apiDelete, apiPostFormData } from './api'

export interface EarningsOverview {
  totalEarned: number
  pending: number
  available: number
  paid: number
}

export interface EarningHistory {
  id: string
  sourceType: string
  sourceId: string
  amount: number
  netAmount: number
  commissionAmount: number
  status: string
  createdAt: string
  availableAt?: string
  paidAt?: string
}

export interface BankAccount {
  id: string
  accountHolderName: string
  accountNumber: string // Masked
  bankName: string
  ifscCode: string
  branchName?: string
  accountType: string
  isPrimary: boolean
  isVerified: boolean
}

export interface PayoutRequest {
  id: string
  amount: number
  status: string
  requestedAt: string
  processedAt?: string
  adminNotes?: string
  bankAccount: {
    accountHolderName: string
    bankName: string
    accountNumber: string
  }
}

export interface ApiResponse<T> {
  success: boolean
  data?: T
  error?: {
    code: string
    message: string
  }
}

export interface UpdateTutorProfileRequest {
  firstName?: string
  lastName?: string
  bio?: string
  headline?: string
  hourlyRate?: number
  yearsOfExperience?: number
  education?: string
  certifications?: string
  skills?: string
  languages?: string
  linkedInUrl?: string
  gitHubUrl?: string
  portfolioUrl?: string
  profilePictureUrl?: string
  profilePictureBase64?: string
  profilePictureFileName?: string
  phoneNumber?: string
  language?: string
  timezone?: string
  resumeUrl?: string
}

/**
 * Get earnings overview
 */
export const getEarningsOverview = async (): Promise<EarningsOverview> => {
  const response = await apiGet<ApiResponse<EarningsOverview>>('/tutor/earnings')
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch earnings')
  }
  return response.data
}

/**
 * Get earnings history
 */
export const getEarningsHistory = async (
  sourceType?: string,
  status?: string
): Promise<EarningHistory[]> => {
  const params = new URLSearchParams()
  if (sourceType) params.append('sourceType', sourceType)
  if (status) params.append('status', status)

  const query = params.toString() ? `?${params.toString()}` : ''
  const response = await apiGet<ApiResponse<EarningHistory[]>>(`/tutor/earnings/history${query}`)

  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch earnings history')
  }
  return response.data
}

/**
 * Get bank accounts
 */
export const getBankAccounts = async (): Promise<BankAccount[]> => {
  const response = await apiGet<ApiResponse<BankAccount[]>>('/tutor/bank-accounts')
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch bank accounts')
  }
  return response.data
}

/**
 * Add bank account
 */
export const addBankAccount = async (account: {
  accountHolderName: string
  accountNumber: string
  bankName: string
  ifscCode: string
  branchName?: string
  accountType: string
  isPrimary: boolean
}): Promise<BankAccount> => {
  const response = await apiPost<ApiResponse<BankAccount>>('/tutor/bank-accounts', account)
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to add bank account')
  }
  return response.data
}

/**
 * Update bank account
 */
export const updateBankAccount = async (
  id: string,
  account: {
    accountHolderName: string
    accountNumber?: string
    bankName: string
    ifscCode: string
    branchName?: string
    accountType: string
    isPrimary: boolean
  }
): Promise<BankAccount> => {
  const response = await apiPut<ApiResponse<BankAccount>>(`/tutor/bank-accounts/${id}`, account)
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to update bank account')
  }
  return response.data
}

/**
 * Delete bank account
 */
export const deleteBankAccount = async (id: string): Promise<void> => {
  const response = await apiDelete<ApiResponse<void>>(`/tutor/bank-accounts/${id}`)
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to delete bank account')
  }
}

/**
 * Request payout
 */
export const requestPayout = async (
  bankAccountId: string,
  amount: number
): Promise<PayoutRequest> => {
  const response = await apiPost<ApiResponse<PayoutRequest>>('/tutor/payouts/request', {
    bankAccountId,
    amount,
  })
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to request payout')
  }
  return response.data
}

/**
 * Get payout history
 */
export const getPayoutHistory = async (status?: string): Promise<PayoutRequest[]> => {
  const query = status ? `?status=${status}` : ''
  const response = await apiGet<ApiResponse<PayoutRequest[]>>(`/tutor/payouts${query}`)

  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch payout history')
  }
  return response.data
}

/**
 * Parse resume
 */
export const parseResume = async (file: File): Promise<any> => {
  const formData = new FormData()
  formData.append('resume', file)

  const response = await apiPostFormData<ApiResponse<any>>('/tutor/profile/parse-resume', formData)
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to parse resume')
  }
  return response.data
}

/**
 * Update tutor profile
 */
export const updateTutorProfile = async (data: UpdateTutorProfileRequest): Promise<void> => {
  const response = await apiPut<ApiResponse<void>>('/tutor/profile', data)
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to update tutor profile')
  }
}

/**
 * Change tutor password
 */
export const changePassword = async (data: any): Promise<void> => {
  const response = await apiPost<ApiResponse<void>>('/tutor/change-password', data)
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to change password')
  }
}

export const uploadTutorGovtId = async (file: File): Promise<string> => {
  const formData = new FormData()
  formData.append('govtId', file)
  const response = await apiPostFormData<ApiResponse<string>>('/tutor/profile/govt-id', formData)
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to upload government ID')
  }
  return response.data
}

export const uploadTutorProfileImage = async (file: File): Promise<string> => {
  const formData = new FormData()
  formData.append('File', file)
  const response = await apiPostFormData<ApiResponse<string>>('/tutor/profile/image', formData)
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to upload profile image')
  }
  return response.data
}

export const getTutorProfile = async (): Promise<TutorProfileDto> => {
  const response = await apiGet<ApiResponse<TutorProfileDto>>('/tutor/profile')
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch profile')
  }
  return response.data
}

export const submitTutorVerification = async (payload: {
  govtIdUrl?: string
}): Promise<void> => {
  const response = await apiPost<ApiResponse<void>>('/tutor/profile/submit-verification', payload)
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to submit verification')
  }
}

export interface TutorProfileDto {
  username: string
  firstName: string
  lastName: string
  email: string
  phoneNumber: string
  bio?: string
  headline?: string
  hourlyRate?: number
  yearsOfExperience?: number
  education?: string
  certifications?: string
  skills?: string
  languages?: string
  linkedInUrl?: string
  gitHubUrl?: string
  portfolioUrl?: string
  verificationStatus: string
  averageRating: number
  totalReviews: number
  totalSessions: number
  profilePictureUrl?: string
  language?: string
  timezone?: string
}
