import { apiPost, ApiResponse } from './api'

export interface VerifySessionPaymentRequest {
  razorpayOrderId: string
  razorpayPaymentId: string
  razorpaySignature: string
}

export const verifySessionPayment = async (data: VerifySessionPaymentRequest): Promise<void> => {
  const response = await apiPost<ApiResponse<void>>('/payments/sessions/verify', data)
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to verify payment')
  }
}
