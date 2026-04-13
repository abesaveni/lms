import { useState } from 'react'
import { Link } from 'react-router-dom'
import { CheckCircle, AlertCircle } from 'lucide-react'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'
import { Card } from '../../components/ui/Card'
import logoImage from '../../assets/logo.png'

const ForgotPassword = () => {
  const [email, setEmail] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [isSubmitted, setIsSubmitted] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!email) return

    setIsLoading(true)
    setError(null)

    try {
      const { apiPost } = await import('../../services/api')
      const response = await apiPost<{ success: boolean; error?: { message: string } }>('/auth/forgot-password', { email })

      if (response.success) {
        setIsSubmitted(true)
      } else {
        setError(response.error?.message || 'Failed to send reset email')
      }
    } catch (err: any) {
      setError(err.message || 'An error occurred. Please try again.')
    } finally {
      setIsLoading(false)
    }
  }

  if (isSubmitted) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
        <div className="max-w-md w-full text-center">
          <div className="flex justify-center mb-6">
            <CheckCircle className="h-16 w-16 text-green-500" />
          </div>
          <h2 className="text-3xl font-bold text-gray-900 mb-4">Check your email</h2>
          <p className="text-gray-600 mb-8">
            If an account exists for {email}, we've sent a password reset link to your email address.
          </p>
          <Link to="/login">
            <Button fullWidth>Return to login</Button>
          </Link>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md w-full">
        <div className="text-center mb-8">
          <div className="flex items-center justify-center mb-4">
            <img src={logoImage} alt="LiveExpert.AI Logo" className="h-12 w-auto" />
          </div>
          <h2 className="text-3xl font-bold text-gray-900">Forgot password?</h2>
          <p className="mt-2 text-gray-600">Enter your email and we'll send you a reset link</p>
        </div>

        <Card>
          <form onSubmit={handleSubmit} className="space-y-6">
            <Input
              label="Email address"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="you@example.com"
              required
            />

            {error && (
              <div className="flex items-center gap-2 text-sm text-red-600 bg-red-50 p-3 rounded-lg">
                <AlertCircle className="h-4 w-4" />
                <span>{error}</span>
              </div>
            )}

            <Button type="submit" fullWidth isLoading={isLoading}>
              Send Reset Link
            </Button>

            <p className="text-center text-sm text-gray-600">
              Remember your password?{' '}
              <Link to="/login" className="text-primary-600 hover:text-primary-700 font-medium">
                Back to login
              </Link>
            </p>
          </form>
        </Card>
      </div>
    </div>
  )
}

export default ForgotPassword
