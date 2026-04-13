import { apiGet } from './api'

// ─────────────────────────────────────────────────────────────────────────────
// Types
// ─────────────────────────────────────────────────────────────────────────────

export interface BillingItem {
  id: string
  type: 'Session' | 'Course'
  title: string
  amount: number
  platformFee: number
  status: string
  paymentMethod: string
  gatewayPaymentId?: string
  date: string
  extra?: {
    sessionsPurchased?: number
    sessionsCompleted?: number
  }
}

export interface BillingSummary {
  totalSpent: number
  sessionPayments: number
  coursePayments: number
  activeEnrollments: number
}

export interface BillingHistoryResponse {
  data: BillingItem[]
  total: number
  page: number
  pageSize: number
}

// ─────────────────────────────────────────────────────────────────────────────
// Billing
// ─────────────────────────────────────────────────────────────────────────────

export const getBillingHistory = (params?: { page?: number; pageSize?: number }) => {
  const qs = new URLSearchParams()
  if (params?.page) qs.set('page', String(params.page))
  if (params?.pageSize) qs.set('pageSize', String(params.pageSize))
  const query = qs.toString()
  return apiGet<BillingHistoryResponse>(`/billing/history${query ? `?${query}` : ''}`)
}

export const getBillingSummary = () =>
  apiGet<BillingSummary>('/billing/summary')
