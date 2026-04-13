import { apiGet, apiPut, apiDelete } from './api'

export interface ApiResponse<T> {
  success: boolean
  data?: T
  error?: {
    code: string
    message: string
  }
}

export interface ApiSetting {
  keyName: string
  value: string
}

export interface UpdateApiSettingRequest {
  value: string
  description?: string
  updateEnvFile?: boolean
}

/**
 * Get all API settings for a provider
 */
export const getApiSettings = async (provider: string): Promise<Record<string, string>> => {
  const response = await apiGet<ApiResponse<Record<string, string>>>(`/admin/api-settings/${provider}`)
  if (!response.success || !response.data) {
    throw new Error(response.error?.message || 'Failed to fetch API settings')
  }
  return response.data
}

/**
 * Update an API setting
 */
export const updateApiSetting = async (
  provider: string,
  keyName: string,
  data: UpdateApiSettingRequest
): Promise<void> => {
  const response = await apiPut<ApiResponse<void>>(`/admin/api-settings/${provider}/${keyName}`, data)
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to update API setting')
  }
}

/**
 * Delete an API setting
 */
export const deleteApiSetting = async (provider: string, keyName: string): Promise<void> => {
  const response = await apiDelete<ApiResponse<void>>(`/admin/api-settings/${provider}/${keyName}`)
  if (!response.success) {
    throw new Error(response.error?.message || 'Failed to delete API setting')
  }
}

