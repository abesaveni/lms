import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { Plus, BookOpen, Users, Star, Edit2, Trash2, Eye, EyeOff, MoreVertical } from 'lucide-react'
import { Card, CardContent } from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import { getMyCourses, deleteCourse, updateCourseStatus, CourseListItem } from '../../services/courseApi'

const statusColors: Record<string, string> = {
  Draft: 'bg-gray-100 text-gray-700',
  Published: 'bg-green-100 text-green-700',
  Paused: 'bg-yellow-100 text-yellow-700',
  Archived: 'bg-red-100 text-red-700',
}

const TutorMyCourses = () => {
  const navigate = useNavigate()
  const [courses, setCourses] = useState<CourseListItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [actionMenu, setActionMenu] = useState<string | null>(null)

  useEffect(() => {
    fetchCourses()
  }, [])

  const fetchCourses = async () => {
    try {
      setLoading(true)
      const res = await getMyCourses()
      setCourses(res.data)
    } catch (err: any) {
      setError(err.message || 'Failed to load courses')
    } finally {
      setLoading(false)
    }
  }

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this course? This cannot be undone.')) return
    try {
      await deleteCourse(id)
      setCourses(prev => prev.filter(c => c.id !== id))
    } catch (err: any) {
      alert(err.message || 'Failed to delete course')
    }
  }

  const handleStatusToggle = async (course: CourseListItem) => {
    const newStatus = course.status === 'Published' ? 'Paused' : 'Published'
    try {
      await updateCourseStatus(course.id, newStatus)
      setCourses(prev => prev.map(c => c.id === course.id ? { ...c, status: newStatus } : c))
    } catch (err: any) {
      alert(err.message || 'Failed to update status')
    }
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600" />
      </div>
    )
  }

  return (
    <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">My Courses</h1>
          <p className="text-gray-500 mt-1">{courses.length} course{courses.length !== 1 ? 's' : ''}</p>
        </div>
        <Button onClick={() => navigate('/tutor/courses/create')} className="flex items-center gap-2">
          <Plus className="w-4 h-4" />
          Create Course
        </Button>
      </div>

      {error && (
        <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">{error}</div>
      )}

      {courses.length === 0 ? (
        <Card>
          <CardContent className="py-16 text-center">
            <BookOpen className="w-12 h-12 text-gray-300 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">No courses yet</h3>
            <p className="text-gray-500 mb-6">Create your first course to start teaching students.</p>
            <Button onClick={() => navigate('/tutor/courses/create')}>
              <Plus className="w-4 h-4 mr-2" />
              Create Course
            </Button>
          </CardContent>
        </Card>
      ) : (
        <div className="grid gap-4">
          {courses.map(course => (
            <Card key={course.id} className="hover:shadow-md transition-shadow">
              <CardContent className="p-6">
                <div className="flex items-start gap-4">
                  {course.thumbnailUrl ? (
                    <img
                      src={course.thumbnailUrl}
                      alt={course.title}
                      className="w-24 h-16 object-cover rounded-lg flex-shrink-0"
                    />
                  ) : (
                    <div className="w-24 h-16 bg-gradient-to-br from-primary-100 to-primary-200 rounded-lg flex-shrink-0 flex items-center justify-center">
                      <BookOpen className="w-6 h-6 text-primary-600" />
                    </div>
                  )}

                  <div className="flex-1 min-w-0">
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <h3 className="font-semibold text-gray-900 text-lg leading-tight">{course.title}</h3>
                        <div className="flex flex-wrap gap-2 mt-1">
                          {course.subjectName && (
                            <span className="text-xs text-gray-500">{course.subjectName}</span>
                          )}
                          <span className="text-xs text-gray-400">•</span>
                          <span className="text-xs text-gray-500">{course.level}</span>
                          <span className="text-xs text-gray-400">•</span>
                          <span className="text-xs text-gray-500">{course.totalSessions} sessions</span>
                          <span className="text-xs text-gray-400">•</span>
                          <span className="text-xs text-gray-500">{course.deliveryType}</span>
                        </div>
                      </div>

                      <div className="flex items-center gap-2 flex-shrink-0">
                        <span className={`text-xs font-medium px-2 py-1 rounded-full ${statusColors[course.status || 'Draft'] || statusColors.Draft}`}>
                          {course.status || 'Draft'}
                        </span>
                        <div className="relative">
                          <button
                            onClick={() => setActionMenu(actionMenu === course.id ? null : course.id)}
                            className="p-1 rounded-md hover:bg-gray-100 text-gray-500"
                          >
                            <MoreVertical className="w-4 h-4" />
                          </button>
                          {actionMenu === course.id && (
                            <div className="absolute right-0 mt-1 w-40 bg-white rounded-lg shadow-lg border border-gray-200 z-10 py-1">
                              <button
                                onClick={() => { navigate(`/tutor/courses/edit/${course.id}`); setActionMenu(null) }}
                                className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-50 flex items-center gap-2"
                              >
                                <Edit2 className="w-3.5 h-3.5" /> Edit
                              </button>
                              <button
                                onClick={() => { handleStatusToggle(course); setActionMenu(null) }}
                                className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-50 flex items-center gap-2"
                              >
                                {course.status === 'Published'
                                  ? <><EyeOff className="w-3.5 h-3.5" /> Pause</>
                                  : <><Eye className="w-3.5 h-3.5" /> Publish</>
                                }
                              </button>
                              <button
                                onClick={() => { navigate(`/tutor/courses/${course.id}/enrollments`); setActionMenu(null) }}
                                className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-50 flex items-center gap-2"
                              >
                                <Users className="w-3.5 h-3.5" /> Students
                              </button>
                              <hr className="my-1" />
                              <button
                                onClick={() => { handleDelete(course.id); setActionMenu(null) }}
                                className="w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-red-50 flex items-center gap-2"
                              >
                                <Trash2 className="w-3.5 h-3.5" /> Delete
                              </button>
                            </div>
                          )}
                        </div>
                      </div>
                    </div>

                    <div className="flex items-center gap-6 mt-3">
                      <div className="flex items-center gap-1 text-sm text-gray-600">
                        <Users className="w-4 h-4 text-gray-400" />
                        <span>{course.totalEnrollments} enrolled</span>
                      </div>
                      <div className="flex items-center gap-1 text-sm text-gray-600">
                        <Star className="w-4 h-4 text-yellow-400 fill-yellow-400" />
                        <span>{course.averageRating > 0 ? course.averageRating.toFixed(1) : 'No ratings'}</span>
                        {course.totalReviews > 0 && (
                          <span className="text-gray-400">({course.totalReviews})</span>
                        )}
                      </div>
                      <div className="text-sm font-semibold text-primary-700">
                        ₹{course.pricePerSession.toLocaleString()}/session
                        {course.bundlePrice && (
                          <span className="ml-2 text-gray-500 font-normal">• ₹{course.bundlePrice.toLocaleString()} bundle</span>
                        )}
                      </div>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  )
}

export default TutorMyCourses
