import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { BookOpen, Clock, CheckCircle, AlertCircle } from 'lucide-react'
import { Card, CardContent } from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import { getMyEnrollments, MyEnrollment } from '../../services/enrollmentApi'

const statusColors: Record<string, { bg: string; text: string }> = {
  Active: { bg: 'bg-green-100', text: 'text-green-700' },
  Completed: { bg: 'bg-blue-100', text: 'text-blue-700' },
  Cancelled: { bg: 'bg-red-100', text: 'text-red-700' },
  Expired: { bg: 'bg-gray-100', text: 'text-gray-600' },
  Refunded: { bg: 'bg-orange-100', text: 'text-orange-700' },
}

const MyEnrollments = () => {
  const navigate = useNavigate()
  const [enrollments, setEnrollments] = useState<MyEnrollment[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    getMyEnrollments()
      .then(res => setEnrollments(res.data))
      .catch(err => setError(err.message || 'Failed to load enrollments'))
      .finally(() => setLoading(false))
  }, [])

  if (loading) return (
    <div className="flex items-center justify-center h-64">
      <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600" />
    </div>
  )

  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">My Courses</h1>
        <p className="text-gray-500 mt-1">{enrollments.length} enrollment{enrollments.length !== 1 ? 's' : ''}</p>
      </div>

      {error && (
        <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm flex items-center gap-2">
          <AlertCircle className="w-4 h-4 flex-shrink-0" />
          {error}
        </div>
      )}

      {enrollments.length === 0 ? (
        <Card>
          <CardContent className="py-16 text-center">
            <BookOpen className="w-12 h-12 text-gray-300 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">No courses yet</h3>
            <p className="text-gray-500 mb-6">Browse and enroll in a course to get started.</p>
            <Button onClick={() => navigate('/student/courses')}>Browse Courses</Button>
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-4">
          {enrollments.map(e => {
            const statusStyle = statusColors[e.status] || statusColors.Active
            return (
              <Card key={e.id} className="hover:shadow-md transition-shadow">
                <CardContent className="p-5">
                  <div className="flex items-start gap-4">
                    {e.courseThumbnail ? (
                      <img
                        src={e.courseThumbnail}
                        alt={e.courseTitle}
                        className="w-20 h-14 object-cover rounded-lg flex-shrink-0"
                      />
                    ) : (
                      <div className="w-20 h-14 bg-gradient-to-br from-primary-100 to-indigo-200 rounded-lg flex-shrink-0 flex items-center justify-center">
                        <BookOpen className="w-5 h-5 text-primary-400" />
                      </div>
                    )}

                    <div className="flex-1 min-w-0">
                      <div className="flex items-start justify-between gap-3">
                        <div>
                          <h3 className="font-semibold text-gray-900 leading-tight">{e.courseTitle}</h3>
                          <p className="text-sm text-gray-500 mt-0.5">by {e.tutorName}</p>
                        </div>
                        <span className={`text-xs font-medium px-2 py-1 rounded-full flex-shrink-0 ${statusStyle.bg} ${statusStyle.text}`}>
                          {e.status}
                        </span>
                      </div>

                      {/* Progress bar */}
                      <div className="mt-3">
                        <div className="flex items-center justify-between text-xs text-gray-500 mb-1">
                          <span>{e.sessionsCompleted} of {e.sessionsPurchased} sessions completed</span>
                          <span>{e.progressPercent}%</span>
                        </div>
                        <div className="h-2 bg-gray-100 rounded-full overflow-hidden">
                          <div
                            className="h-2 bg-primary-500 rounded-full transition-all"
                            style={{ width: `${e.progressPercent}%` }}
                          />
                        </div>
                      </div>

                      <div className="flex flex-wrap items-center gap-4 mt-3">
                        <div className="flex items-center gap-1.5 text-sm text-gray-600">
                          <Clock className="w-3.5 h-3.5 text-gray-400" />
                          <span>{e.sessionsRemaining} sessions remaining</span>
                        </div>
                        <div className="text-sm font-medium text-gray-700">
                          ₹{e.amountPaid.toLocaleString()} paid
                          <span className="text-xs text-gray-400 ml-1">({e.enrollmentType})</span>
                        </div>
                        {e.expiresAt && e.status === 'Active' && (
                          <div className="text-xs text-gray-400">
                            Expires {new Date(e.expiresAt).toLocaleDateString()}
                          </div>
                        )}
                        {e.completedAt && (
                          <div className="flex items-center gap-1 text-xs text-blue-600">
                            <CheckCircle className="w-3.5 h-3.5" />
                            Completed {new Date(e.completedAt).toLocaleDateString()}
                          </div>
                        )}
                      </div>
                    </div>
                  </div>

                  <div className="mt-3 pt-3 border-t flex justify-end">
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={() => navigate(`/student/courses/${e.courseId}`)}
                    >
                      View Course
                    </Button>
                  </div>
                </CardContent>
              </Card>
            )
          })}
        </div>
      )}
    </div>
  )
}

export default MyEnrollments
