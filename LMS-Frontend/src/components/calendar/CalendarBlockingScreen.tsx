import { Calendar, AlertCircle, CheckCircle } from 'lucide-react'
import { Card } from '../ui/Card'
import Button from '../ui/Button'
import { useNavigate } from 'react-router-dom'

interface CalendarBlockingScreenProps {
  userRole: 'student' | 'tutor'
  onConnect?: () => void
  onDismiss?: () => void
}

export const CalendarBlockingScreen = ({ userRole, onConnect, onDismiss }: CalendarBlockingScreenProps) => {
  const navigate = useNavigate()

  const handleConnect = () => {
    if (onConnect) {
      onConnect()
    } else {
      // Navigate to calendar connection page
      navigate('/calendar/connect')
    }
  }

  const studentRestrictions = [
    'Cannot book sessions',
    'Cannot join sessions',
  ]

  const tutorRestrictions = [
    'Cannot create sessions',
    'Cannot start sessions',
    'Cannot receive bookings',
  ]

  const restrictions = userRole === 'student' ? studentRestrictions : tutorRestrictions

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 px-4">
      <Card className="max-w-2xl w-full text-center p-12">
        <div className="w-20 h-20 mx-auto mb-6 rounded-full bg-yellow-100 flex items-center justify-center">
          <Calendar className="w-10 h-10 text-yellow-600" />
        </div>
        
        <h1 className="text-3xl font-bold text-gray-900 mb-4">Google Calendar Required</h1>
        <p className="text-lg text-gray-600 mb-6">
          You must connect your Google Calendar to continue using LiveExpert.AI.
        </p>

        <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-6 mb-6 text-left">
          <div className="flex items-start gap-3 mb-4">
            <AlertCircle className="w-5 h-5 text-yellow-600 flex-shrink-0 mt-0.5" />
            <div>
              <h3 className="font-semibold text-yellow-900 mb-2">Without Google Calendar, you cannot:</h3>
              <ul className="space-y-2">
                {restrictions.map((restriction, idx) => (
                  <li key={idx} className="flex items-center gap-2 text-yellow-800">
                    <span className="text-yellow-600">×</span>
                    {restriction}
                  </li>
                ))}
              </ul>
            </div>
          </div>
        </div>

        <div className="bg-blue-50 border border-blue-200 rounded-lg p-6 mb-6 text-left">
          <div className="flex items-start gap-3">
            <CheckCircle className="w-5 h-5 text-blue-600 flex-shrink-0 mt-0.5" />
            <div>
              <h3 className="font-semibold text-blue-900 mb-2">Why we need Google Calendar:</h3>
              <ul className="space-y-1 text-sm text-blue-800">
                <li>• To schedule and manage your sessions</li>
                <li>• To create Google Meet links automatically</li>
                <li>• To send calendar invites for bookings</li>
                <li>• To sync your availability</li>
              </ul>
            </div>
          </div>
        </div>

        <div className="flex flex-col sm:flex-row gap-4 justify-center">
          <Button size="lg" onClick={handleConnect}>
            <Calendar className="mr-2 w-5 h-5" />
            Connect Google Calendar
          </Button>
          <Button size="lg" variant="outline" onClick={() => navigate('/')}>
            Back to Home
          </Button>
        </div>

        <p className="text-sm text-gray-500 mt-6">
          Your calendar data is encrypted and secure. We only request calendar access for scheduling purposes.
        </p>

        <button
          onClick={() => {
            localStorage.setItem('calendarSkipped', 'true')
            onDismiss?.()
            navigate(-1)
          }}
          className="text-sm text-gray-500 hover:text-gray-700 mt-3 underline"
        >
          Skip for now
        </button>
      </Card>
    </div>
  )
}

export default CalendarBlockingScreen
