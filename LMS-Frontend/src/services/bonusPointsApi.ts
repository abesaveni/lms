import { apiGet, ApiResponse } from './api'

export interface BonusPointItem {
  id: string
  points: number
  reason: string
  createdAt: string
}

export interface BonusPointsSummary {
  totalPoints: number
  items: BonusPointItem[]
}

export const getBonusPointsSummary = async (): Promise<BonusPointsSummary> => {
  const response = await apiGet<ApiResponse<BonusPointsSummary>>('/bonus-points/summary')
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to load bonus points')
  }
  return response.data
}
