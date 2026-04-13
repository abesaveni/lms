/**
 * Auth utility functions
 */

export interface User {
  id: string
  username: string
  email: string
  role: string
  profileImage?: string
}

/**
 * Get current user from localStorage
 */
export const getCurrentUser = (): User | null => {
  try {
    const userStr = localStorage.getItem('user')
    if (!userStr) return null
    return JSON.parse(userStr)
  } catch {
    return null
  }
}

/**
 * Get current user role
 */
export const getCurrentUserRole = (): string | null => {
  const user = getCurrentUser()
  return user?.role || null
}

/**
 * Check if current user is admin
 */
export const isAdmin = (): boolean => {
  const role = getCurrentUserRole()
  return role === 'Admin' || role === 'admin'
}

/**
 * Decode JWT token to get user info (fallback)
 */
export const decodeToken = (token: string): any => {
  try {
    const base64Url = token.split('.')[1]
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    )
    return JSON.parse(jsonPayload)
  } catch {
    return null
  }
}

/**
 * Get user role from token (fallback if user not in localStorage)
 */
export const getUserRoleFromToken = (): string | null => {
  const token = localStorage.getItem('token')
  if (!token) return null
  
  const decoded = decodeToken(token)
  return decoded?.role || decoded?.Role || null
}

/**
 * Check if JWT token is expired
 */
export const isTokenExpired = (token: string): boolean => {
  try {
    const decoded = decodeToken(token)
    if (!decoded || !decoded.exp) return true
    
    // exp is in seconds, convert to milliseconds
    const expirationTime = decoded.exp * 1000
    const currentTime = Date.now()
    
    // Consider token expired if it expires within the next minute (buffer)
    return expirationTime < (currentTime + 60000)
  } catch {
    return true
  }
}

/**
 * Check if current token is expired
 */
export const isCurrentTokenExpired = (): boolean => {
  const token = localStorage.getItem('token')
  if (!token) return true
  return isTokenExpired(token)
}

/**
 * Logout user and clear all auth data
 */
export const logout = async () => {
  // Disconnect SignalR if available
  try {
    const { signalRService } = await import('../services/signalr')
    await signalRService.disconnect()
  } catch (error) {
    // SignalR service might not be available, ignore
  }
  
  // Clear all auth data
  localStorage.removeItem('token')
  localStorage.removeItem('refreshToken')
  localStorage.removeItem('user')
  
  // Dispatch event to notify other components
  window.dispatchEvent(new Event('tokenUpdated'))
  
  // Redirect to login page
  if (window.location.pathname !== '/login') {
    window.location.href = '/login'
  }
}
