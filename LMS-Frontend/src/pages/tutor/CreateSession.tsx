import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { Video, Link as LinkIcon } from 'lucide-react'
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'
import { createSession, getSubjects, TeacherSubjectDto } from '../../services/sessionsApi'

const CreateSession = () => {
  const navigate = useNavigate()
  const [subjects, setSubjects] = useState<TeacherSubjectDto[]>([])
  const [isLoadingSubjects, setIsLoadingSubjects] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [googleMeetLink, setGoogleMeetLink] = useState('')
  const [isGeneratingLink, setIsGeneratingLink] = useState(false)

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
  })

  useEffect(() => {
    const fetchCreateData = async () => {
      try {
        const subjectsData = await getSubjects();
        setSubjects(subjectsData);
        if (subjectsData.length > 0) {
          setFormData(prev => ({ ...prev, subjectId: subjectsData[0].id }));
        }
      } catch (err) {
        console.error("Failed to fetch subjects", err);
      } finally {
        setIsLoadingSubjects(false);
      }
    }
    fetchCreateData();
  }, []);

  const handleGenerateMeetLink = async () => {
    setIsGeneratingLink(true)
    // Simulated Meet link generation (In production, the backend handles this via startSession or similar integration)
    setTimeout(() => {
      setGoogleMeetLink(`https://meet.google.com/${Math.random().toString(36).substring(2, 5)}-${Math.random().toString(36).substring(2, 6)}-${Math.random().toString(36).substring(2, 5)}`)
      setIsGeneratingLink(false)
    }, 1500)
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!googleMeetLink) {
      alert('Please generate a Google Meet link first')
      return
    }

    setIsSubmitting(true);
    try {
      // Build ISO string from local date/time input
      const scheduledAt = new Date(`${formData.date}T${formData.time}:00Z`).toISOString();
      
      await createSession({
        title: formData.title,
        description: formData.description,
        sessionType: formData.type,
        subjectId: formData.subjectId,
        scheduledAt: scheduledAt,
        duration: parseInt(formData.duration),
        basePrice: formData.basePrice,
        maxStudents: formData.type === 'OneOnOne' ? 1 : formData.maxStudents,
        pricingType: formData.pricingType
      });

      // API call success - now we can navigate
      console.log('Session Created Successfully')
      navigate('/tutor/sessions')
    } catch (err: any) {
      alert(err.message || "Failed to create session. Please check your data.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Create New Session</h1>
        <p className="text-gray-600">Schedule a new teaching session with Google Meet integration</p>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        <Card>
          <CardHeader>
            <CardTitle>Session Details</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <Input
              label="Session Title"
              value={formData.title}
              onChange={(e) => setFormData({ ...formData, title: e.target.value })}
              placeholder="e.g., React Fundamentals Workshop"
              required
            />

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Description
              </label>
              <textarea
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                rows={4}
                className="w-full px-4 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500"
                placeholder="What will students learn in this session?"
                required
              />
            </div>

            <div className="grid md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Session Type
                </label>
                <select
                  value={formData.type}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      type: e.target.value as 'OneOnOne' | 'Group',
                      maxStudents: e.target.value === 'OneOnOne' ? 1 : formData.maxStudents,
                    })
                  }
                  className="w-full px-4 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500"
                >
                  <option value="OneOnOne">1-on-1 Session</option>
                  <option value="Group">Group Session</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Subject
                </label>
                <select
                  value={formData.subjectId}
                  onChange={(e) => setFormData({ ...formData, subjectId: e.target.value })}
                  className="w-full px-4 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500 bg-white"
                  required
                >
                  {isLoadingSubjects ? (
                    <option>Loading Subjects...</option>
                  ) : subjects.length > 0 ? (
                    subjects.map(s => <option key={s.id} value={s.id}>{s.name}</option>)
                  ) : (
                    <option value="">No Subjects Available</option>
                  )}
                </select>
                {!isLoadingSubjects && subjects.length === 0 && (
                  <p className="text-[10px] text-red-500 mt-1 font-bold uppercase tracking-wider">Please contact Admin to add subjects</p>
                )}
              </div>
            </div>

            {formData.type === 'Group' && (
              <Input
                label="Max Students"
                type="number"
                value={formData.maxStudents}
                onChange={(e) =>
                  setFormData({ ...formData, maxStudents: parseInt(e.target.value) || 1 })
                }
                min="2"
                required
              />
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Schedule</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid md:grid-cols-3 gap-4">
              <Input
                label="Date"
                type="date"
                value={formData.date}
                onChange={(e) => setFormData({ ...formData, date: e.target.value })}
                min={new Date().toISOString().split('T')[0]}
                required
              />
              <Input
                label="Time"
                type="time"
                value={formData.time}
                onChange={(e) => setFormData({ ...formData, time: e.target.value })}
                required
              />
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Duration (minutes)
                </label>
                <select
                  value={formData.duration}
                  onChange={(e) => setFormData({ ...formData, duration: e.target.value })}
                  className="w-full px-4 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500"
                >
                  <option value="30">30 min</option>
                  <option value="60">60 min</option>
                  <option value="90">90 min</option>
                  <option value="120">120 min</option>
                </select>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Pricing</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Pricing Type
              </label>
              <select
                value={formData.pricingType}
                onChange={(e) =>
                  setFormData({ ...formData, pricingType: e.target.value as 'Fixed' | 'Hourly' })
                }
                className="w-full px-4 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500"
              >
                <option value="Fixed">Fixed Session Price</option>
                <option value="Hourly">Hourly Rate</option>
              </select>
            </div>
            <Input
              label={formData.pricingType === 'Hourly' ? 'Hourly Rate (₹/hr)' : 'Base Price (₹)'}
              type="number"
              value={formData.basePrice}
              onChange={(e) => setFormData({ ...formData, basePrice: parseFloat(e.target.value) || 0 })}
              min="0"
              required
            />
            <p className="text-xs text-gray-500">
              Platform fee is added at checkout and controlled by admin.
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Google Meet Integration</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {!googleMeetLink ? (
              <div className="p-6 bg-primary-50 border border-primary-200 rounded-lg">
                <div className="flex items-center gap-3 mb-4">
                  <Video className="w-6 h-6 text-primary-600" />
                  <div>
                    <h3 className="font-semibold text-gray-900">Generate Google Meet Link</h3>
                    <p className="text-sm text-gray-600">
                      A Google Meet link will be created and scheduled for this session
                    </p>
                  </div>
                </div>
                <Button
                  type="button"
                  onClick={handleGenerateMeetLink}
                  isLoading={isGeneratingLink}
                >
                  <LinkIcon className="mr-2 w-5 h-5" />
                  {isGeneratingLink ? 'Generating...' : 'Generate Google Meet Link'}
                </Button>
              </div>
            ) : (
              <div className="p-4 bg-green-50 border border-green-200 rounded-lg">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-3">
                    <Video className="w-5 h-5 text-green-600" />
                    <div>
                      <p className="font-semibold text-gray-900">Google Meet Link Generated</p>
                      <p className="text-sm text-gray-600">{googleMeetLink}</p>
                    </div>
                  </div>
                  <Button
                    type="button"
                    size="sm"
                    variant="outline"
                    onClick={() => setGoogleMeetLink('')}
                  >
                    Regenerate
                  </Button>
                </div>
              </div>
            )}
            <p className="text-xs text-gray-500">
              The link will be shared with students when they book. You can start the meeting from your sessions page.
            </p>
          </CardContent>
        </Card>

        <div className="flex gap-3">
          <Button type="submit" size="lg" isLoading={isSubmitting}>
            Create Session
          </Button>
          <Button type="button" variant="outline" onClick={() => navigate('/tutor/sessions')}>
            Cancel
          </Button>
        </div>
      </form>
    </div>
  )
}

export default CreateSession
