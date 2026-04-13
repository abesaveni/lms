import { apiGet, apiPost, apiPut } from './api'

export interface CookieConsent {
  id?: string
  necessary: boolean
  functional: boolean
  analytics: boolean
  marketing: boolean
  consentGivenAt?: string
  consentUpdatedAt?: string
}

export interface UserConsent {
  id: string
  consentType: 'GoogleLogin' | 'GoogleCalendar'
  consentTypeName: string
  granted: boolean
  grantedAt?: string
  revokedAt?: string
}

export interface ApiResponse<T> {
  success: boolean
  data?: T
  error?: {
    code: string
    message: string
  }
}

/**
 * Save cookie consent
 */
export const saveCookieConsent = async (consent: {
  functional: boolean
  analytics: boolean
  marketing: boolean
}): Promise<CookieConsent> => {
  const response = await apiPost<ApiResponse<CookieConsent>>('/consent/cookies', {
    functional: consent.functional,
    analytics: consent.analytics,
    marketing: consent.marketing,
  })
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to save cookie consent')
  }
  return response.data
}

/**
 * Update cookie consent
 */
export const updateCookieConsent = async (consent: {
  functional: boolean
  analytics: boolean
  marketing: boolean
}): Promise<CookieConsent> => {
  const response = await apiPut<ApiResponse<CookieConsent>>('/consent/cookies', {
    functional: consent.functional,
    analytics: consent.analytics,
    marketing: consent.marketing,
  })
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to update cookie consent')
  }
  return response.data
}

/**
 * Get cookie consent (from backend - for logged-in users)
 */
export const getCookieConsent = async (): Promise<CookieConsent | null> => {
  try {
    const response = await apiGet<ApiResponse<CookieConsent | null>>('/consent/cookies')
    if (!response.success) {
      return null
    }
    return response.data || null
  } catch (error) {
    return null
  }
}

/**
 * Save user consent (Google OAuth)
 */
export const saveUserConsent = async (
  consentType: 'GoogleLogin' | 'GoogleCalendar',
  granted: boolean
): Promise<UserConsent> => {
  const response = await apiPost<ApiResponse<UserConsent>>('/consent/user', {
    consentType,
    granted,
  })
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to save user consent')
  }
  return response.data
}

/**
 * Revoke user consent
 */
export const revokeUserConsent = async (
  consentType: 'GoogleLogin' | 'GoogleCalendar'
): Promise<void> => {
  const response = await apiPost<ApiResponse<void>>('/consent/user/revoke', {
    consentType,
  })
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to revoke consent')
  }
}

/**
 * Get user consents
 */
export const getUserConsents = async (): Promise<UserConsent[]> => {
  const response = await apiGet<ApiResponse<UserConsent[]>>('/consent/user')
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch user consents')
  }
  return response.data
}

/**
 * Get consent from localStorage
 */
export const getCookieConsentFromStorage = (): CookieConsent | null => {
  try {
    const stored = localStorage.getItem('cookieConsent')
    if (!stored) return null
    return JSON.parse(stored)
  } catch {
    return null
  }
}

/**
 * Save consent to localStorage
 */
export const saveCookieConsentToStorage = (consent: CookieConsent): void => {
  localStorage.setItem('cookieConsent', JSON.stringify(consent))
}

/**
 * Check if consent has been given
 */
export const hasCookieConsent = (): boolean => {
  const consent = getCookieConsentFromStorage()
  return consent !== null
}

/**
 * Check if specific cookie category is consented
 */
export const hasCookieCategoryConsent = (category: 'functional' | 'analytics' | 'marketing'): boolean => {
  const consent = getCookieConsentFromStorage()
  if (!consent) return false
  
  switch (category) {
    case 'functional':
      return consent.functional
    case 'analytics':
      return consent.analytics
    case 'marketing':
      return consent.marketing
    default:
      return false
  }
}
