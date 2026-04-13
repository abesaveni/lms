import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { Plus, Trash2, ChevronRight, ChevronLeft, BookOpen, DollarSign, Settings, List } from 'lucide-react'
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'
import {
  createCourse, updateCourse, getCourseById,
  CreateCoursePayload, SyllabusItem
} from '../../services/courseApi'

const LEVELS = ['Beginner', 'Intermediate', 'Advanced', 'AllLevels']
const DELIVERY_TYPES = ['LiveOneOnOne', 'LiveGroup', 'Recorded']
const LANGUAGES = ['English', 'Hindi', 'Tamil', 'Telugu', 'Kannada', 'Malayalam', 'Bengali', 'Marathi']

const STEPS = [
  { id: 1, label: 'Basic Info', icon: BookOpen },
  { id: 2, label: 'Pricing', icon: DollarSign },
  { id: 3, label: 'Syllabus', icon: List },
  { id: 4, label: 'Settings', icon: Settings },
]

const defaultPayload: CreateCoursePayload = {
  title: '',
  shortDescription: '',
  fullDescription: '',
  subjectName: '',
  categoryName: '',
  level: 'Beginner',
  language: 'English',
  thumbnailUrl: '',
  tags: [],
  totalSessions: 10,
  sessionDurationMinutes: 60,
  deliveryType: 'LiveOneOnOne',
  maxStudentsPerBatch: 1,
  pricePerSession: 0,
  bundlePrice: null,
  allowPartialBooking: false,
  minSessionsForPartial: 5,
  refundPolicy: '',
  trialAvailable: false,
  trialDurationMinutes: 30,
  trialPrice: 0,
  prerequisites: '',
  materialsRequired: '',
  whatYouWillLearn: '',
  syllabus: [],
}

