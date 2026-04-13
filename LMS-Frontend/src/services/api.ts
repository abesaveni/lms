/**
 * API Service - Centralized API configuration and helpers
 */

const API_BASE_URL = (import.meta as any).env?.VITE_API_BASE_URL || 'http://localhost:5128/api'

/**
 * Get authentication token from localStorage
 */
export const getAuthToken = (): string | null => {
  return localStorage.getItem('token')
}

/**
 * Logout user and clear all auth data (imported from auth utils)
 */
const performLogout = async () => {
  const { logout } = await import('../utils/auth')
  await logout()
}

/**
 * Get default headers for API requests
 */
export const getDefaultHeaders = (): HeadersInit => {
  const headers: HeadersInit = {
    'Content-Type': 'application/json',
  }

  const token = getAuthToken()
  if (token) {
    headers['Authorization'] = `Bearer ${token}`
  }

  return headers
}

// Re-export logout from auth utils
export { logout } from '../utils/auth'

export interface ApiResponse<T = any> {
  success: boolean
  data?: T
  error?: {
    code: string
    message: string
  }
}

/**
 * Handle API response
 */
export const handleApiResponse = async <T>(response: Response): Promise<T> => {
  if (!response.ok) {
    let error: any
    try {
      error = await response.json()
    } catch {
      error = { message: 'An error occurred' }
    }

    // Handle 401 Unauthorized - token expired or invalid
    if (response.status === 401) {
      // Check if we have a token (means it's expired/invalid, not missing)
      const token = getAuthToken()
      if (token) {
        // Token exists but is invalid/expired - auto logout
        console.warn('Token expired or invalid. Logging out...')
        await performLogout()
        throw new Error('Your session has expired. Please log in again.')
      }
      // No token - might be expected for anonymous endpoints
      const errorMessage = error?.error?.message || error?.message || 'Unauthorized'
      throw new Error(errorMessage)
    }

    // Handle 402 Payment Required - trial expired or subscription needed
    if (response.status === 402) {
      window.dispatchEvent(new CustomEvent('subscription:required', { detail: error }))
      throw new Error('subscription_required')
    }

    throw new Error(error?.error?.message || error?.message || `HTTP error! status: ${response.status}`)
  }

  const data = await response.json()
  return data
}

/**
 * API request wrapper
 */
export const apiRequest = async <T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> => {
  // Ensure endpoint starts with /
  const normalizedEndpoint = endpoint.startsWith('/') ? endpoint : `/${endpoint}`
  const url = `${API_BASE_URL}${normalizedEndpoint}`
  const headers = {
    ...getDefaultHeaders(),
    ...options.headers,
  }

  const response = await fetch(url, {
    ...options,
    headers,
  })

  return handleApiResponse<T>(response)
}

/**
 * GET request
 */
export const apiGet = <T>(endpoint: string): Promise<T> => {
  return apiRequest<T>(endpoint, { method: 'GET' })
}

/**
 * Public API request wrapper (no auth headers)
 */
export const apiRequestPublic = async <T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> => {
  const normalizedEndpoint = endpoint.startsWith('/') ? endpoint : `/${endpoint}`
  const url = `${API_BASE_URL}${normalizedEndpoint}`
  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    ...options.headers,
  }

  const response = await fetch(url, {
    ...options,
    headers,
  })

  return handleApiResponse<T>(response)
}

/**
 * Public GET request (no auth headers)
 */
export const apiGetPublic = <T>(endpoint: string): Promise<T> => {
  return apiRequestPublic<T>(endpoint, { method: 'GET' })
}

/**
 * POST request
 */
export const apiPost = <T>(endpoint: string, data?: any): Promise<T> => {
  return apiRequest<T>(endpoint, {
    method: 'POST',
    body: data ? JSON.stringify(data) : undefined,
  })
}

/**
 * PUT request
 */
export const apiPut = <T>(endpoint: string, data?: any): Promise<T> => {
  return apiRequest<T>(endpoint, {
    method: 'PUT',
    body: data ? JSON.stringify(data) : undefined,
  })
}

/**
 * DELETE request
 */
export const apiDelete = <T>(endpoint: string): Promise<T> => {
  return apiRequest<T>(endpoint, { method: 'DELETE' })
}

/**
 * PATCH request
 */
export const apiPatch = <T>(endpoint: string, data?: any): Promise<T> => {
  return apiRequest<T>(endpoint, {
    method: 'PATCH',
    body: data ? JSON.stringify(data) : undefined,
  })
}

/**
 * POST request with FormData (for file uploads)
 */
export const apiPostFormData = <T>(endpoint: string, formData: FormData): Promise<T> => {
  const token = getAuthToken()
  const headers: HeadersInit = {}

  if (token) {
    headers['Authorization'] = `Bearer ${token}`
  }
  // Don't set Content-Type for FormData, browser will set it with boundary

  const normalizedEndpoint = endpoint.startsWith('/') ? endpoint : `/${endpoint}`
  return fetch(`${API_BASE_URL}${normalizedEndpoint}`, {
    method: 'POST',
    headers,
    body: formData,
  }).then(handleApiResponse<T>)
}

/**
 * Helper to get the full URL for a media file
 */
export const getMediaUrl = (path?: string): string | undefined => {
  if (!path) return undefined
  if (path.startsWith('http') || path.startsWith('data:')) return path
  // Remove /api from base URL and prepend to path
  const baseUrl = API_BASE_URL.replace('/api', '')
  const normalizedPath = path.startsWith('/') ? path : `/${path}`
  return `${baseUrl}${normalizedPath}`
}

