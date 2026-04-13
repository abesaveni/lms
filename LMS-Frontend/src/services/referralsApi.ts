import { apiGet } from './api'

export interface ApiResponse<T> {
  success: boolean
  data?: T
  error?: {
    code: string
    message: string
  }
}

export interface ReferralCodeDto {
  referralCode: string
  totalReferrals: number
  totalEarnings: number
}

export interface ReferralStatsDto {
  totalReferrals: number
  successfulReferrals: number
  totalEarnings: number
  pendingEarnings: number
}

export interface ReferralHistoryDto {
  id: string
  referredUserName: string
  referredAt: string
  reward: number
  isRewardClaimed: boolean
  status: string
}

export const getReferralCode = async (): Promise<ReferralCodeDto> => {
  const response = await apiGet<ApiResponse<ReferralCodeDto>>('/shared/referrals/code')
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to load referral code')
  }
  return response.data
}

export const getReferralStats = async (): Promise<ReferralStatsDto> => {
  const response = await apiGet<ApiResponse<ReferralStatsDto>>('/shared/referrals/stats')
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to load referral stats')
  }
  return response.data
}

export const getReferralHistory = async (): Promise<ReferralHistoryDto[]> => {
  const response = await apiGet<ApiResponse<ReferralHistoryDto[]>>('/shared/referrals/history')
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to load referral history')
  }
  return response.data
}
