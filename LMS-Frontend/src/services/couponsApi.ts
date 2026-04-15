import { apiGet, apiPost, apiPut, ApiResponse } from './api'

export interface CouponDto {
  id: string
  code: string
  description?: string
  discountType: 'Percentage' | 'Flat'
  discountValue: number
  maxDiscountAmount?: number
  minOrderAmount?: number
  maxUses?: number
  usedCount: number
  expiresAt?: string
  isActive: boolean
}

export interface CouponValidationResult {
  isValid: boolean
  message: string
  discountAmount: number
  finalAmount: number
  couponId?: string
}

export interface CreateCouponDto {
  code: string
  description?: string
  discountType: number   // 0 = Percentage, 1 = Flat
  discountValue: number
  maxDiscountAmount?: number
  minOrderAmount?: number
  maxUses?: number
  expiresAt?: string
  tutorId?: string
}

/**
 * Validate a coupon code before booking (Authenticated students)
 */
export const validateCoupon = async (
  code: string,
  orderAmount: number,
  tutorId?: string
): Promise<CouponValidationResult> => {
  const params = new URLSearchParams({ code, orderAmount: orderAmount.toString() })
  if (tutorId) params.append('tutorId', tutorId)
  const response = await apiGet<ApiResponse<CouponValidationResult>>(`/coupons/validate?${params}`)
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to validate coupon')
  }
  return response.data
}

/**
 * Admin: Create a coupon
 */
export const createCoupon = async (data: CreateCouponDto): Promise<CouponDto> => {
  const response = await apiPost<ApiResponse<CouponDto>>('/coupons', data)
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to create coupon')
  }
  return response.data
}

/**
 * Admin: Toggle coupon active/inactive
 */
export const toggleCoupon = async (id: string, isActive: boolean): Promise<void> => {
  const response = await apiPut<ApiResponse<void>>(`/coupons/${id}/toggle`, { isActive })
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to update coupon')
  }
}
