import { ReactNode, useState, useEffect } from 'react'
import { useNavigate, useLocation, Link } from 'react-router-dom'
import { Menu, X, LogOut } from 'lucide-react'
import { Avatar } from '../ui/Avatar'
import { getCurrentUser, getCurrentUserRole } from '../../utils/auth'
import logoImage from '../../assets/logo.png'
import AdminSidebar from './AdminSidebar'
import StudentSidebar from './StudentSidebar'
import TutorSidebar from './TutorSidebar'

interface DashboardLayoutProps {
  children: ReactNode
  role: 'admin' | 'student' | 'tutor'
}

const DashboardLayout = ({ children, role }: DashboardLayoutProps) => {
  const [sidebarOpen, setSidebarOpen] = useState(true)
  const navigate = useNavigate()
  const location = useLocation()
  const [user, setUser] = useState(getCurrentUser())

  useEffect(() => {
    const handleUpdate = () => setUser(getCurrentUser())
    window.addEventListener('tokenUpdated', handleUpdate)
    window.addEventListener('profileUpdated', handleUpdate)
    return () => {
      window.removeEventListener('tokenUpdated', handleUpdate)
      window.removeEventListener('profileUpdated', handleUpdate)
    }
  }, [])


  // Verify route matches user's role and handle cross-tab login changes
  useEffect(() => {
    const verifyRole = () => {
      const userRole = getCurrentUserRole()?.toLowerCase()
      const normalizedUserRole = userRole === 'admin' ? 'admin' : userRole === 'tutor' ? 'tutor' : 'student'

      // If user role doesn't match the route's required role, redirect
      if (normalizedUserRole !== role) {
        if (normalizedUserRole === 'admin') {
          navigate('/admin/dashboard', { replace: true })
        } else if (normalizedUserRole === 'tutor') {
          navigate('/tutor/dashboard', { replace: true })
        } else if (normalizedUserRole === 'student') {
          navigate('/student/dashboard', { replace: true })
        } else {
          // No user or invalid role, redirect to login
          navigate('/login', { replace: true })
        }
      }
    }

    // Check on mount
    verifyRole()

    // Listen for storage changes (cross-tab login/logout)
    const handleStorageChange = (e: StorageEvent) => {
      if (e.key === 'user' || e.key === 'token') {
        // User changed in another tab, verify role again
        verifyRole()
      }
    }

    // Listen for custom tokenUpdated event (same-tab login)
    const handleTokenUpdate = () => {
      verifyRole()
    }

    window.addEventListener('storage', handleStorageChange)
    window.addEventListener('tokenUpdated', handleTokenUpdate)

    return () => {
      window.removeEventListener('storage', handleStorageChange)
      window.removeEventListener('tokenUpdated', handleTokenUpdate)
    }
  }, [role, navigate])

  useEffect(() => {
    const verifyTutorAccess = async () => {
      if (role !== 'tutor') return
      // Always allow these paths regardless of verification status
      if (location.pathname === '/tutor/verification-pending') return
      if (location.pathname === '/tutor/profile-settings') return
      if (location.pathname === '/tutor/onboarding') return
      if (location.pathname === '/tutor/profile') return

      try {
        const { getCurrentUserProfile } = await import('../../services/usersApi')
        const profile = await getCurrentUserProfile()
        const status = profile.tutorProfile?.verificationStatus?.toLowerCase()
        // Only redirect if status is explicitly a blocked state (not new/unstarted tutors)
        if (status && status !== 'approved' && status !== 'notstarted' && status !== 'not_started' && status !== '') {
          navigate('/tutor/verification-pending', { replace: true })
        }
      } catch (error) {
        // If we cannot verify, avoid blocking access here
        console.error('Failed to verify tutor status:', error)
      }
    }

    verifyTutorAccess()
  }, [role, location.pathname, navigate])

  const handleLogout = async () => {
    const { logout } = await import('../../utils/auth')
    await logout()
  }

  const renderSidebar = () => {
    switch (role) {
      case 'admin':
        return <AdminSidebar isOpen={sidebarOpen} currentPath={location.pathname} />
      case 'student':
        return <StudentSidebar isOpen={sidebarOpen} currentPath={location.pathname} />
      case 'tutor':
        return <TutorSidebar isOpen={sidebarOpen} currentPath={location.pathname} />
      default:
        return null
    }
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Mobile Header */}
      <div className="lg:hidden bg-white border-b border-gray-200 px-4 py-3 flex items-center justify-between">
        <button
          onClick={() => setSidebarOpen(!sidebarOpen)}
          className="p-2 rounded-lg hover:bg-gray-100"
        >
          <Menu className="w-6 h-6" />
        </button>
        <Link to="/">
          <img src={logoImage} alt="LiveExpert.AI" className="h-8 w-auto" />
        </Link>
        <div className="w-10" /> {/* Spacer */}
      </div>

      <div className="flex">
        {/* Sidebar */}
        <aside
          className={`
            fixed inset-y-0 left-0 z-40
            bg-white border-r border-gray-200
            transition-transform duration-300 ease-in-out
            ${sidebarOpen ? 'translate-x-0' : '-translate-x-full'}
            lg:translate-x-0
            w-64
            flex flex-col
            h-screen
          `}
        >
          {/* Logo */}
          <div className="h-16 border-b border-gray-200 flex items-center justify-between px-6 flex-shrink-0">
            <Link to="/">
              <img src={logoImage} alt="LiveExpert.AI" className="h-8 w-auto" />
            </Link>
            <button
              onClick={() => setSidebarOpen(false)}
              className="lg:hidden p-1 rounded hover:bg-gray-100"
            >
              <X className="w-5 h-5" />
            </button>
          </div>

          {/* Sidebar Navigation - Scrollable */}
          <div className="flex-1 overflow-y-auto py-4 min-h-0">
            {renderSidebar()}
          </div>

          {/* User Profile & Logout - Fixed at bottom */}
          <div className="border-t border-gray-200 p-4 flex-shrink-0 bg-white">
            <div className="flex items-center gap-3 mb-3">
              <Avatar 
                src={user?.profileImage} 
                name={user?.username || 'User'} 
                size="md" 
                className="flex-shrink-0" 
              />
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium text-gray-900 truncate">
                  {user?.username || 'User'}
                </p>
                <p className="text-xs text-gray-500 truncate">
                  {user?.email || ''}
                </p>
              </div>
            </div>
            <button
              onClick={handleLogout}
              className="w-full flex items-center gap-2 px-3 py-2 text-sm text-gray-700 hover:bg-gray-100 rounded-lg transition-colors"
            >
              <LogOut className="w-4 h-4" />
              Logout
            </button>
          </div>
        </aside>

        {/* Overlay for mobile */}
        {sidebarOpen && (
          <div
            className="lg:hidden fixed inset-0 bg-black bg-opacity-50 z-30"
            onClick={() => setSidebarOpen(false)}
          />
        )}

        {/* Main Content */}
        <main className="flex-1 lg:ml-64">
          <div className="h-full">
            {children}
          </div>
        </main>
      </div>
    </div>
  )
}

export default DashboardLayout
