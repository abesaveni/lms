import { useState, useEffect } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { Calendar, CheckCircle, AlertCircle, Loader } from 'lucide-react'
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card'
import Button from '../../components/ui/Button'

const ConnectCalendar = () => {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const [isConnecting, setIsConnecting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)

  const successParam = searchParams.get('success')

  useEffect(() => {
    if (successParam === 'true') {
      setSuccess(true)
      // Redirect to dashboard after 3 seconds
      setTimeout(() => {
        navigate('/student/dashboard') // or tutor dashboard based on role
      }, 3000)
    } else if (successParam === 'false') {
      setError('Failed to connect Google Calendar. Please try again.')
    }
  }, [successParam, navigate])

  const handleConnect = async () => {
    setIsConnecting(true)
    setError(null)

    try {
      // Get authorization URL from backend
      const { getCalendarAuthUrl } = await import('../../services/calendarApi')
      const authUrl = await getCalendarAuthUrl()
      // Redirect to Google OAuth
      window.location.href = authUrl
    } catch (err: any) {
      console.error('Calendar connect error:', err)
      setError(err.message || 'An error occurred. Please try again.')
      setIsConnecting(false)
    }
  }

  const handleMockConnect = async () => {
    setIsConnecting(true)
    setError(null)

    try {
      const { mockConnectCalendar } = await import('../../services/calendarApi')
      const success = await mockConnectCalendar()
      if (success) {
        setSuccess(true)
        setTimeout(() => {
          const user = JSON.parse(localStorage.getItem('user') || '{}')
          navigate(user.role === 'Tutor' ? '/tutor/dashboard' : '/student/dashboard')
        }, 3000)
      } else {
        setError('Failed to create demo connection.')
      }
    } catch (err: any) {
      setError(err.message || 'An error occurred during demo connection.')
    } finally {
      setIsConnecting(false)
    }
  }

  if (success) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50 px-4">
        <Card className="max-w-md w-full text-center p-12">
          <div className="w-16 h-16 mx-auto mb-6 rounded-full bg-green-100 flex items-center justify-center">
            <CheckCircle className="w-8 h-8 text-green-600" />
          </div>
          <h2 className="text-2xl font-bold text-gray-900 mb-4">Calendar Connected!</h2>
          <p className="text-gray-600 mb-6">
            Your Google Calendar has been successfully connected. Redirecting to dashboard...
          </p>
          <Loader className="w-6 h-6 mx-auto animate-spin text-primary-600" />
        </Card>
      </div>
    )
  }

  const isConfigError = error?.toLowerCase().includes('configured') || error?.toLowerCase().includes('config')

  return (
    <div className="max-w-2xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <div className="text-center mb-8">
        <div className="w-16 h-16 mx-auto mb-4 rounded-full bg-primary-100 flex items-center justify-center">
          <Calendar className="w-8 h-8 text-primary-600" />
        </div>
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Connect Google Calendar</h1>
        <p className="text-gray-600">Required to use LiveExpert.AI</p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Why Connect Google Calendar?</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-3">
            <div className="flex items-start gap-3">
              <CheckCircle className="w-5 h-5 text-green-600 flex-shrink-0 mt-0.5" />
              <div>
                <p className="font-medium text-gray-900">Automatic Session Scheduling</p>
                <p className="text-sm text-gray-600">Sessions are automatically added to your calendar</p>
              </div>
            </div>
            <div className="flex items-start gap-3">
              <CheckCircle className="w-5 h-5 text-green-600 flex-shrink-0 mt-0.5" />
              <div>
                <p className="font-medium text-gray-900">Google Meet Integration</p>
                <p className="text-sm text-gray-600">Meet links are created automatically for your sessions</p>
              </div>
            </div>
            <div className="flex items-start gap-3">
              <CheckCircle className="w-5 h-5 text-green-600 flex-shrink-0 mt-0.5" />
              <div>
                <p className="font-medium text-gray-900">Calendar Invites</p>
                <p className="text-sm text-gray-600">Receive calendar invites for all your bookings</p>
              </div>
            </div>
            <div className="flex items-start gap-3">
              <CheckCircle className="w-5 h-5 text-green-600 flex-shrink-0 mt-0.5" />
              <div>
                <p className="font-medium text-gray-900">Availability Sync</p>
                <p className="text-sm text-gray-600">Your calendar availability is synced automatically</p>
              </div>
            </div>
          </div>

          {error && (
            <div className="p-4 bg-red-50 border border-red-200 rounded-lg">
              <div className="flex items-start gap-3">
                <AlertCircle className="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" />
                <div className="flex-1">
                  <p className="font-medium text-red-900">Error</p>
                  <p className="text-sm text-red-700">{error}</p>
                  {isConfigError && (
                    <div className="mt-3 p-3 bg-white rounded border border-red-100">
                      <p className="text-xs text-red-600 mb-2">
                        Admin Tip: Google Calendar Client ID is missing. You can set it in Admin {'>'} API Settings.
                      </p>
                      <Button 
                        size="sm" 
                        variant="ghost" 
                        fullWidth 
                        onClick={handleMockConnect}
                        className="text-xs text-red-700 hover:bg-red-50"
                      >
                        Skip (Connect as Demo)
                      </Button>
                    </div>
                  )}
                </div>
              </div>
            </div>
          )}

          <div className="pt-4 border-t border-gray-200 space-y-3">
            <Button
              fullWidth
              size="lg"
              onClick={handleConnect}
              isLoading={isConnecting}
              disabled={isConnecting}
            >
              <Calendar className="mr-2 w-5 h-5" />
              {isConnecting ? 'Connecting...' : 'Connect Google Calendar'}
            </Button>
          </div>

          <p className="text-xs text-gray-500 text-center">
            By connecting, you authorize LiveExpert.AI to access your Google Calendar for scheduling purposes.
            Your data is encrypted and secure.
          </p>
        </CardContent>
      </Card>
    </div>
  )
}

export default ConnectCalendar
