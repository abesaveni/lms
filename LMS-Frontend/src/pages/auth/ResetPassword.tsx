import { useState, useEffect } from 'react'
import { Link, useSearchParams, useNavigate } from 'react-router-dom'
import { CheckCircle, AlertCircle, Eye, EyeOff } from 'lucide-react'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'
import { Card } from '../../components/ui/Card'
import logoImage from '../../assets/logo.png'

const ResetPassword = () => {
    const [searchParams] = useSearchParams()
    const navigate = useNavigate()
    const token = searchParams.get('token')
    const userId = searchParams.get('userId')

    const [passwordData, setPasswordData] = useState({
        newPassword: '',
        confirmPassword: ''
    })
    const [showPassword, setShowPassword] = useState(false)
    const [isLoading, setIsLoading] = useState(false)
    const [isSuccess, setIsSuccess] = useState(false)
    const [error, setError] = useState<string | null>(null)

    useEffect(() => {
        if (!token || !userId) {
            setError('Missing reset token or user identification.')
        }
    }, [token, userId])

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()
        if (passwordData.newPassword !== passwordData.confirmPassword) {
            setError('Passwords do not match')
            return
        }

        if (passwordData.newPassword.length < 6) {
            setError('Password must be at least 6 characters')
            return
        }

        setIsLoading(true)
        setError(null)

        try {
            const { apiPost } = await import('../../services/api')
            const response = await apiPost<{ success: boolean; error?: { message: string } }>('/auth/reset-password', {
                userId,
                token,
                newPassword: passwordData.newPassword
            })

            if (response.success) {
                setIsSuccess(true)
                setTimeout(() => navigate('/login'), 5000)
            } else {
                setError(response.error?.message || 'Failed to reset password')
            }
        } catch (err: any) {
            setError(err.message || 'An error occurred. Please try again.')
        } finally {
            setIsLoading(false)
        }
    }

    if (isSuccess) {
        return (
            <div className="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
                <div className="max-w-md w-full text-center">
                    <div className="flex justify-center mb-6">
                        <CheckCircle className="h-16 w-16 text-green-500" />
                    </div>
                    <h2 className="text-3xl font-bold text-gray-900 mb-4">Password Reset!</h2>
                    <p className="text-gray-600 mb-8">
                        Your password has been successfully reset. You can now login with your new password.
                        You will be redirected to the login page shortly.
                    </p>
                    <Link to="/login">
                        <Button fullWidth>Go to Login Now</Button>
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
                    <h2 className="text-3xl font-bold text-gray-900">Reset your password</h2>
                    <p className="mt-2 text-gray-600">Please enter and confirm your new password below</p>
                </div>

                <Card>
                    <form onSubmit={handleSubmit} className="space-y-6">
                        {/* New Password */}
                        <div className="relative">
                            <Input
                                label="New Password"
                                type={showPassword ? 'text' : 'password'}
                                value={passwordData.newPassword}
                                onChange={(e) => setPasswordData({ ...passwordData, newPassword: e.target.value })}
                                placeholder="Min 6 characters"
                                required
                                className="pr-10"
                            />
                            <button
                                type="button"
                                onClick={() => setShowPassword(!showPassword)}
                                className="absolute right-3 top-[34px] text-gray-500 hover:text-gray-700"
                            >
                                {showPassword ? <EyeOff className="w-5 h-5" /> : <Eye className="w-5 h-5" />}
                            </button>
                        </div>

                        {/* Confirm Password */}
                        <Input
                            label="Confirm Password"
                            type="password"
                            value={passwordData.confirmPassword}
                            onChange={(e) => setPasswordData({ ...passwordData, confirmPassword: e.target.value })}
                            placeholder="Confirm your new password"
                            required
                        />

                        {error && (
                            <div className="flex items-center gap-2 text-sm text-red-600 bg-red-50 p-3 rounded-lg">
                                <AlertCircle className="h-4 w-4" />
                                <span>{error}</span>
                            </div>
                        )}

                        <Button type="submit" fullWidth isLoading={isLoading} disabled={!!error && !token}>
                            Reset Password
                        </Button>

                        <p className="text-center text-sm text-gray-600">
                            Don't want to reset?{' '}
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

export default ResetPassword
