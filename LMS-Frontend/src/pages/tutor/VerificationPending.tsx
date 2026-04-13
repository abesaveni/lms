import { Clock, Mail, CheckCircle } from 'lucide-react'
import { Card, CardContent } from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import { useNavigate } from 'react-router-dom'

const VerificationPending = () => {
  const navigate = useNavigate()

  return (
    <div className="max-w-2xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <Card className="text-center">
        <CardContent className="pt-12 pb-12">
          <div className="w-20 h-20 mx-auto mb-6 rounded-full bg-yellow-100 flex items-center justify-center">
            <Clock className="w-10 h-10 text-yellow-600" />
          </div>
          
          <h1 className="text-3xl font-bold text-gray-900 mb-4">Verification Pending</h1>
          <p className="text-lg text-gray-600 mb-8">
            Your tutor profile has been submitted for verification. Our admin team will review your application shortly.
          </p>

          <div className="bg-blue-50 border border-blue-200 rounded-lg p-6 mb-6 text-left">
            <div className="flex items-start gap-3">
              <Mail className="w-5 h-5 text-blue-600 flex-shrink-0 mt-0.5" />
              <div>
                <h3 className="font-semibold text-blue-900 mb-2">What to Expect</h3>
                <ul className="space-y-2 text-sm text-blue-800">
                  <li>• You'll receive an email notification once your verification is complete</li>
                  <li>• Verification typically takes 24-48 hours</li>
                  <li>• Once approved, you can start creating sessions</li>
                  <li>• You'll be visible in the "Find Tutors" section</li>
                </ul>
              </div>
            </div>
          </div>

          <div className="bg-gray-50 rounded-lg p-6 mb-6">
            <h3 className="font-semibold text-gray-900 mb-3">While You Wait</h3>
            <div className="space-y-2 text-sm text-gray-600 text-left">
              <div className="flex items-center gap-2">
                <CheckCircle className="w-4 h-4 text-green-600" />
                <span>Connect your Google Calendar (required)</span>
              </div>
              <div className="flex items-center gap-2">
                <CheckCircle className="w-4 h-4 text-green-600" />
                <span>Review your profile and session pricing</span>
              </div>
              <div className="flex items-center gap-2">
                <CheckCircle className="w-4 h-4 text-green-600" />
                <span>Explore the platform and prepare your content</span>
              </div>
            </div>
          </div>

          <div className="flex gap-3 justify-center">
            <Button onClick={() => navigate('/calendar/connect')}>
              Connect Google Calendar
            </Button>
            <Button variant="outline" onClick={() => navigate('/')}>
              Back to Home
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}

export default VerificationPending
