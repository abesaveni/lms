import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { Eye, EyeOff } from 'lucide-react'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'
import { Card } from '../../components/ui/Card'
import logoImage from '../../assets/logo.png'

const Login = () => {
  const navigate = useNavigate()
  const [showPassword, setShowPassword] = useState(false)
  const [formData, setFormData] = useState({
    email: '',
    password: '',
    rememberMe: false,
  })
  const [errors, setErrors] = useState<Record<string, string>>({})
  const [isLoading, setIsLoading] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setErrors({})
    setIsLoading(true)

    // Validation
    const newErrors: Record<string, string> = {}
    if (!formData.email) newErrors.email = 'Email is required'
    if (!formData.password) newErrors.password = 'Password is required'

    if (Object.keys(newErrors).length > 0) {
      setErrors(newErrors)
      setIsLoading(false)
      return
    }

    try {
      // Call login API
      const { apiPost } = await import('../../services/api')
      const response = await apiPost<{
        success: boolean
        data?: {
          accessToken: string
          refreshToken: string
          user: {
            id: string
            username: string
            email: string
            role: string
            profileImage?: string
          }
        }
        error?: { code: string; message: string }
      }>('/auth/login', {
        email: formData.email,
        password: formData.password,
      })

      if (response.success && response.data) {
        // Store token and user info
        localStorage.setItem('token', response.data.accessToken)
        localStorage.setItem('refreshToken', response.data.refreshToken)
        localStorage.setItem('user', JSON.stringify(response.data.user))
        
        // Dispatch event to trigger SignalR connection
        window.dispatchEvent(new Event('tokenUpdated'))

        // Redirect based on role (case-insensitive)
        const role = response.data.user.role?.toLowerCase() || ''
        console.log('Login successful, user role:', role, 'Full user:', response.data.user) // Debug log
        
        // Check for admin (including superadmin email as fallback)
        if (role === 'admin' || formData.email.toLowerCase() === 'superadmin@liveexpert.ai') {
          console.log('Redirecting to admin dashboard')
          navigate('/admin/dashboard')
        } else if (role === 'tutor') {
          console.log('Redirecting to tutor dashboard')
          navigate('/tutor/dashboard')
        } else if (role === 'student') {
          console.log('Redirecting to student dashboard')
          navigate('/student/dashboard')
        } else {
          // Fallback: try to determine from email or default to student
          console.warn('Unknown role:', role, 'defaulting to student dashboard')
          navigate('/student/dashboard')
        }
      } else {
        setErrors({ 
          email: response.error?.message || 'Invalid email or password',
          password: response.error?.message || 'Invalid email or password'
        })
      }
    } catch (error: any) {
      setErrors({
        email: error.message || 'An error occurred. Please try again.',
        password: error.message || 'An error occurred. Please try again.'
      })
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-primary-50 via-white to-accent-50 py-12 px-4 sm:px-6 lg:px-8 relative">
      {/* Back to Home Button */}
      <Link
        to="/"
        className="absolute top-4 left-4 flex items-center gap-2 text-gray-600 hover:text-gray-900 transition-colors"
      >
        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 19l-7-7m0 0l7-7m-7 7h18" />
        </svg>
        <span className="text-sm font-medium">Back to Home</span>
      </Link>

      <div className="max-w-md w-full">
        <div className="text-center mb-8">
          <div className="flex items-center justify-center mb-4">
            <img 
              src={logoImage} 
              alt="LiveExpert.AI Logo" 
              className="h-12 w-auto"
            />
          </div>
          <h2 className="text-3xl font-bold text-gray-900">Welcome back</h2>
          <p className="mt-2 text-gray-600">Sign in to your account</p>
        </div>

        <Card>
          <form onSubmit={handleSubmit} className="space-y-6">
            {/* Email */}
            <Input
              label="Email address"
              type="email"
              value={formData.email}
              onChange={(e) => setFormData({ ...formData, email: e.target.value })}
              error={errors.email}
              placeholder="you@example.com"
              autoComplete="off"
              required
            />

            {/* Password */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">
                Password
              </label>
              <div className="relative">
                <Input
                  type={showPassword ? 'text' : 'password'}
                  value={formData.password}
                  onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                  error={errors.password}
                  placeholder="Enter your password"
                  autoComplete="new-password"
                  className="pr-10"
                  required
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-500 hover:text-gray-700"
                >
                  {showPassword ? <EyeOff className="w-5 h-5" /> : <Eye className="w-5 h-5" />}
                </button>
              </div>
            </div>

            {/* Remember Me & Forgot Password */}
            <div className="flex items-center justify-between">
              <label className="flex items-center">
                <input
                  type="checkbox"
                  checked={formData.rememberMe}
                  onChange={(e) => setFormData({ ...formData, rememberMe: e.target.checked })}
                  className="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
                />
                <span className="ml-2 text-sm text-gray-700">Remember me</span>
              </label>
              <Link
                to="/forgot-password"
                className="text-sm text-primary-600 hover:text-primary-700 font-medium"
              >
                Forgot password?
              </Link>
            </div>

            {/* Submit Button */}
            <Button type="submit" fullWidth isLoading={isLoading}>
              Sign in
            </Button>

            {/* Sign Up Link */}
            <p className="text-center text-sm text-gray-600">
              Don't have an account?{' '}
              <Link to="/join-us" className="text-primary-600 hover:text-primary-700 font-medium">
                Sign up
              </Link>
            </p>
          </form>
        </Card>
      </div>
    </div>
  )
}

export default Login
