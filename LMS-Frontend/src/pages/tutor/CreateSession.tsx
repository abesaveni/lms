import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { Video, Zap, Tag, Lock, Info } from 'lucide-react'
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'
import { createSession, getSubjects, TeacherSubjectDto } from '../../services/sessionsApi'

const CreateSession = () => {
  const navigate = useNavigate()
  const [subjects, setSubjects] = useState<TeacherSubjectDto[]>([])
  const [isLoadingSubjects, setIsLoadingSubjects] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [submitError, setSubmitError] = useState<string | null>(null)

  const [formData, setFormData] = useState({
    title: '',
    description: '',
    type: 'OneOnOne' as 'OneOnOne' | 'Group',
    subjectId: '',
    date: '',
    time: '',
    duration: '60',
    maxStudents: 1,
    pricingType: 'Fixed' as 'Fixed' | 'Hourly',
    basePrice: 45,
    // Advanced options
    requiresSubscription: false,
    instantBooking: false,
    noShowProtection: false,
    enableFlashSale: false,
    flashSalePrice: 0,
    flashSaleEndsAt: '',
  })

  useEffect(() => {
    const fetchCreateData = async () => {
      try {
        const subjectsData = await getSubjects()
        setSubjects(subjectsData)
        if (subjectsData.length > 0) {
          setFormData(prev => ({ ...prev, subjectId: subjectsData[0].id }))
        }
      } catch (err) {
        console.error('Failed to fetch subjects', err)
      } finally {
        setIsLoadingSubjects(false)
      }
    }
    fetchCreateData()
  }, [])

  const set = (field: string, value: any) => setFormData(prev => ({ ...prev, [field]: value }))

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setIsSubmitting(true)
    setSubmitError(null)
    try {
      const scheduledAt = new Date(`${formData.date}T${formData.time}:00`).toISOString()

      await createSession({
        title: formData.title,
        description: formData.description,
        sessionType: formData.type,
        subjectId: formData.subjectId,
        scheduledAt,
        duration: parseInt(formData.duration),
        basePrice: formData.basePrice,
        maxStudents: formData.type === 'OneOnOne' ? 1 : formData.maxStudents,
        pricingType: formData.pricingType,
        requiresSubscription: formData.requiresSubscription,
        instantBooking: formData.instantBooking,
        noShowProtection: formData.noShowProtection,
        flashSalePrice: formData.enableFlashSale && formData.flashSalePrice > 0 ? formData.flashSalePrice : undefined,
        flashSaleEndsAt: formData.enableFlashSale && formData.flashSaleEndsAt ? new Date(formData.flashSaleEndsAt).toISOString() : undefined,
      })

      navigate('/tutor/sessions')
    } catch (err: any) {
      setSubmitError(err.message || 'Failed to create session. Please check your data.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Create New Session</h1>
        <p className="text-gray-600">Schedule a teaching session with Google Meet integration</p>
      </div>

      {submitError && (
        <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-xl text-red-700 text-sm">{submitError}</div>
      )}

      <form onSubmit={handleSubmit} className="space-y-6">

        {/* Basic Details */}
        <Card>
          <CardHeader><CardTitle>Session Details</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <Input label="Session Title" value={formData.title}
              onChange={(e) => set('title', e.target.value)}
              placeholder="e.g., React Fundamentals Workshop" required />

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Description</label>
              <textarea value={formData.description}
                onChange={(e) => set('description', e.target.value)}
                rows={4}
                className="w-full px-4 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                placeholder="What will students learn in this session?" required />
            </div>

            <div className="grid md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Session Type</label>
                <select value={formData.type}
                  onChange={(e) => set('type', e.target.value)}
                  className="w-full px-4 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-indigo-500">
                  <option value="OneOnOne">1-on-1 Session</option>
                  <option value="Group">Group Session</option>
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Subject</label>
                <select value={formData.subjectId}
                  onChange={(e) => set('subjectId', e.target.value)}
                  className="w-full px-4 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-indigo-500 bg-white" required>
                  {isLoadingSubjects
                    ? <option>Loading…</option>
                    : subjects.length > 0
                      ? subjects.map(s => <option key={s.id} value={s.id}>{s.name}</option>)
                      : <option value="">No Subjects Available</option>}
                </select>
                {!isLoadingSubjects && subjects.length === 0 && (
                  <p className="text-xs text-red-500 mt-1">Contact admin to add subjects</p>
                )}
              </div>
            </div>

            {formData.type === 'Group' && (
              <Input label="Max Students" type="number" value={formData.maxStudents}
                onChange={(e) => set('maxStudents', parseInt(e.target.value) || 1)} min="2" required />
            )}
          </CardContent>
        </Card>

        {/* Schedule */}
        <Card>
          <CardHeader><CardTitle>Schedule</CardTitle></CardHeader>
          <CardContent>
            <div className="grid md:grid-cols-3 gap-4">
              <Input label="Date" type="date" value={formData.date}
                onChange={(e) => set('date', e.target.value)}
                min={new Date().toISOString().split('T')[0]} required />
              <Input label="Time" type="time" value={formData.time}
                onChange={(e) => set('time', e.target.value)} required />
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Duration</label>
                <select value={formData.duration} onChange={(e) => set('duration', e.target.value)}
                  className="w-full px-4 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-indigo-500">
                  <option value="30">30 min</option>
                  <option value="60">60 min</option>
                  <option value="90">90 min</option>
                  <option value="120">120 min</option>
                </select>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Pricing */}
        <Card>
          <CardHeader><CardTitle>Pricing</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Pricing Type</label>
              <select value={formData.pricingType} onChange={(e) => set('pricingType', e.target.value)}
                className="w-full px-4 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-indigo-500">
                <option value="Fixed">Fixed Session Price</option>
                <option value="Hourly">Hourly Rate</option>
              </select>
            </div>
            <Input
              label={formData.pricingType === 'Hourly' ? 'Hourly Rate (₹/hr)' : 'Session Price (₹)'}
              type="number" value={formData.basePrice}
              onChange={(e) => set('basePrice', parseFloat(e.target.value) || 0)}
              min="0" required />
            <p className="text-xs text-gray-500">Platform fee is added at checkout and controlled by admin.</p>
          </CardContent>
        </Card>

        {/* Advanced Options */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Zap className="w-4 h-4 text-indigo-500" />
              Advanced Options
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">

            {/* Instant Booking */}
            <label className="flex items-start gap-3 cursor-pointer p-3 rounded-xl border border-gray-100 hover:bg-indigo-50/40 transition-colors">
              <input type="checkbox" checked={formData.instantBooking}
                onChange={(e) => set('instantBooking', e.target.checked)}
                className="mt-0.5 w-4 h-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-400" />
              <div>
                <p className="text-sm font-semibold text-gray-900 flex items-center gap-1.5">
                  <Zap className="w-3.5 h-3.5 text-indigo-500" />
                  Instant Booking
                </p>
                <p className="text-xs text-gray-500 mt-0.5">
                  Students are confirmed immediately after payment — no approval needed from you.
                </p>
              </div>
            </label>

            {/* Subscription Required */}
            <label className="flex items-start gap-3 cursor-pointer p-3 rounded-xl border border-gray-100 hover:bg-purple-50/40 transition-colors">
              <input type="checkbox" checked={formData.requiresSubscription}
                onChange={(e) => set('requiresSubscription', e.target.checked)}
                className="mt-0.5 w-4 h-4 rounded border-gray-300 text-purple-600 focus:ring-purple-400" />
              <div>
                <p className="text-sm font-semibold text-gray-900 flex items-center gap-1.5">
                  <Lock className="w-3.5 h-3.5 text-purple-500" />
                  Subscribers Only
                </p>
                <p className="text-xs text-gray-500 mt-0.5">
                  Only students with an active subscription plan can book this session.
                </p>
              </div>
            </label>

            {/* No-Show Protection */}
            <label className="flex items-start gap-3 cursor-pointer p-3 rounded-xl border border-gray-100 hover:bg-green-50/40 transition-colors">
              <input type="checkbox" checked={formData.noShowProtection}
                onChange={(e) => set('noShowProtection', e.target.checked)}
                className="mt-0.5 w-4 h-4 rounded border-gray-300 text-green-600 focus:ring-green-400" />
              <div>
                <p className="text-sm font-semibold text-gray-900 flex items-center gap-1.5">
                  <Info className="w-3.5 h-3.5 text-green-500" />
                  No-Show Protection
                </p>
                <p className="text-xs text-gray-500 mt-0.5">
                  If a student doesn't attend, they receive an automatic refund and 50 bonus points.
                </p>
              </div>
            </label>

            {/* Flash Sale */}
            <label className="flex items-start gap-3 cursor-pointer p-3 rounded-xl border border-gray-100 hover:bg-orange-50/40 transition-colors">
              <input type="checkbox" checked={formData.enableFlashSale}
                onChange={(e) => set('enableFlashSale', e.target.checked)}
                className="mt-0.5 w-4 h-4 rounded border-gray-300 text-orange-600 focus:ring-orange-400" />
              <div className="flex-1">
                <p className="text-sm font-semibold text-gray-900 flex items-center gap-1.5">
                  <Tag className="w-3.5 h-3.5 text-orange-500" />
                  Flash Sale
                </p>
                <p className="text-xs text-gray-500 mt-0.5 mb-2">
                  Offer a limited-time discounted price to attract more students.
                </p>
                {formData.enableFlashSale && (
                  <div className="grid grid-cols-2 gap-3 mt-2">
                    <div>
                      <label className="block text-xs font-medium text-gray-600 mb-1">Sale Price (₹)</label>
                      <input type="number" min="0" value={formData.flashSalePrice || ''}
                        onChange={(e) => set('flashSalePrice', parseFloat(e.target.value) || 0)}
                        placeholder={`< ${formData.basePrice}`}
                        className="w-full px-3 py-2 rounded-lg border border-orange-200 focus:outline-none focus:ring-2 focus:ring-orange-400 text-sm"
                      />
                    </div>
                    <div>
                      <label className="block text-xs font-medium text-gray-600 mb-1">Sale Ends At</label>
                      <input type="datetime-local" value={formData.flashSaleEndsAt}
                        onChange={(e) => set('flashSaleEndsAt', e.target.value)}
                        min={new Date().toISOString().slice(0, 16)}
                        className="w-full px-3 py-2 rounded-lg border border-orange-200 focus:outline-none focus:ring-2 focus:ring-orange-400 text-sm"
                      />
                    </div>
                  </div>
                )}
              </div>
            </label>

          </CardContent>
        </Card>

        {/* Meet Link Info */}
        <Card>
          <CardHeader><CardTitle>Google Meet Integration</CardTitle></CardHeader>
          <CardContent>
            <div className="p-4 bg-blue-50 border border-blue-200 rounded-xl flex items-start gap-3">
              <Video className="w-5 h-5 text-blue-600 flex-shrink-0 mt-0.5" />
              <div>
                <p className="font-semibold text-gray-900 text-sm">Meeting link auto-generated</p>
                <p className="text-xs text-gray-600 mt-0.5">
                  A Google Meet link (or Jitsi fallback) will be created automatically. Start the meeting from your Sessions page when it's time.
                </p>
              </div>
            </div>
          </CardContent>
        </Card>

        <div className="flex gap-3">
          <Button type="submit" size="lg" isLoading={isSubmitting}>Create Session</Button>
          <Button type="button" variant="outline" onClick={() => navigate('/tutor/sessions')}>Cancel</Button>
        </div>
      </form>
    </div>
  )
}

export default CreateSession
