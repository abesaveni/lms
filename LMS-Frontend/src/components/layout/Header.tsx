import { useState, useEffect } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { Menu, X, User, LogOut, MessageSquare } from 'lucide-react'
import Button from '../ui/Button'
import { Avatar } from '../ui/Avatar'
import logoImage from '../../assets/logo.png'
import NotificationBell from './NotificationBell'
import { getCurrentUser, logout } from '../../utils/auth'

const Header = () => {
  const [isMenuOpen, setIsMenuOpen] = useState(false)
  const [isProfileOpen, setIsProfileOpen] = useState(false)
  const navigate = useNavigate()
  const [currentUser, setCurrentUser] = useState(getCurrentUser())
  
  useEffect(() => {
    const handleUpdate = () => {
      setCurrentUser(getCurrentUser())
    }
    window.addEventListener('tokenUpdated', handleUpdate)
    window.addEventListener('profileUpdated', handleUpdate)
    return () => {
      window.removeEventListener('tokenUpdated', handleUpdate)
      window.removeEventListener('profileUpdated', handleUpdate)
    }
  }, [])

  const isAuthenticated = !!currentUser
  const role = currentUser?.role?.toLowerCase() || 'student'

  const inboxPath = role === 'tutor' ? '/tutor/inbox' : role === 'admin' ? '/admin/inbox' : '/student/inbox'
  const dashboardPath = role === 'tutor' ? '/tutor/dashboard' : role === 'admin' ? '/admin/dashboard' : '/student/dashboard'
  const profilePath = role === 'tutor' ? '/tutor/profile' : '/student/profile'

  const handleLogout = async () => {
    setIsProfileOpen(false)
    setIsMenuOpen(false)
    await logout()
  }

  const navLinks = [
    { label: 'Home', path: '/' },
    { label: 'Find Tutors', path: '/find-tutors' },
    { label: 'Sessions', path: '/sessions' },
    { label: 'About Us', path: '/about-us' },
  ]

  return (
    <header className="sticky top-0 z-50 bg-white/95 backdrop-blur-sm border-b border-gray-200">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex items-center justify-between h-16">
          {/* Logo */}
          <Link to="/" className="flex items-center gap-2">
            <img 
              src={logoImage} 
              alt="LiveExpert.AI Logo" 
              className="h-10 w-auto"
            />
          </Link>

          {/* Desktop Navigation */}
          <nav className="hidden md:flex items-center gap-8">
            {navLinks.map((link) => (
              <Link
                key={link.path}
                to={link.path}
                className="text-gray-700 hover:text-primary-600 transition-colors font-medium"
              >
                {link.label}
              </Link>
            ))}
          </nav>

          {/* Auth Buttons / Profile */}
          <div className="hidden md:flex items-center gap-4">
            {isAuthenticated ? (
              <>
                <Link
                  to={inboxPath}
                  className="p-2 rounded-lg hover:bg-gray-100 transition-colors relative"
                >
                  <MessageSquare className="w-5 h-5 text-gray-700" />
                  <span className="absolute top-0 right-0 w-2 h-2 bg-red-500 rounded-full"></span>
                </Link>
                <NotificationBell />
                <div className="relative">
                  <button
                    onClick={() => setIsProfileOpen(!isProfileOpen)}
                    className="flex items-center gap-2 p-2 rounded-lg hover:bg-gray-100 transition-colors"
                  >
                    <Avatar 
                      src={currentUser?.profileImage} 
                      name={currentUser?.username || 'User'} 
                      size="sm" 
                    />
                  </button>
                  {isProfileOpen && (
                    <div className="absolute right-0 mt-2 w-48 bg-white rounded-lg shadow-lg border border-gray-200 py-2">
                      <Link
                        to={dashboardPath}
                        className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                        onClick={() => setIsProfileOpen(false)}
                      >
                        Dashboard
                      </Link>
                      <Link
                        to={profilePath}
                        className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                        onClick={() => setIsProfileOpen(false)}
                      >
                        Profile
                      </Link>
                      <Link
                        to={inboxPath}
                        className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                        onClick={() => setIsProfileOpen(false)}
                      >
                        Inbox
                      </Link>
                      <button
                        className="w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-gray-100 flex items-center gap-2"
                        onClick={handleLogout}
                      >
                        <LogOut className="w-4 h-4" />
                        Logout
                      </button>
                    </div>
                  )}
                </div>
              </>
            ) : (
              <>
                <button
                  onClick={() => navigate('/login')}
                  className="p-2 rounded-lg hover:bg-gray-100 transition-colors"
                >
                  <User className="w-5 h-5 text-gray-700" />
                </button>
                <button
                  onClick={() => navigate('/join-us')}
                  className="px-6 py-2 bg-gray-900 text-white rounded-lg font-medium hover:bg-gray-800 transition-colors"
                >
                  JOIN US
                </button>
              </>
            )}
          </div>

          {/* Mobile Menu Button */}
          <button
            className="md:hidden p-2 rounded-lg hover:bg-gray-100"
            onClick={() => setIsMenuOpen(!isMenuOpen)}
          >
            {isMenuOpen ? <X className="w-6 h-6" /> : <Menu className="w-6 h-6" />}
          </button>
        </div>

        {/* Mobile Menu */}
        {isMenuOpen && (
          <div className="md:hidden py-4 border-t border-gray-200">
            <nav className="flex flex-col gap-4">
              {navLinks.map((link) => (
                <Link
                  key={link.path}
                  to={link.path}
                  className="text-gray-700 hover:text-primary-600 transition-colors font-medium"
                  onClick={() => setIsMenuOpen(false)}
                >
                  {link.label}
                </Link>
              ))}
              <div className="pt-4 border-t border-gray-200 flex flex-col gap-2">
                {!isAuthenticated ? (
                  <>
                    <Button variant="ghost" fullWidth onClick={() => { navigate('/login'); setIsMenuOpen(false) }}>
                      Login
                    </Button>
                    <Button
                      variant="primary"
                      fullWidth
                      onClick={() => {
                        navigate('/join-us')
                        setIsMenuOpen(false)
                      }}
                    >
                      Get Started
                    </Button>
                  </>
                ) : (
                  <>
                    <Button
                      variant="ghost"
                      fullWidth
                      onClick={() => {
                        navigate(dashboardPath)
                        setIsMenuOpen(false)
                      }}
                      className="justify-start gap-2"
                    >
                      <User className="w-4 h-4" />
                      Dashboard
                    </Button>
                    <Button
                      variant="ghost"
                      fullWidth
                      onClick={handleLogout}
                      className="justify-start gap-2 text-red-600 hover:text-red-700 hover:bg-red-50"
                    >
                      <LogOut className="w-4 h-4" />
                      Logout
                    </Button>
                  </>
                )}
              </div>
            </nav>
          </div>
        )}
      </div>

    </header>
  )
}

export default Header
