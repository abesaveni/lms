import { useEffect, useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { Eye, EyeOff, Check, Mail, MessageCircle } from 'lucide-react'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'
import { Card } from '../../components/ui/Card'
import logoImage from '../../assets/logo.png'

const Register = () => {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const [step, setStep] = useState(1)
  const [showPassword, setShowPassword] = useState(false)
  const [verificationCode, setVerificationCode] = useState('')
  const [codeSent, setCodeSent] = useState(false)
  const [codeResendTimer, setCodeResendTimer] = useState(0)
  const [formData, setFormData] = useState({
    role: 'student',
    name: '',
    email: '',
    referralCode: '',
    password: '',
    confirmPassword: '',
    phone: '',
    whatsappNumber: '',
    agreeToTerms: false,
  })
  const [errors, setErrors] = useState<Record<string, string>>({})
  const [isLoading, setIsLoading] = useState(false)
  const [isSendingCode, setIsSendingCode] = useState(false)
  const [resumeFile, setResumeFile] = useState<File | null>(null)
  const [isParsingResume, setIsParsingResume] = useState(false)
  const [resumeParsed, setResumeParsed] = useState(false)
  const [resumeUrl, setResumeUrl] = useState<string>('')
  const isTutorSignup = formData.role === 'tutor'
  const [roleLocked, setRoleLocked] = useState(false)
  const [studentSignupToken, setStudentSignupToken] = useState<string | null>(null)
  const [tutorStep, setTutorStep] = useState(1)
  const [tutorSignupToken, setTutorSignupToken] = useState<string | null>(null)
  const [isTutorSubmitting, setIsTutorSubmitting] = useState(false)
  const [devOtp, setDevOtp] = useState<string | null>(null)
  const [tutorProfile, setTutorProfile] = useState({
    headline: '',
    bio: '',
    skills: '',
    experience: '',
    certifications: '',
    education: '',
    languages: '',
    yearsOfExperience: '',
    linkedInUrl: '',
    gitHubUrl: '',
    portfolioUrl: '',
  })

  useEffect(() => {
    const roleParam = searchParams.get('role')
    const referralParam = searchParams.get('ref')
    if (roleParam === 'student' || roleParam === 'tutor') {
      setFormData((prev) => ({ ...prev, role: roleParam }))
      setRoleLocked(true)
    }
    if (referralParam) {
      setFormData((prev) => ({ ...prev, referralCode: referralParam.toUpperCase() }))
    }
  }, [searchParams])

  const handleStudentSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setErrors({})

    if (step === 1) {
      // Validate step 1
      const newErrors: Record<string, string> = {}
      if (!formData.name) newErrors.name = 'Name is required'
      if (!formData.email) newErrors.email = 'Email is required'
      if (!formData.password) newErrors.password = 'Password is required'
      if (formData.password.length < 8) {
        newErrors.password = 'Password must be at least 8 characters'
      }
      if (formData.password !== formData.confirmPassword) {
        newErrors.confirmPassword = 'Passwords do not match'
      }
      if (!formData.agreeToTerms) {
        newErrors.agreeToTerms = 'You must agree to the terms'
      }

      if (Object.keys(newErrors).length > 0) {
        setErrors(newErrors)
        return
      }

      setStep(2)
    } else if (step === 2) {
      setStep(3)
    } else if (step === 3) {
      if (!codeSent) {
        setIsSendingCode(true)
        try {
          const { apiPost } = await import('../../services/api')

          const requestResponse = await apiPost<{
            success: boolean
            devOtp?: string
            error?: { code: string; message: string }
          }>('/student/request-email-verification', {
            email: formData.email,
            name: formData.name,
          })

          if (!requestResponse.success) {
            throw new Error(requestResponse.error?.message || 'Failed to send verification code')
          }

          if (requestResponse.devOtp) setDevOtp(requestResponse.devOtp)
          setCodeSent(true)
          setCodeResendTimer(60)
          const timer = setInterval(() => {
            setCodeResendTimer((prev) => {
              if (prev <= 1) {
                clearInterval(timer)
                return 0
              }
              return prev - 1
            })
          }, 1000)
        } catch (error: any) {
          setErrors({
            verificationCode: error.message || 'Failed to send verification code. Please try again.',
          })
        } finally {
          setIsSendingCode(false)
        }
        return
      }

      const newErrors: Record<string, string> = {}
      if (!verificationCode) {
        newErrors.verificationCode = 'Verification code is required'
      } else if (verificationCode.length !== 6) {
        newErrors.verificationCode = 'Code must be 6 digits'
      }

      if (Object.keys(newErrors).length > 0) {
        setErrors(newErrors)
        return
      }

      setIsLoading(true)
      try {
        const { apiPost } = await import('../../services/api')
        const verifyResponse = await apiPost<{
          success: boolean
          data?: { verificationToken: string }
          error?: { code: string; message: string }
        }>('/student/confirm-email-verification', {
          email: formData.email,
          code: verificationCode,
        })

        if (!verifyResponse.success || !verifyResponse.data) {
          throw new Error(verifyResponse.error?.message || 'Email verification failed')
        }

        setStudentSignupToken(verifyResponse.data.verificationToken)

        setStep(4)
      } catch (error: any) {
        setErrors({
          verificationCode: error.message || 'Verification failed. Try again.',
        })
      } finally {
        setIsLoading(false)
      }
    } else {
      // Final submission - register then login
      setIsLoading(true)
      try {
        const { apiPost } = await import('../../services/api')
        if (!studentSignupToken) {
          throw new Error('Please verify your email to complete signup')
        }
        const [firstName, ...lastParts] = formData.name.split(' ')
        const lastName = lastParts.join(' ').trim()

        const studentBaseUsername = formData.email.split('@')[0].replace(/[^a-zA-Z0-9_]/g, '_').replace(/_{2,}/g, '_').replace(/^_|_$/g, '').slice(0, 44)
        const studentRandomSuffix = Math.random().toString(36).substring(2, 6)
        const registerResponse = await apiPost<{
          success: boolean
          data?: {
            userId: string
            email: string
            role: string
          }
          error?: { code: string; message: string }
        }>('/student/register', {
          username: `${studentBaseUsername}_${studentRandomSuffix}`,
          email: formData.email,
          password: formData.password,
          phoneNumber: (formData.phone || formData.whatsappNumber).replace(/\s+/g, ''),
          whatsAppNumber: (formData.whatsappNumber || formData.phone).replace(/\s+/g, ''),
          firstName: firstName || formData.name,
          lastName: lastName || firstName || formData.name,
          referralCode: formData.referralCode?.trim() || undefined,
          signupVerificationToken: studentSignupToken,
        })

        if (!registerResponse.success || !registerResponse.data) {
          throw new Error(registerResponse.error?.message || 'Registration failed')
        }

        const loginResponse = await apiPost<{
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

        if (!loginResponse.success || !loginResponse.data) {
          throw new Error(loginResponse.error?.message || 'Login failed after verification')
        }

        localStorage.setItem('token', loginResponse.data.accessToken)
        localStorage.setItem('refreshToken', loginResponse.data.refreshToken)
        localStorage.setItem('user', JSON.stringify(loginResponse.data.user))
        window.dispatchEvent(new Event('tokenUpdated'))

        navigate('/student/dashboard')
      } catch (error: any) {
        setErrors({
          email: error.message || 'An error occurred. Please try again.',
        })
      } finally {
        setIsLoading(false)
      }
    }
  }

  const handleResendCode = () => {
    if (codeResendTimer > 0) return
    setIsSendingCode(true)
      ; (async () => {
        try {
          const { apiPost } = await import('../../services/api')
          const endpoint = isTutorSignup
            ? '/tutor/request-email-verification'
            : '/student/request-email-verification'
          const response = await apiPost<{ success: boolean; devOtp?: string; error?: { message: string } }>(endpoint, {
            email: formData.email,
            name: formData.name,
          })
          if (!response.success) {
            throw new Error(response.error?.message || 'Failed to resend code')
          }
          if (response.devOtp) setDevOtp(response.devOtp)
          setCodeSent(true)
          setCodeResendTimer(60)
          const timer = setInterval(() => {
            setCodeResendTimer((prev) => {
              if (prev <= 1) {
                clearInterval(timer)
                return 0
              }
              return prev - 1
            })
          }, 1000)
        } catch (error: any) {
          setErrors({ email: error.message || 'Failed to resend code' })
        } finally {
          setIsSendingCode(false)
        }
      })()
  }

  const getPasswordStrength = (password: string) => {
    if (password.length === 0) return { strength: 0, label: '' }
    if (password.length < 8) return { strength: 1, label: 'Weak' }
    if (password.length < 12) return { strength: 2, label: 'Medium' }
    return { strength: 3, label: 'Strong' }
  }

  const passwordStrength = getPasswordStrength(formData.password)
  // ── Tutor signup: 3-step flow ─────────────────────────────────────────────
  const tutorSteps = ['Account', 'Verify Email', 'Your Profile']

  const handleTutorNext = async () => {
    setErrors({})

    // ── Step 1 → send OTP + go to step 2 ───────────────────────────────────
    if (tutorStep === 1) {
      const newErrors: Record<string, string> = {}
      if (!formData.name) newErrors.name = 'Full name is required'
      if (!formData.email) newErrors.email = 'Email is required'
      if (!formData.password) newErrors.password = 'Password is required'
      if (formData.password.length < 8) newErrors.password = 'Password must be at least 8 characters'
      if (formData.password !== formData.confirmPassword) newErrors.confirmPassword = 'Passwords do not match'
      if (!formData.agreeToTerms) newErrors.agreeToTerms = 'You must agree to the terms'
      if (Object.keys(newErrors).length > 0) { setErrors(newErrors); return }

      setIsTutorSubmitting(true)
      try {
        const { apiPost } = await import('../../services/api')
        const response = await apiPost<{ success: boolean; devOtp?: string; error?: { message: string } }>(
          '/tutor/request-email-verification',
          { email: formData.email, name: formData.name }
        )
        if (!response.success) throw new Error(response.error?.message || 'Failed to send verification code')
        if (response.devOtp) setDevOtp(response.devOtp)
        setCodeSent(true)
        setCodeResendTimer(60)
        const timer = setInterval(() => {
          setCodeResendTimer((prev) => { if (prev <= 1) { clearInterval(timer); return 0 } return prev - 1 })
        }, 1000)
        setTutorStep(2)
      } catch (error: any) {
        setErrors({ email: error.message || 'Failed to send verification code' })
      } finally {
        setIsTutorSubmitting(false)
      }
      return
    }

    // ── Step 2 → verify OTP + go to step 3 ─────────────────────────────────
    if (tutorStep === 2) {
      if (!verificationCode || verificationCode.length !== 6) {
        setErrors({ verificationCode: 'Enter the 6-digit code sent to your email' })
        return
      }
      setIsTutorSubmitting(true)
      try {
        const { apiPost } = await import('../../services/api')
        const verifyResponse = await apiPost<{
          success: boolean
          data?: { verificationToken: string }
          error?: { code: string; message: string }
        }>('/tutor/confirm-email-verification', { email: formData.email, code: verificationCode })
        if (!verifyResponse.success || !verifyResponse.data) {
          throw new Error(verifyResponse.error?.message || 'Email verification failed')
        }
        setTutorSignupToken(verifyResponse.data.verificationToken)
        setTutorStep(3)
      } catch (error: any) {
        setErrors({ verificationCode: error.message || 'Verification failed. Try again.' })
      } finally {
        setIsTutorSubmitting(false)
      }
      return
    }

    // ── Step 3 → register + login + update profile + submit ─────────────────
    if (tutorStep === 3) {
      const newErrors: Record<string, string> = {}
      if (!tutorProfile.headline) newErrors.headline = 'Profession / Category is required'
      if (!tutorProfile.skills) newErrors.skills = 'Skills are required'
      if (!tutorProfile.bio) newErrors.bio = 'Please tell students about yourself'
      if (!tutorProfile.experience) newErrors.experience = 'Please describe your experience'
      if (Object.keys(newErrors).length > 0) { setErrors(newErrors); return }
      if (!tutorSignupToken) { setErrors({ submitError: 'Session expired — please go back and verify your email again.' }); return }

      setIsTutorSubmitting(true)
      try {
        const { apiPost } = await import('../../services/api')
        const [firstName, ...lastParts] = formData.name.split(' ')
        const lastName = lastParts.join(' ').trim()

        // 1. Register
        const baseUsername = formData.email.split('@')[0].replace(/[^a-zA-Z0-9_]/g, '_').replace(/_{2,}/g, '_').replace(/^_|_$/g, '').slice(0, 44)
        const randomSuffix = Math.random().toString(36).substring(2, 6)
        const registerResponse = await apiPost<{
          success: boolean
          data?: { userId: string; email: string; role: string }
          error?: { code: string; message: string }
        }>('/tutor/register', {
          username: `${baseUsername}_${randomSuffix}`,
          email: formData.email,
          password: formData.password,
          phoneNumber: (formData.phone || formData.whatsappNumber || '').replace(/\s+/g, '') || undefined,
          whatsAppNumber: (formData.whatsappNumber || formData.phone || '').replace(/\s+/g, '') || undefined,
          firstName: firstName || formData.name,
          lastName: lastName || firstName || formData.name,
          signupVerificationToken: tutorSignupToken,
        })
        if (!registerResponse.success) throw new Error(registerResponse.error?.message || 'Registration failed')

        // 2. Login
        const loginResponse = await apiPost<{
          success: boolean
          data?: { accessToken: string; refreshToken: string; user: { id: string; username: string; email: string; role: string; profileImage?: string } }
          error?: { code: string; message: string }
        }>('/auth/login', { email: formData.email, password: formData.password })
        if (!loginResponse.success || !loginResponse.data) throw new Error(loginResponse.error?.message || 'Login failed')

        localStorage.setItem('token', loginResponse.data.accessToken)
        localStorage.setItem('refreshToken', loginResponse.data.refreshToken)
        localStorage.setItem('user', JSON.stringify(loginResponse.data.user))
        window.dispatchEvent(new Event('tokenUpdated'))

        // 3. Update profile (includes resume URL if file was parsed/uploaded)
        const { updateTutorProfile, submitTutorVerification } = await import('../../services/tutorApi')
        await updateTutorProfile({
          headline: tutorProfile.headline,
          bio: tutorProfile.experience
            ? `${tutorProfile.bio}\n\nExperience:\n${tutorProfile.experience}`
            : tutorProfile.bio,
          skills: tutorProfile.skills,
          certifications: tutorProfile.certifications || undefined,
          education: tutorProfile.education || undefined,
          languages: tutorProfile.languages || undefined,
          yearsOfExperience: tutorProfile.yearsOfExperience ? parseInt(tutorProfile.yearsOfExperience, 10) : undefined,
          linkedInUrl: tutorProfile.linkedInUrl || undefined,
          gitHubUrl: tutorProfile.gitHubUrl || undefined,
          portfolioUrl: tutorProfile.portfolioUrl || undefined,
          resumeUrl: resumeUrl || undefined,
        })

        // 4. Submit for admin verification
        await submitTutorVerification({})
        navigate('/tutor/profile-settings')
      } catch (error: any) {
        setErrors({ submitError: error.message || 'Failed to create account. Please try again.' })
      } finally {
        setIsTutorSubmitting(false)
      }
    }
  }

  // Auto-parse resume when a file is selected
  const handleResumeChange = async (file: File | null) => {
    setResumeFile(file)
    setResumeParsed(false)
    setResumeUrl('')
    if (!file) return

    setIsParsingResume(true)
    setErrors((prev) => { const n = { ...prev }; delete n.resume; return n })
    try {
      const { parseResume } = await import('../../services/tutorApi')
      const parsed = await parseResume(file)
      if (parsed.resumeUrl) setResumeUrl(parsed.resumeUrl)
      setTutorProfile((prev) => ({
        ...prev,
        headline: parsed.headline || parsed.title || prev.headline,
        bio: parsed.bio || prev.bio,
        skills: parsed.skills?.join(', ') || prev.skills,
        experience: parsed.experience || parsed.summary || prev.experience,
        certifications: parsed.certifications?.join(', ') || prev.certifications,
        education: parsed.highestEducation || parsed.educationHistory?.[0]?.degree || prev.education,
        languages: parsed.languages?.join(', ') || prev.languages,
        yearsOfExperience: parsed.totalExperience?.toString() || parsed.yearsOfExperience?.toString() || prev.yearsOfExperience,
      }))
      setResumeParsed(true)
    } catch {
      // Non-blocking: user can still fill fields manually
      setErrors((prev) => ({ ...prev, resume: 'Could not parse resume — please fill the fields below manually.' }))
    } finally {
      setIsParsingResume(false)
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
          <h2 className="text-3xl font-bold text-gray-900">
            {isTutorSignup ? 'Create your tutor account' : 'Create your account'}
          </h2>
          <p className="mt-2 text-gray-600">
            {isTutorSignup
              ? 'Build your tutor profile, upload your resume, and start teaching.'
              : 'Start your learning journey today'}
          </p>
        </div>

        <Card>
          {!roleLocked && (
            <div className="mb-6">
              <div className="flex items-center gap-2 rounded-lg bg-gray-100 p-1">
                <button
                  type="button"
                  onClick={() => {
                    setFormData((prev) => ({ ...prev, role: 'student' }))
                    setStep(1)
                    setErrors({})
                  }}
                  className={`flex-1 rounded-md px-4 py-2 text-sm font-medium transition-colors ${!isTutorSignup ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-600'
                    }`}
                >
                  Student
                </button>
                <button
                  type="button"
                  onClick={() => {
                    setFormData((prev) => ({ ...prev, role: 'tutor' }))
                    setStep(1)
                    setErrors({})
                  }}
                  className={`flex-1 rounded-md px-4 py-2 text-sm font-medium transition-colors ${isTutorSignup ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-600'
                    }`}
                >
                  Tutor
                </button>
              </div>
            </div>
          )}

          {isTutorSignup ? (
            <div className="space-y-6">
              {/* Tutor Stepper */}
              <div>
                <div className="flex items-center justify-between mb-2">
                  <span className="text-sm font-medium text-gray-700">
                    Step {tutorStep} of {tutorSteps.length}
                  </span>
                  <span className="text-sm text-gray-500">{tutorSteps[tutorStep - 1]}</span>
                </div>
                <div className="w-full bg-gray-200 rounded-full h-2">
                  <div
                    className="bg-gradient-primary h-2 rounded-full transition-all duration-300"
                    style={{ width: `${(tutorStep / tutorSteps.length) * 100}%` }}
                  />
                </div>
              </div>

              {tutorStep === 1 && (
                <form
                  onSubmit={(e) => {
                    e.preventDefault()
                    handleTutorNext()
                  }}
                  className="space-y-5"
                >
                  <Input
                    label="Full name"
                    type="text"
                    value={formData.name}
                    onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                    error={errors.name}
                    placeholder="John Doe"
                    required
                  />
                  <Input
                    label="Email address"
                    type="email"
                    value={formData.email}
                    onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                    error={errors.email}
                    placeholder="you@example.com"
                    required
                  />
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
                        placeholder="At least 8 characters"
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
                  <Input
                    label="Confirm password"
                    type="password"
                    value={formData.confirmPassword}
                    onChange={(e) => setFormData({ ...formData, confirmPassword: e.target.value })}
                    error={errors.confirmPassword}
                    placeholder="Re-enter your password"
                    required
                  />
                  <div>
                    <label className="flex items-start">
                      <input
                        type="checkbox"
                        checked={formData.agreeToTerms}
                        onChange={(e) => setFormData({ ...formData, agreeToTerms: e.target.checked })}
                        className="mt-1 w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
                      />
                      <span className="ml-2 text-sm text-gray-700">
                        I agree to the{' '}
                        <Link to="/terms-and-conditions" target="_blank" className="text-primary-600 hover:text-primary-700 underline">
                          Terms and Conditions
                        </Link>{' '}
                        and{' '}
                        <Link to="/privacy-policy" target="_blank" className="text-primary-600 hover:text-primary-700 underline">
                          Privacy Policy
                        </Link>
                      </span>
                    </label>
                    {errors.agreeToTerms && (
                      <p className="mt-1.5 text-sm text-red-600">{errors.agreeToTerms}</p>
                    )}
                  </div>
                  <Button type="submit" fullWidth isLoading={isTutorSubmitting}>
                    Create Tutor Account
                  </Button>
                </form>
              )}

              {tutorStep === 2 && (
                <div className="space-y-4">
                  <div className="text-center mb-2">
                    <div className="w-14 h-14 mx-auto mb-3 rounded-full bg-primary-100 flex items-center justify-center">
                      <Mail className="w-7 h-7 text-primary-600" />
                    </div>
                    <h3 className="text-lg font-semibold mb-1">Verify Your Email</h3>
                    <p className="text-sm text-gray-600">
                      We sent a 6-digit code to{' '}
                      <span className="font-semibold text-gray-900">{formData.email}</span>
                    </p>
                  </div>

                  {devOtp && (
                    <div className="rounded-xl border-2 border-primary-400 bg-primary-50 p-4 text-center">
                      <p className="text-xs font-semibold text-primary-700 uppercase tracking-wide mb-1">Your verification code</p>
                      <p className="font-mono font-bold text-3xl tracking-[0.4em] text-primary-900 select-all">{devOtp}</p>
                      <p className="text-xs text-primary-600 mt-1">Enter this code below (also sent to your email)</p>
                    </div>
                  )}

                  <Input
                    label="Verification code"
                    type="text"
                    value={verificationCode}
                    onChange={(e) => {
                      const value = e.target.value.replace(/\D/g, '').slice(0, 6)
                      setVerificationCode(value)
                    }}
                    error={errors.verificationCode}
                    placeholder="000000"
                    className="text-center text-2xl font-mono tracking-widest"
                    maxLength={6}
                    autoFocus
                  />

                  <div className="flex items-center justify-between text-sm">
                    <span className="text-gray-600">Didn't receive it?</span>
                    <button
                      type="button"
                      onClick={handleResendCode}
                      disabled={codeResendTimer > 0 || isSendingCode}
                      className="text-primary-600 hover:text-primary-700 font-medium disabled:text-gray-400 disabled:cursor-not-allowed"
                    >
                      {isSendingCode ? 'Sending...' : codeResendTimer > 0 ? `Resend in ${codeResendTimer}s` : 'Resend Code'}
                    </button>
                  </div>

                  <Button fullWidth onClick={handleTutorNext} isLoading={isTutorSubmitting}>
                    Verify & Continue
                    <Check className="ml-2 w-5 h-5" />
                  </Button>

                  <Button
                    type="button"
                    variant="ghost"
                    fullWidth
                    onClick={() => {
                      setTutorStep(1)
                      setVerificationCode('')
                      setErrors({})
                    }}
                  >
                    Back — Change Email or Password
                  </Button>
                </div>
              )}

              {tutorStep === 3 && (
                <div className="space-y-5">
                  {/* Resume upload — auto-parses on select */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Resume <span className="text-gray-400 font-normal">(optional — auto-fills your profile)</span>
                    </label>
                    <label
                      className={`flex flex-col items-center justify-center w-full border-2 border-dashed rounded-lg p-5 cursor-pointer transition-colors ${
                        resumeFile
                          ? 'border-primary-400 bg-primary-50'
                          : 'border-gray-300 bg-gray-50 hover:border-primary-400 hover:bg-primary-50'
                      }`}
                    >
                      <input
                        type="file"
                        accept=".pdf,.doc,.docx,.txt"
                        className="hidden"
                        onChange={(e) => handleResumeChange(e.target.files?.[0] || null)}
                      />
                      {isParsingResume ? (
                        <div className="flex flex-col items-center gap-2 text-primary-600">
                          <svg className="animate-spin h-7 w-7" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                          </svg>
                          <span className="text-sm font-medium">Parsing resume…</span>
                        </div>
                      ) : resumeFile ? (
                        <div className="flex flex-col items-center gap-1 text-center">
                          <svg className="w-8 h-8 text-primary-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                          </svg>
                          <span className="text-sm font-medium text-gray-700">{resumeFile.name}</span>
                          <span className="text-xs text-gray-500">Click to replace</span>
                        </div>
                      ) : (
                        <div className="flex flex-col items-center gap-1 text-center">
                          <svg className="w-8 h-8 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
                          </svg>
                          <span className="text-sm font-medium text-gray-600">Drop your resume here or click to browse</span>
                          <span className="text-xs text-gray-400">PDF, DOC, DOCX, TXT</span>
                        </div>
                      )}
                    </label>
                    {resumeParsed && (
                      <p className="mt-1.5 text-sm text-green-700 font-medium flex items-center gap-1">
                        <Check className="w-4 h-4" /> Resume parsed — fields pre-filled below. Review and adjust as needed.
                      </p>
                    )}
                    {errors.resume && <p className="mt-1.5 text-sm text-red-600">{errors.resume}</p>}
                  </div>

                  <Input
                    label="Profession / Category *"
                    value={tutorProfile.headline}
                    onChange={(e) => setTutorProfile({ ...tutorProfile, headline: e.target.value })}
                    error={errors.headline}
                    placeholder="e.g., Vocal Coach, Fitness Trainer, UI/UX Designer"
                  />

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1.5">
                      About You / Bio *
                    </label>
                    <textarea
                      value={tutorProfile.bio}
                      onChange={(e) => setTutorProfile({ ...tutorProfile, bio: e.target.value })}
                      rows={3}
                      className="w-full px-4 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500 text-sm"
                      placeholder="Tell students about your teaching style and background."
                    />
                    {errors.bio && <p className="mt-1 text-sm text-red-600">{errors.bio}</p>}
                  </div>

                  <Input
                    label="Skills * (comma-separated)"
                    value={tutorProfile.skills}
                    onChange={(e) => setTutorProfile({ ...tutorProfile, skills: e.target.value })}
                    error={errors.skills}
                    placeholder="e.g., React, Node.js, TypeScript"
                  />

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1.5">
                      Professional Experience *
                    </label>
                    <textarea
                      value={tutorProfile.experience}
                      onChange={(e) => setTutorProfile({ ...tutorProfile, experience: e.target.value })}
                      rows={3}
                      className="w-full px-4 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500 text-sm"
                      placeholder="Describe your work history and professional achievements."
                    />
                    {errors.experience && <p className="mt-1 text-sm text-red-600">{errors.experience}</p>}
                  </div>

                  <Input
                    label="Years of Experience"
                    type="number"
                    value={tutorProfile.yearsOfExperience}
                    onChange={(e) => setTutorProfile({ ...tutorProfile, yearsOfExperience: e.target.value })}
                    placeholder="e.g., 5"
                    min={0}
                    max={50}
                  />

                  <Input
                    label="Highest Education"
                    value={tutorProfile.education}
                    onChange={(e) => setTutorProfile({ ...tutorProfile, education: e.target.value })}
                    placeholder="e.g., B.Sc. Computer Science — MIT (Optional)"
                  />

                  <Input
                    label="Certifications"
                    value={tutorProfile.certifications}
                    onChange={(e) => setTutorProfile({ ...tutorProfile, certifications: e.target.value })}
                    placeholder="e.g., AWS Certified, PMP (Optional, comma-separated)"
                  />

                  <Input
                    label="Languages"
                    value={tutorProfile.languages}
                    onChange={(e) => setTutorProfile({ ...tutorProfile, languages: e.target.value })}
                    placeholder="e.g., English, Hindi (Optional)"
                  />

                  <Input
                    label="LinkedIn URL"
                    type="url"
                    value={tutorProfile.linkedInUrl}
                    onChange={(e) => setTutorProfile({ ...tutorProfile, linkedInUrl: e.target.value })}
                    placeholder="https://linkedin.com/in/yourprofile (Optional)"
                  />

                  <Input
                    label="GitHub URL"
                    type="url"
                    value={tutorProfile.gitHubUrl}
                    onChange={(e) => setTutorProfile({ ...tutorProfile, gitHubUrl: e.target.value })}
                    placeholder="https://github.com/yourusername (Optional)"
                  />

                  <Input
                    label="Portfolio URL"
                    type="url"
                    value={tutorProfile.portfolioUrl}
                    onChange={(e) => setTutorProfile({ ...tutorProfile, portfolioUrl: e.target.value })}
                    placeholder="https://yourportfolio.com (Optional)"
                  />

                  {errors.submitError && (
                    <div className="rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700">
                      {errors.submitError}
                    </div>
                  )}

                  <Button fullWidth onClick={handleTutorNext} isLoading={isTutorSubmitting}>
                    {isTutorSubmitting ? 'Creating your account…' : 'Create Account & Submit for Review'}
                  </Button>

                  <Button
                    type="button"
                    variant="ghost"
                    fullWidth
                    onClick={() => {
                      setTutorStep(1)
                      setErrors({})
                    }}
                  >
                    Back — Change Name or Email
                  </Button>

                  <p className="text-xs text-gray-500 text-center">
                    Your profile will be reviewed by our team. You'll receive an email once approved.
                  </p>
                </div>
              )}
            </div>
          ) : (
            <>
              {/* Progress Indicator */}
              <div className="mb-6">
                <div className="flex items-center justify-between mb-2">
                  <span className="text-sm font-medium text-gray-700">Step {step} of 4</span>
                  <span className="text-sm text-gray-500">
                    {step === 1 ? 'Basic Info' : step === 2 ? 'Profile' : step === 3 ? 'Verify Email' : 'Complete'}
                  </span>
                </div>
                <div className="w-full bg-gray-200 rounded-full h-2">
                  <div
                    className="bg-gradient-primary h-2 rounded-full transition-all duration-300"
                    style={{ width: `${(step / 4) * 100}%` }}
                  />
                </div>
              </div>

              <form onSubmit={handleStudentSubmit} className="space-y-6">
                {step === 1 ? (
                  <>
                    {/* Name */}
                    <Input
                      label="Full name"
                      type="text"
                      value={formData.name}
                      onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                      error={errors.name}
                      placeholder="John Doe"
                      required
                    />

                    {/* Email */}
                    <Input
                      label="Email address"
                      type="email"
                      value={formData.email}
                      onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                      error={errors.email}
                      placeholder="you@example.com"
                      required
                    />

                    {/* Referral Code (Optional) */}
                    <Input
                      label="Referral code (optional)"
                      type="text"
                      value={formData.referralCode}
                      onChange={(e) =>
                        setFormData({ ...formData, referralCode: e.target.value.toUpperCase() })
                      }
                      placeholder="ABC123"
                    />

                    {/* Phone (Optional) */}
                    <Input
                      label="Phone number (optional)"
                      type="tel"
                      value={formData.phone}
                      onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
                      placeholder="+1 (555) 000-0000"
                    />

                    {/* WhatsApp Number */}
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        WhatsApp Number <span className="text-gray-500">(for session reminders)</span>
                      </label>
                      <div className="relative">
                        <MessageCircle className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
                        <Input
                          type="tel"
                          value={formData.whatsappNumber}
                          onChange={(e) => setFormData({ ...formData, whatsappNumber: e.target.value })}
                          placeholder="+91 98765 43210"
                          className="pl-10"
                          required
                        />
                      </div>
                      <p className="mt-1.5 text-xs text-gray-500">
                        We'll send session reminders and welcome messages to this number
                      </p>
                    </div>

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
                          placeholder="At least 8 characters"
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
                      {formData.password && (
                        <div className="mt-2">
                          <div className="flex gap-1 mb-1">
                            {[1, 2, 3].map((level) => (
                              <div
                                key={level}
                                className={`h-1 flex-1 rounded ${level <= passwordStrength.strength
                                  ? passwordStrength.strength === 1
                                    ? 'bg-red-500'
                                    : passwordStrength.strength === 2
                                      ? 'bg-yellow-500'
                                      : 'bg-green-500'
                                  : 'bg-gray-200'
                                  }`}
                              />
                            ))}
                          </div>
                          <p className="text-xs text-gray-600">{passwordStrength.label}</p>
                        </div>
                      )}
                    </div>

                    {/* Confirm Password */}
                    <Input
                      label="Confirm password"
                      type="password"
                      value={formData.confirmPassword}
                      onChange={(e) => setFormData({ ...formData, confirmPassword: e.target.value })}
                      error={errors.confirmPassword}
                      placeholder="Re-enter your password"
                      required
                    />

                    {/* Terms & Conditions */}
                    <div>
                      <label className="flex items-start">
                        <input
                          type="checkbox"
                          checked={formData.agreeToTerms}
                          onChange={(e) => setFormData({ ...formData, agreeToTerms: e.target.checked })}
                          className="mt-1 w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
                        />
                        <span className="ml-2 text-sm text-gray-700">
                          I agree to the{' '}
                          <Link to="/terms-and-conditions" target="_blank" className="text-primary-600 hover:text-primary-700 underline">
                            Terms and Conditions
                          </Link>{' '}
                          and{' '}
                          <Link to="/privacy-policy" target="_blank" className="text-primary-600 hover:text-primary-700 underline">
                            Privacy Policy
                          </Link>
                        </span>
                      </label>
                      {errors.agreeToTerms && (
                        <p className="mt-1.5 text-sm text-red-600">{errors.agreeToTerms}</p>
                      )}
                    </div>

                    <Button type="submit" fullWidth isLoading={isSendingCode}>
                      {isSendingCode
                        ? 'Sending Code...'
                        : 'Continue'}
                      <Mail className="ml-2 w-5 h-5" />
                    </Button>
                  </>
                ) : step === 2 ? (
                  <>
                    <div className="text-center py-8">
                      <div className="w-16 h-16 mx-auto mb-4 rounded-full bg-gradient-primary flex items-center justify-center">
                        <Check className="w-8 h-8 text-white" />
                      </div>
                      <h3 className="text-xl font-semibold mb-2">Almost there</h3>
                      <p className="text-gray-600 mb-6">
                        Next, verify your email to complete registration.
                      </p>
                    </div>

                    <Button type="submit" fullWidth isLoading={isLoading}>
                      Continue to Email Verification
                    </Button>

                    <Button
                      type="button"
                      variant="ghost"
                      fullWidth
                      onClick={() => setStep(1)}
                    >
                      Go back
                    </Button>
                  </>
                ) : step === 3 ? (
                  <>
                    {/* Email Verification Step */}
                    <div className="text-center mb-6">
                      <div className="w-16 h-16 mx-auto mb-4 rounded-full bg-primary-100 flex items-center justify-center">
                        <Mail className="w-8 h-8 text-primary-600" />
                      </div>
                      <h3 className="text-xl font-semibold mb-2">Verify Your Email</h3>
                      {codeSent ? (
                        <>
                          <p className="text-gray-600 mb-1">We sent a 6-digit code to</p>
                          <p className="font-semibold text-gray-900">{formData.email}</p>
                        </>
                      ) : (
                        <>
                          <p className="text-gray-600 mb-1">We'll send a verification code to</p>
                          <p className="font-semibold text-gray-900">{formData.email}</p>
                        </>
                      )}
                    </div>

                    {codeSent ? (
                      <div className="space-y-4">
                        {devOtp && (
                          <div className="rounded-xl border-2 border-amber-400 bg-amber-50 p-4 text-center">
                            <p className="text-xs font-semibold text-amber-700 uppercase tracking-wide mb-1">⚠️ Email not working — use this OTP</p>
                            <p className="font-mono font-bold text-3xl tracking-[0.4em] text-amber-900 select-all">{devOtp}</p>
                            <p className="text-xs text-amber-600 mt-1">Tap the code to select, then paste it below</p>
                          </div>
                        )}
                        <div>
                          <label className="block text-sm font-medium text-gray-700 mb-2">
                            Enter Verification Code
                          </label>
                          <Input
                            type="text"
                            value={verificationCode}
                            onChange={(e) => {
                              const value = e.target.value.replace(/\D/g, '').slice(0, 6)
                              setVerificationCode(value)
                            }}
                            error={errors.verificationCode}
                            placeholder="000000"
                            className="text-center text-2xl font-mono tracking-widest"
                            maxLength={6}
                            autoFocus
                          />
                          <div className="mt-3 flex items-center justify-between text-sm">
                            <span className="text-gray-600">Didn't receive code?</span>
                            <button
                              type="button"
                              onClick={handleResendCode}
                              disabled={codeResendTimer > 0 || isSendingCode}
                              className="text-primary-600 hover:text-primary-700 font-medium disabled:text-gray-400 disabled:cursor-not-allowed"
                            >
                              {isSendingCode ? 'Sending...' : codeResendTimer > 0 ? `Resend in ${codeResendTimer}s` : 'Resend Code'}
                            </button>
                          </div>
                        </div>
                        <Button type="submit" fullWidth isLoading={isLoading}>
                          Verify Email
                          <Check className="ml-2 w-5 h-5" />
                        </Button>
                      </div>
                    ) : (
                      <div className="space-y-4">
                        {errors.verificationCode && (
                          <div className="rounded-lg bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-700">
                            {errors.verificationCode}
                          </div>
                        )}
                        {errors.email && (
                          <div className="rounded-lg bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-700">
                            {errors.email}
                          </div>
                        )}
                        <Button type="submit" fullWidth isLoading={isSendingCode}>
                          {isSendingCode ? 'Sending...' : 'Send Verification Code'}
                          <Mail className="ml-2 w-5 h-5" />
                        </Button>
                      </div>
                    )}

                    <Button
                      type="button"
                      variant="ghost"
                      fullWidth
                      onClick={() => setStep(1)}
                    >
                      Change Email
                    </Button>
                  </>
                ) : (
                  <>
                    <div className="text-center py-8">
                      <div className="w-16 h-16 mx-auto mb-4 rounded-full bg-gradient-primary flex items-center justify-center">
                        <Check className="w-8 h-8 text-white" />
                      </div>
                      <h3 className="text-xl font-semibold mb-2">
                        {formData.role === 'tutor' ? 'Profile Complete!' : 'Registration Complete!'}
                      </h3>
                      <p className="text-gray-600 mb-6">
                        {formData.role === 'tutor'
                          ? 'Your tutor profile is ready. Continue to your dashboard.'
                          : 'Your account is ready. Continue to your dashboard.'}
                      </p>
                    </div>

                    {errors.email && (
                      <div className="rounded-lg bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-700">
                        {errors.email}
                      </div>
                    )}

                    <Button type="submit" fullWidth isLoading={isLoading}>
                      Continue to Dashboard
                    </Button>
                  </>
                )}


                {/* Sign In Link */}
                <p className="text-center text-sm text-gray-600">
                  Already have an account?{' '}
                  <Link to="/login" className="text-primary-600 hover:text-primary-700 font-medium">
                    Sign in
                  </Link>
                </p>
              </form>
            </>
          )}
        </Card>
      </div>
    </div>
  )
}

export default Register
