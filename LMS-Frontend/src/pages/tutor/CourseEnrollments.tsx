import { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { ArrowLeft, Users, Loader2 } from 'lucide-react'
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card'
import { Badge } from '../../components/ui/Badge'
import Button from '../../components/ui/Button'
import { getCourseEnrollments, CourseEnrollmentItem } from '../../services/enrollmentApi'

const CourseEnrollments = () => {
  const { courseId } = useParams<{ courseId: string }>()
  const navigate = useNavigate()
  const [enrollments, setEnrollments] = useState<CourseEnrollmentItem[]>([])
  const [total, setTotal] = useState(0)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!courseId) return
    setIsLoading(true)
    getCourseEnrollments(courseId)
      .then(res => {
        setEnrollments((res as any).data || [])
        setTotal((res as any).total || 0)
      })
      .catch(err => setError(err.message || 'Failed to load enrollments'))
      .finally(() => setIsLoading(false))
  }, [courseId])

  return (
    <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="flex items-center gap-3 mb-6">
        <button
          onClick={() => navigate('/tutor/courses')}
          className="p-2 rounded-lg hover:bg-gray-100 text-gray-500 hover:text-gray-700 transition-colors"
        >
          <ArrowLeft className="w-5 h-5" />
        </button>
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Course Enrollments</h1>
          <p className="text-sm text-gray-500 mt-0.5">Students enrolled in this course</p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Users className="w-5 h-5 text-primary-600" />
            {isLoading ? 'Loading...' : `${total} Student${total !== 1 ? 's' : ''} Enrolled`}
          </CardTitle>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="flex items-center justify-center py-16">
              <Loader2 className="w-7 h-7 animate-spin text-primary-500" />
            </div>
          ) : error ? (
            <div className="text-center py-12">
              <p className="text-red-500 text-sm mb-3">{error}</p>
              <Button size="sm" onClick={() => window.location.reload()}>Retry</Button>
            </div>
          ) : enrollments.length === 0 ? (
            <div className="text-center py-16">
              <Users className="w-10 h-10 text-gray-200 mx-auto mb-3" />
              <p className="text-sm font-medium text-gray-700">No students enrolled yet</p>
              <p className="text-xs text-gray-400 mt-1">Students will appear here once they enroll in your course</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-gray-100">
                    <th className="text-left py-3 px-2 text-xs font-semibold text-gray-500 uppercase tracking-wide">Student</th>
                    <th className="text-left py-3 px-2 text-xs font-semibold text-gray-500 uppercase tracking-wide">Type</th>
                    <th className="text-center py-3 px-2 text-xs font-semibold text-gray-500 uppercase tracking-wide">Sessions</th>
                    <th className="text-right py-3 px-2 text-xs font-semibold text-gray-500 uppercase tracking-wide">Paid</th>
                    <th className="text-center py-3 px-2 text-xs font-semibold text-gray-500 uppercase tracking-wide">Status</th>
                    <th className="text-left py-3 px-2 text-xs font-semibold text-gray-500 uppercase tracking-wide">Enrolled</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-50">
                  {enrollments.map((e) => (
                    <tr key={e.id} className="hover:bg-gray-50/50 transition-colors">
                      <td className="py-3 px-2">
                        <p className="font-medium text-gray-900">{e.studentName}</p>
                        <p className="text-xs text-gray-400">{e.studentEmail}</p>
                      </td>
                      <td className="py-3 px-2 text-gray-600 capitalize">{e.enrollmentType}</td>
                      <td className="py-3 px-2 text-center text-gray-700">
                        {e.sessionsCompleted}/{e.sessionsPurchased}
                      </td>
                      <td className="py-3 px-2 text-right font-medium text-gray-900">₹{e.amountPaid?.toLocaleString()}</td>
                      <td className="py-3 px-2 text-center">
                        <Badge variant={e.status === 'Active' ? 'success' : e.status === 'Expired' ? 'warning' : 'info'}>
                          {e.status}
                        </Badge>
                      </td>
                      <td className="py-3 px-2 text-gray-500 text-xs">
                        {e.enrolledAt ? new Date(e.enrolledAt).toLocaleDateString('en-IN') : '—'}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  )
}

export default CourseEnrollments
