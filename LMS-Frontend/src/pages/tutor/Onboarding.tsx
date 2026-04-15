import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Upload, FileText, Check, AlertCircle, User, CheckCircle, ArrowRight, Edit3 } from 'lucide-react'
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'

const TutorOnboarding = () => {
  const navigate = useNavigate()
  const [step, setStep] = useState(1)
  const [profileMode, setProfileMode] = useState<'upload' | 'manual' | 'review'>('upload')
  const [isUploading, setIsUploading] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const [formData, setFormData] = useState({
    skills: '',
    experience: '',
    education: '',
    certifications: '',
    languages: '',
    bio: '',
    govtId: null as File | null,
  })

  const handleResumeUpload = async (file: File) => {
    setIsUploading(true)
    setError(null)
    try {
      const { parseResume } = await import('../../services/tutorApi')
      const parsed = await parseResume(file)

      const hasData = parsed.skills?.length || parsed.bio || parsed.headline ||
        parsed.highestEducation || parsed.educationHistory?.length ||
        parsed.totalExperience || parsed.yearsOfExperience

      if (parsed.notConfigured) {
        setError('Resume parsing is not configured yet. Please fill in your profile manually.')
        setProfileMode('manual')
        return
      }

      if (parsed.parseError && !hasData) {
        setError(`Resume parsing failed: ${parsed.parseError}. Please fill in manually.`)
        setProfileMode('manual')
        return
      }

      setFormData(prev => ({
        ...prev,
        skills: parsed.skills?.join(', ') || '',
        experience: parsed.totalExperience || parsed.yearsOfExperience
          ? `${parsed.totalExperience || parsed.yearsOfExperience} years of experience`
          : '',
        education: parsed.highestEducation || parsed.educationHistory?.[0]?.degree || '',
        certifications: parsed.certifications?.join(', ') || '',
        languages: parsed.languages?.join(', ') || '',
        bio: parsed.bio || parsed.headline || '',
      }))

      if (!hasData) {
        setError('Could not extract data from this resume. Please review and fill in manually.')
      }
      setProfileMode('review')
    } catch (err: any) {
      setError(err.message || 'Failed to parse resume. Please try again or fill in manually.')
      setProfileMode('manual')
    } finally {
      setIsUploading(false)
    }
  }

  const handleNext = () => {
    if (step === 1) {
      setStep(2)
    } else if (step === 2) {
      if (!formData.skills || !formData.experience) {
        setError('Please fill in at least Skills and Experience')
        return
      }
      setError(null)
      setStep(3)
    } else if (step === 3) {
      if (!formData.govtId) {
        setError('Please upload a government-issued ID')
        return
      }
      handleSubmit()
    }
  }

  const handleSubmit = async () => {
    setIsSubmitting(true)
    setError(null)
    try {
      const {
        updateTutorProfile,
        uploadTutorGovtId,
        submitTutorVerification,
      } = await import('../../services/tutorApi')

      const yearsMatch = formData.experience.match(/\d+/)
      const yearsOfExperience = yearsMatch ? parseInt(yearsMatch[0], 10) : 0

      await updateTutorProfile({
        bio: formData.bio,
        skills: formData.skills,
        certifications: formData.certifications,
        education: formData.education,
        languages: formData.languages,
        yearsOfExperience,
      })

      let govtIdUrl = ''
      if (formData.govtId) {
        govtIdUrl = await uploadTutorGovtId(formData.govtId)
      }

      await submitTutorVerification({ govtIdUrl })
      navigate('/tutor/verification-pending')
    } catch (err: any) {
      setError(err.message || 'Failed to submit verification')
    } finally {
      setIsSubmitting(false)
    }
  }

  const stepLabels: Record<number, string> = {
    1: 'Basic Account',
    2: 'Profile',
    3: 'Verification',
  }

  const ProfileField = ({
    label,
    value,
    onChange,
    placeholder,
    multiline,
    required,
  }: {
    label: string
    value: string
    onChange: (v: string) => void
    placeholder?: string
    multiline?: boolean
    required?: boolean
  }) => (
    <div>
      <label className="block text-sm font-medium text-gray-700 mb-1">
        {label}{required && <span className="text-red-500 ml-1">*</span>}
      </label>
      {multiline ? (
        <textarea
          value={value}
          onChange={(e) => onChange(e.target.value)}
          rows={3}
          className="w-full px-4 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500 text-sm"
          placeholder={placeholder}
        />
      ) : (
        <Input value={value} onChange={(e) => onChange(e.target.value)} placeholder={placeholder} />
      )}
    </div>
  )

  return (
    <div className="max-w-2xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Complete Your Tutor Profile</h1>
        <p className="text-gray-600">Follow these steps to become a verified tutor</p>
      </div>

      {error && (
        <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700 flex items-start gap-2">
          <AlertCircle className="w-4 h-4 flex-shrink-0 mt-0.5" />
          {error}
        </div>
      )}

      {/* Progress Indicator */}
      <div className="mb-8">
        <div className="flex items-center justify-between mb-2">
          <span className="text-sm font-medium text-gray-700">Step {step} of 3</span>
          <span className="text-sm text-gray-500">{stepLabels[step]}</span>
        </div>
        <div className="w-full bg-gray-200 rounded-full h-2">
          <div
            className="bg-gradient-primary h-2 rounded-full transition-all duration-300"
            style={{ width: `${(step / 3) * 100}%` }}
          />
        </div>
      </div>

      {/* Step 1: Account Created */}
      {step === 1 && (
        <Card>
          <CardContent className="pt-6 text-center py-12">
            <CheckCircle className="w-16 h-16 mx-auto mb-4 text-green-600" />
            <h2 className="text-2xl font-semibold mb-2">Account Created!</h2>
            <p className="text-gray-600 mb-6">
              Your account is ready. Now let's build your tutor profile.
            </p>
            <Button onClick={() => setStep(2)}>
              Continue to Profile
              <ArrowRight className="ml-2 w-4 h-4" />
            </Button>
          </CardContent>
        </Card>
      )}

      {/* Step 2: Profile — Upload mode */}
      {step === 2 && profileMode === 'upload' && (
        <Card>
          <CardHeader>
            <CardTitle>Build Your Profile from Resume</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-gray-600 mb-6 text-sm">
              Upload your resume and we'll automatically extract your skills, experience, and education.
              Your file is processed in memory and never stored.
            </p>

            <label className="block p-10 border-2 border-dashed border-gray-300 rounded-xl text-center cursor-pointer hover:border-primary-400 hover:bg-primary-50 transition-colors group">
              <input
                type="file"
                accept=".pdf,.doc,.docx"
                onChange={(e) => {
                  const file = e.target.files?.[0]
                  if (file) handleResumeUpload(file)
                }}
                className="hidden"
                disabled={isUploading}
              />
              {isUploading ? (
                <>
                  <div className="w-12 h-12 mx-auto mb-4 rounded-full bg-primary-100 flex items-center justify-center">
                    <Upload className="w-6 h-6 text-primary-600 animate-bounce" />
                  </div>
                  <p className="font-medium text-gray-700">Parsing your resume...</p>
                  <p className="text-sm text-gray-500 mt-1">Extracting skills and experience</p>
                </>
              ) : (
                <>
                  <div className="w-12 h-12 mx-auto mb-4 rounded-full bg-gray-100 group-hover:bg-primary-100 flex items-center justify-center transition-colors">
                    <FileText className="w-6 h-6 text-gray-400 group-hover:text-primary-600 transition-colors" />
                  </div>
                  <p className="font-medium text-gray-700">Click to upload your resume</p>
                  <p className="text-sm text-gray-500 mt-1">PDF, DOC, or DOCX</p>
                </>
              )}
            </label>

            <div className="mt-6 text-center">
              <button
                onClick={() => setProfileMode('manual')}
                className="text-sm text-gray-500 hover:text-primary-600 underline underline-offset-2 transition-colors"
              >
                Fill in manually instead
              </button>
            </div>

            <div className="mt-6">
              <Button variant="outline" onClick={() => setStep(1)} fullWidth>
                Back
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Step 2: Profile — Review parsed data */}
      {step === 2 && profileMode === 'review' && (
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle>Review Your Profile</CardTitle>
              <button
                onClick={() => setProfileMode('upload')}
                className="text-sm text-primary-600 hover:text-primary-700 flex items-center gap-1"
              >
                <Upload className="w-4 h-4" />
                Re-upload
              </button>
            </div>
          </CardHeader>
          <CardContent>
            <div className="flex items-center gap-2 p-3 bg-green-50 border border-green-200 rounded-lg mb-6">
              <CheckCircle className="w-4 h-4 text-green-600 flex-shrink-0" />
              <p className="text-sm text-green-800">
                Resume parsed successfully! Review and edit the details below.
              </p>
            </div>

            <div className="space-y-4">
              <ProfileField
                label="Skills"
                value={formData.skills}
                onChange={(v) => setFormData({ ...formData, skills: v })}
                placeholder="e.g., React, TypeScript, Python"
                required
              />
              <ProfileField
                label="Experience"
                value={formData.experience}
                onChange={(v) => setFormData({ ...formData, experience: v })}
                placeholder="Describe your professional experience..."
                multiline
                required
              />
              <ProfileField
                label="Education"
                value={formData.education}
                onChange={(v) => setFormData({ ...formData, education: v })}
                placeholder="e.g., M.S. Computer Science, MIT"
              />
              <ProfileField
                label="Certifications"
                value={formData.certifications}
                onChange={(v) => setFormData({ ...formData, certifications: v })}
                placeholder="e.g., AWS Certified, Google Cloud Professional"
              />
              <ProfileField
                label="Languages"
                value={formData.languages}
                onChange={(v) => setFormData({ ...formData, languages: v })}
                placeholder="e.g., English, Spanish"
              />
              <ProfileField
                label="Bio"
                value={formData.bio}
                onChange={(v) => setFormData({ ...formData, bio: v })}
                placeholder="Tell students about yourself..."
                multiline
              />
            </div>

            <div className="flex gap-3 mt-6">
              <Button onClick={handleNext} fullWidth disabled={isSubmitting}>
                Continue
                <ArrowRight className="ml-2 w-4 h-4" />
              </Button>
              <Button variant="outline" onClick={() => setStep(1)}>
                Back
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Step 2: Profile — Manual mode */}
      {step === 2 && profileMode === 'manual' && (
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle>Your Tutor Profile</CardTitle>
              <button
                onClick={() => setProfileMode('upload')}
                className="text-sm text-primary-600 hover:text-primary-700 flex items-center gap-1"
              >
                <Edit3 className="w-4 h-4" />
                Use resume instead
              </button>
            </div>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <ProfileField
                label="Skills"
                value={formData.skills}
                onChange={(v) => setFormData({ ...formData, skills: v })}
                placeholder="e.g., React, TypeScript, Python, Machine Learning"
                required
              />
              <ProfileField
                label="Experience"
                value={formData.experience}
                onChange={(v) => setFormData({ ...formData, experience: v })}
                placeholder="Describe your professional experience..."
                multiline
                required
              />
              <ProfileField
                label="Education"
                value={formData.education}
                onChange={(v) => setFormData({ ...formData, education: v })}
                placeholder="e.g., M.S. Computer Science, MIT"
              />
              <ProfileField
                label="Certifications"
                value={formData.certifications}
                onChange={(v) => setFormData({ ...formData, certifications: v })}
                placeholder="e.g., AWS Certified, Google Cloud Professional"
              />
              <ProfileField
                label="Languages"
                value={formData.languages}
                onChange={(v) => setFormData({ ...formData, languages: v })}
                placeholder="e.g., English, Spanish, French"
              />
              <ProfileField
                label="Bio"
                value={formData.bio}
                onChange={(v) => setFormData({ ...formData, bio: v })}
                placeholder="Tell students about yourself..."
                multiline
              />
            </div>

            <div className="flex gap-3 mt-6">
              <Button onClick={handleNext} fullWidth disabled={isSubmitting}>
                Continue
                <ArrowRight className="ml-2 w-4 h-4" />
              </Button>
              <Button variant="outline" onClick={() => setStep(1)}>
                Back
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Step 3: Identity Verification */}
      {step === 3 && (
        <Card>
          <CardHeader>
            <CardTitle>Identity Verification</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-6">
              <div className="p-4 bg-yellow-50 border border-yellow-200 rounded-lg">
                <div className="flex items-start gap-3">
                  <AlertCircle className="w-5 h-5 text-yellow-600 flex-shrink-0 mt-0.5" />
                  <div>
                    <p className="font-semibold text-yellow-900 mb-1">Verification Required</p>
                    <p className="text-sm text-yellow-800">
                      Upload a government-issued ID to verify your identity before you can start teaching.
                    </p>
                  </div>
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Government-Issued ID <span className="text-red-500">*</span>
                </label>
                <label className="block p-6 border-2 border-dashed border-gray-300 rounded-xl text-center cursor-pointer hover:border-primary-400 hover:bg-primary-50 transition-colors">
                  <input
                    type="file"
                    accept=".pdf,.jpg,.jpeg,.png"
                    onChange={(e) => {
                      const file = e.target.files?.[0]
                      if (file) {
                        setFormData({ ...formData, govtId: file })
                        setError(null)
                      }
                    }}
                    className="hidden"
                  />
                  <User className="w-10 h-10 mx-auto mb-3 text-gray-400" />
                  <Button variant="outline" type="button">
                    <Upload className="mr-2 w-4 h-4" />
                    Upload ID Document
                  </Button>
                  <p className="text-sm text-gray-500 mt-2">
                    Accepted: Aadhaar, PAN, Passport, Driving License (PDF or Image)
                  </p>
                </label>

                {formData.govtId && (
                  <div className="mt-3 p-3 bg-green-50 border border-green-200 rounded-lg">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-2">
                        <CheckCircle className="w-4 h-4 text-green-600" />
                        <span className="text-sm font-medium text-gray-900">{formData.govtId.name}</span>
                        <span className="text-xs text-gray-500">
                          ({(formData.govtId.size / 1024).toFixed(0)} KB)
                        </span>
                      </div>
                      <button
                        onClick={() => setFormData({ ...formData, govtId: null })}
                        className="text-xs text-gray-500 hover:text-red-600 transition-colors"
                      >
                        Remove
                      </button>
                    </div>
                  </div>
                )}
              </div>

              <div className="p-4 bg-gray-50 rounded-lg">
                <h4 className="font-semibold text-gray-900 mb-2">What happens next?</h4>
                <ul className="space-y-2 text-sm text-gray-600">
                  <li className="flex items-start gap-2">
                    <Check className="w-4 h-4 text-green-600 flex-shrink-0 mt-0.5" />
                    Your profile will be reviewed by our admin team
                  </li>
                  <li className="flex items-start gap-2">
                    <Check className="w-4 h-4 text-green-600 flex-shrink-0 mt-0.5" />
                    You'll receive an email once verification is complete
                  </li>
                  <li className="flex items-start gap-2">
                    <Check className="w-4 h-4 text-green-600 flex-shrink-0 mt-0.5" />
                    Once verified, you can start creating sessions
                  </li>
                </ul>
              </div>
            </div>

            <div className="flex gap-3 mt-6">
              <Button onClick={handleNext} fullWidth disabled={!formData.govtId || isSubmitting}>
                {isSubmitting ? 'Submitting...' : 'Submit for Verification'}
              </Button>
              <Button variant="outline" onClick={() => setStep(2)}>
                Back
              </Button>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  )
}

export default TutorOnboarding