const CreateEditCourse = () => {
  const navigate = useNavigate()
  const { id } = useParams()
  const isEdit = Boolean(id)

  const [step, setStep] = useState(1)
  const [form, setForm] = useState<CreateCoursePayload>(defaultPayload)
  const [tagsInput, setTagsInput] = useState('')
  const [loading, setLoading] = useState(false)
  const [loadingCourse, setLoadingCourse] = useState(isEdit)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (isEdit && id) {
      getCourseById(id).then(course => {
        const syllabus: SyllabusItem[] = course.syllabusJson
          ? JSON.parse(course.syllabusJson)
          : []
        const tags: string[] = course.tagsJson
          ? JSON.parse(course.tagsJson)
          : []
        setForm({
          title: course.title,
          shortDescription: course.shortDescription || '',
          fullDescription: course.fullDescription || '',
          subjectName: course.subjectName || '',
          categoryName: course.categoryName || '',
          level: course.level,
          language: course.language,
          thumbnailUrl: course.thumbnailUrl || '',
          tags,
          totalSessions: course.totalSessions,
          sessionDurationMinutes: course.sessionDurationMinutes,
          deliveryType: course.deliveryType,
          maxStudentsPerBatch: course.maxStudentsPerBatch,
          pricePerSession: course.pricePerSession,
          bundlePrice: course.bundlePrice || null,
          allowPartialBooking: course.allowPartialBooking,
          minSessionsForPartial: course.minSessionsForPartial,
          refundPolicy: course.refundPolicy || '',
          trialAvailable: course.trialAvailable,
          trialDurationMinutes: course.trialDurationMinutes,
          trialPrice: course.trialPrice,
          prerequisites: course.prerequisites || '',
          materialsRequired: course.materialsRequired || '',
          whatYouWillLearn: course.whatYouWillLearn || '',
          syllabus,
        })
        setTagsInput(tags.join(', '))
      }).catch(err => {
        setError(err.message || 'Failed to load course')
      }).finally(() => setLoadingCourse(false))
    }
  }, [id, isEdit])

  const set = (field: keyof CreateCoursePayload, value: any) =>
    setForm(prev => ({ ...prev, [field]: value }))

  const addSyllabusItem = () => {
    const next = (form.syllabus?.length ?? 0) + 1
    set('syllabus', [...(form.syllabus || []), { sessionNumber: next, title: '', topics: '', description: '' }])
  }

  const updateSyllabus = (i: number, field: keyof SyllabusItem, value: string | number) => {
    const updated = [...(form.syllabus || [])]
    updated[i] = { ...updated[i], [field]: value }
    set('syllabus', updated)
  }

  const removeSyllabus = (i: number) => {
    const updated = (form.syllabus || []).filter((_, idx) => idx !== i)
      .map((s, idx) => ({ ...s, sessionNumber: idx + 1 }))
    set('syllabus', updated)
  }

  const handleSubmit = async () => {
    if (!form.title.trim()) { setError('Title is required'); return }
    if (form.pricePerSession <= 0) { setError('Price per session must be greater than 0'); return }

    const tags = tagsInput.split(',').map(t => t.trim()).filter(Boolean)
    const payload = { ...form, tags }

    setLoading(true)
    setError(null)
    try {
      if (isEdit && id) {
        await updateCourse(id, payload)
      } else {
        await createCourse(payload)
      }
      navigate('/tutor/courses')
    } catch (err: any) {
      setError(err.message || 'Failed to save course')
    } finally {
      setLoading(false)
    }
  }

  if (loadingCourse) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600" />
      </div>
    )
  }

  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">{isEdit ? 'Edit Course' : 'Create Course'}</h1>
        <p className="text-gray-500 mt-1">Fill in the details to {isEdit ? 'update' : 'publish'} your course</p>
      </div>

      {/* Step indicator */}
      <div className="flex items-center mb-8">
        {STEPS.map((s, i) => {
          return (
            <div key={s.id} className="flex items-center">
              <button
                onClick={() => setStep(s.id)}
                className={`flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-medium transition-colors ${
                  step === s.id
                    ? 'bg-primary-100 text-primary-700'
                    : step > s.id
                    ? 'text-green-600'
                    : 'text-gray-400'
                }`}
              >
                <div className={`w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold ${
                  step === s.id ? 'bg-primary-600 text-white' : step > s.id ? 'bg-green-500 text-white' : 'bg-gray-200 text-gray-500'
                }`}>
                  {step > s.id ? '✓' : s.id}
                </div>
                <span className="hidden sm:block">{s.label}</span>
              </button>
              {i < STEPS.length - 1 && <ChevronRight className="w-4 h-4 text-gray-300 mx-1" />}
            </div>
          )
        })}
      </div>

      {error && (
        <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">{error}</div>
      )}

      {/* Step 1: Basic Info */}
      {step === 1 && (
        <Card>
          <CardHeader><CardTitle>Basic Information</CardTitle></CardHeader>
          <CardContent className="space-y-5">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Course Title *</label>
              <Input
                value={form.title}
                onChange={e => set('title', e.target.value)}
                placeholder="e.g., Complete Python Programming for Beginners"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Short Description</label>
              <Input
                value={form.shortDescription}
                onChange={e => set('shortDescription', e.target.value)}
                placeholder="One-line summary shown in course cards"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Full Description</label>
              <textarea
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 min-h-28"
                value={form.fullDescription}
                onChange={e => set('fullDescription', e.target.value)}
                placeholder="Detailed description of what students will learn..."
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Subject</label>
                <Input
                  value={form.subjectName}
                  onChange={e => set('subjectName', e.target.value)}
                  placeholder="e.g., Python, Mathematics"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Category</label>
                <Input
                  value={form.categoryName}
                  onChange={e => set('categoryName', e.target.value)}
                  placeholder="e.g., Programming, Science"
                />
              </div>
            </div>
            <div className="grid grid-cols-3 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Level</label>
                <select
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary-500"
                  value={form.level}
                  onChange={e => set('level', e.target.value)}
                >
                  {LEVELS.map(l => <option key={l} value={l}>{l}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Language</label>
                <select
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary-500"
                  value={form.language}
                  onChange={e => set('language', e.target.value)}
                >
                  {LANGUAGES.map(l => <option key={l} value={l}>{l}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Delivery Type</label>
                <select
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary-500"
                  value={form.deliveryType}
                  onChange={e => set('deliveryType', e.target.value)}
                >
                  {DELIVERY_TYPES.map(d => <option key={d} value={d}>{d}</option>)}
                </select>
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Total Sessions</label>
                <Input
                  type="number" min={1}
                  value={form.totalSessions}
                  onChange={e => set('totalSessions', parseInt(e.target.value) || 1)}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Session Duration (minutes)</label>
                <Input
                  type="number" min={15}
                  value={form.sessionDurationMinutes}
                  onChange={e => set('sessionDurationMinutes', parseInt(e.target.value) || 60)}
                />
              </div>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Thumbnail URL</label>
              <Input
                value={form.thumbnailUrl}
                onChange={e => set('thumbnailUrl', e.target.value)}
                placeholder="https://..."
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Tags (comma-separated)</label>
              <Input
                value={tagsInput}
                onChange={e => setTagsInput(e.target.value)}
                placeholder="python, programming, beginner"
              />
            </div>
          </CardContent>
        </Card>
      )}

      {/* Step 2: Pricing */}
      {step === 2 && (
        <Card>
          <CardHeader><CardTitle>Pricing & Billing Options</CardTitle></CardHeader>
          <CardContent className="space-y-5">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Price per Session (₹) *</label>
                <Input
                  type="number" min={0}
                  value={form.pricePerSession}
                  onChange={e => set('pricePerSession', parseFloat(e.target.value) || 0)}
                />
                <p className="text-xs text-gray-400 mt-1">Charged per individual session</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Bundle Price (₹)</label>
                <Input
                  type="number" min={0}
                  value={form.bundlePrice ?? ''}
                  onChange={e => set('bundlePrice', e.target.value ? parseFloat(e.target.value) : null)}
                  placeholder="Leave empty to disable"
                />
                <p className="text-xs text-gray-400 mt-1">Discounted price for full course</p>
              </div>
            </div>

            <div className="border-t pt-4">
              <div className="flex items-center justify-between mb-3">
                <div>
                  <h4 className="font-medium text-gray-800">Allow Partial Booking</h4>
                  <p className="text-xs text-gray-500">Students can buy a subset of sessions</p>
                </div>
                <label className="relative inline-flex items-center cursor-pointer">
                  <input
                    type="checkbox"
                    className="sr-only peer"
                    checked={form.allowPartialBooking}
                    onChange={e => set('allowPartialBooking', e.target.checked)}
                  />
                  <div className="w-11 h-6 bg-gray-200 peer-focus:ring-2 peer-focus:ring-primary-500 rounded-full peer peer-checked:after:translate-x-full after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-primary-600"></div>
                </label>
              </div>
              {form.allowPartialBooking && (
                <div className="ml-4">
                  <label className="block text-sm font-medium text-gray-700 mb-1">Minimum Sessions for Partial</label>
                  <Input
                    type="number" min={1}
                    value={form.minSessionsForPartial}
                    onChange={e => set('minSessionsForPartial', parseInt(e.target.value) || 1)}
                    className="w-40"
                  />
                </div>
              )}
            </div>

            <div className="border-t pt-4">
              <div className="flex items-center justify-between mb-3">
                <div>
                  <h4 className="font-medium text-gray-800">Trial Session Available</h4>
                  <p className="text-xs text-gray-500">Offer a trial before full enrollment</p>
                </div>
                <label className="relative inline-flex items-center cursor-pointer">
                  <input
                    type="checkbox"
                    className="sr-only peer"
                    checked={form.trialAvailable}
                    onChange={e => set('trialAvailable', e.target.checked)}
                  />
                  <div className="w-11 h-6 bg-gray-200 peer-focus:ring-2 peer-focus:ring-primary-500 rounded-full peer peer-checked:after:translate-x-full after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-primary-600"></div>
                </label>
              </div>
              {form.trialAvailable && (
                <div className="grid grid-cols-2 gap-4 ml-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Trial Duration (minutes)</label>
                    <Input
                      type="number" min={15}
                      value={form.trialDurationMinutes}
                      onChange={e => set('trialDurationMinutes', parseInt(e.target.value) || 30)}
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Trial Price (₹, 0 = free)</label>
                    <Input
                      type="number" min={0}
                      value={form.trialPrice}
                      onChange={e => set('trialPrice', parseFloat(e.target.value) || 0)}
                    />
                  </div>
                </div>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Refund Policy</label>
              <textarea
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 min-h-20"
                value={form.refundPolicy}
                onChange={e => set('refundPolicy', e.target.value)}
                placeholder="Describe your refund/cancellation policy..."
              />
            </div>
          </CardContent>
        </Card>
      )}

      {/* Step 3: Syllabus */}
      {step === 3 && (
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle>Syllabus</CardTitle>
              <Button variant="outline" size="sm" onClick={addSyllabusItem} className="flex items-center gap-1.5">
                <Plus className="w-3.5 h-3.5" /> Add Session
              </Button>
            </div>
          </CardHeader>
          <CardContent className="space-y-4">
            {(form.syllabus || []).length === 0 ? (
              <div className="text-center py-10 text-gray-400">
                <List className="w-8 h-8 mx-auto mb-2 opacity-50" />
                <p className="text-sm">No sessions added yet. Click "Add Session" to start building your syllabus.</p>
              </div>
            ) : (
              (form.syllabus || []).map((item, i) => (
                <div key={i} className="border border-gray-200 rounded-lg p-4 space-y-3">
                  <div className="flex items-center justify-between">
                    <span className="text-sm font-semibold text-primary-600">Session {item.sessionNumber}</span>
                    <button
                      onClick={() => removeSyllabus(i)}
                      className="text-red-400 hover:text-red-600 p-1"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-600 mb-1">Title *</label>
                    <Input
                      value={item.title}
                      onChange={e => updateSyllabus(i, 'title', e.target.value)}
                      placeholder="e.g., Introduction to Variables"
                      className="text-sm"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-600 mb-1">Topics Covered</label>
                    <Input
                      value={item.topics || ''}
                      onChange={e => updateSyllabus(i, 'topics', e.target.value)}
                      placeholder="e.g., int, str, float, boolean"
                      className="text-sm"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-600 mb-1">Description</label>
                    <textarea
                      className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 min-h-16"
                      value={item.description || ''}
                      onChange={e => updateSyllabus(i, 'description', e.target.value)}
                      placeholder="Brief description of this session..."
                    />
                  </div>
                </div>
              ))
            )}
          </CardContent>
        </Card>
      )}

      {/* Step 4: Settings */}
      {step === 4 && (
        <Card>
          <CardHeader><CardTitle>Course Settings</CardTitle></CardHeader>
          <CardContent className="space-y-5">
            {form.deliveryType === 'LiveGroup' && (
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Max Students per Batch</label>
                <Input
                  type="number" min={2}
                  value={form.maxStudentsPerBatch}
                  onChange={e => set('maxStudentsPerBatch', parseInt(e.target.value) || 1)}
                  className="w-40"
                />
              </div>
            )}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Prerequisites</label>
              <textarea
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 min-h-20"
                value={form.prerequisites}
                onChange={e => set('prerequisites', e.target.value)}
                placeholder="What should students know before taking this course?"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Materials Required</label>
              <textarea
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 min-h-20"
                value={form.materialsRequired}
                onChange={e => set('materialsRequired', e.target.value)}
                placeholder="e.g., Laptop with Python installed, notebook..."
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">What You Will Learn</label>
              <textarea
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 min-h-24"
                value={form.whatYouWillLearn}
                onChange={e => set('whatYouWillLearn', e.target.value)}
                placeholder="List the key outcomes students will achieve..."
              />
            </div>
          </CardContent>
        </Card>
      )}

      {/* Navigation buttons */}
      <div className="flex items-center justify-between mt-6">
        <Button
          variant="outline"
          onClick={() => step > 1 ? setStep(step - 1) : navigate('/tutor/courses')}
          className="flex items-center gap-2"
        >
          <ChevronLeft className="w-4 h-4" />
          {step === 1 ? 'Cancel' : 'Back'}
        </Button>

        {step < STEPS.length ? (
          <Button onClick={() => setStep(step + 1)} className="flex items-center gap-2">
            Next <ChevronRight className="w-4 h-4" />
          </Button>
        ) : (
          <Button onClick={handleSubmit} disabled={loading}>
            {loading ? 'Saving...' : isEdit ? 'Update Course' : 'Create Course'}
          </Button>
        )}
      </div>
    </div>
  )
}

export default CreateEditCourse
