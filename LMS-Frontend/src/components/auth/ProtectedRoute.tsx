import { useEffect, useState } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { getCurrentUser, getCurrentUserRole } from '../../utils/auth'

interface ProtectedRouteProps {
  children: React.ReactNode
  requiredRole?: 'admin' | 'student' | 'tutor'
  redirectTo?: string
}

/**
 * ProtectedRoute component that:
 * 1. Checks if user is authenticated
 * 2. Verifies user's role matches required role
 * 3. Redirects if there's a mismatch
 * 4. Listens for localStorage changes (cross-tab login)
 */
const ProtectedRoute = ({ children, requiredRole }: ProtectedRouteProps) => {
  const navigate = useNavigate()
  const location = useLocation()
  const [isChecking, setIsChecking] = useState(true)

  useEffect(() => {
    const checkAuth = () => {
      setIsChecking(true)
      const user = getCurrentUser()
      const userRole = getCurrentUserRole()?.toLowerCase()

      // If no user, redirect to login
      if (!user) {
        navigate('/login', { state: { from: location.pathname } })
        return
      }

      // If role is required and doesn't match, redirect
      if (requiredRole) {
        const normalizedUserRole = userRole === 'admin' ? 'admin' : userRole === 'tutor' ? 'tutor' : 'student'

        if (normalizedUserRole !== requiredRole) {
          // Redirect to appropriate dashboard based on user's actual role
          if (normalizedUserRole === 'admin') {
            navigate('/admin/dashboard')
          } else if (normalizedUserRole === 'tutor') {
            navigate('/tutor/dashboard')
          } else {
            navigate('/student/dashboard')
          }
          return
        }
      }

      setIsChecking(false)
    }

    // Check on mount
    checkAuth()

    // Listen for storage changes (cross-tab login/logout)
    const handleStorageChange = (e: StorageEvent) => {
      if (e.key === 'user' || e.key === 'token') {
        checkAuth()
      }
    }

    // Listen for custom tokenUpdated event (same-tab login)
    const handleTokenUpdate = () => {
      checkAuth()
    }

    window.addEventListener('storage', handleStorageChange)
    window.addEventListener('tokenUpdated', handleTokenUpdate)

    return () => {
      window.removeEventListener('storage', handleStorageChange)
      window.removeEventListener('tokenUpdated', handleTokenUpdate)
    }
  }, [navigate, location.pathname, requiredRole])

  if (isChecking) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-primary-600" />
      </div>
    )
  }

  return <>{children}</>
}

export default ProtectedRoute
