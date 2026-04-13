import { useState, useEffect, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { Search, Filter, Star, Users, Clock, BookOpen, ChevronDown } from 'lucide-react'
import { Card, CardContent } from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import { browseCourses, CourseListItem } from '../../services/courseApi'

const LEVELS = ['', 'Beginner', 'Intermediate', 'Advanced', 'AllLevels']
const MAX_PRICES = [0, 500, 1000, 2000, 5000]

const BrowseCourses = () => {
  const navigate = useNavigate()
  const [courses, setCourses] = useState<CourseListItem[]>([])
  const [total, setTotal] = useState(0)
  const [loading, setLoading] = useState(true)
  const [page, setPage] = useState(1)

  const [q, setQ] = useState('')
  const [level, setLevel] = useState('')
  const [maxPrice, setMaxPrice] = useState(0)
  const [showFilters, setShowFilters] = useState(false)

  const pageSize = 12

  const fetchCourses = useCallback(async (resetPage = false) => {
    const p = resetPage ? 1 : page
    if (resetPage) setPage(1)
    setLoading(true)
    try {
      const res = await browseCourses({
        q: q || undefined,
        level: level || undefined,
        maxPrice: maxPrice || undefined,
        page: p,
        pageSize,
      })
      setCourses(res.data)
      setTotal(res.total)
    } catch (err) {
      console.error('Failed to browse courses', err)
    } finally {
      setLoading(false)
    }
  }, [q, level, maxPrice, page])

  useEffect(() => {
    fetchCourses()
  }, [page])

  const handleSearch = () => {
    fetchCourses(true)
  }

  const totalPages = Math.ceil(total / pageSize)

  return (
    <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">Browse Courses</h1>
        <p className="text-gray-500 mt-1">Discover courses from expert tutors</p>
      </div>

      {/* Search bar */}
      <div className="flex gap-3 mb-4">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            type="text"
            className="w-full pl-10 pr-4 py-2.5 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-500"
            placeholder="Search by title, subject, tutor..."
            value={q}
            onChange={e => setQ(e.target.value)}
            onKeyDown={e => e.key === 'Enter' && handleSearch()}
          />
        </div>
        <Button onClick={handleSearch}>Search</Button>
        <Button
          variant="outline"
          onClick={() => setShowFilters(!showFilters)}
          className="flex items-center gap-1.5"
        >
          <Filter className="w-4 h-4" />
          Filters
          <ChevronDown className={`w-3.5 h-3.5 transition-transform ${showFilters ? 'rotate-180' : ''}`} />
        </Button>
      </div>

      {showFilters && (
        <div className="flex flex-wrap gap-4 mb-6 p-4 bg-gray-50 rounded-lg border">
          <div>
            <label className="block text-xs font-medium text-gray-600 mb-1">Level</label>
            <select
              className="border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary-500"
              value={level}
              onChange={e => setLevel(e.target.value)}
            >
              <option value="">All Levels</option>
              {LEVELS.filter(Boolean).map(l => <option key={l} value={l}>{l}</option>)}
            </select>
          </div>
          <div>
            <label className="block text-xs font-medium text-gray-600 mb-1">Max Price (₹/session)</label>
            <select
              className="border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary-500"
              value={maxPrice}
              onChange={e => setMaxPrice(parseInt(e.target.value))}
            >
              <option value={0}>Any Price</option>
              {MAX_PRICES.filter(Boolean).map(p => <option key={p} value={p}>Up to ₹{p}</option>)}
            </select>
          </div>
          <div className="flex items-end">
            <Button variant="outline" size="sm" onClick={() => { setLevel(''); setMaxPrice(0); setQ('') }}>
              Clear Filters
            </Button>
          </div>
        </div>
      )}

      {loading ? (
        <div className="flex items-center justify-center h-64">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600" />
        </div>
      ) : courses.length === 0 ? (
        <div className="text-center py-16">
          <BookOpen className="w-12 h-12 text-gray-300 mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-700 mb-2">No courses found</h3>
          <p className="text-gray-400 text-sm">Try adjusting your search or filters</p>
        </div>
      ) : (
        <>
          <p className="text-sm text-gray-500 mb-4">{total} course{total !== 1 ? 's' : ''} found</p>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5">
            {courses.map(course => (
              <Card
                key={course.id}
                className="hover:shadow-lg transition-shadow cursor-pointer group"
                onClick={() => navigate(`/student/courses/${course.id}`)}
              >
                <div className="relative">
                  {course.thumbnailUrl ? (
                    <img
                      src={course.thumbnailUrl}
                      alt={course.title}
                      className="w-full h-36 object-cover rounded-t-xl"
                    />
                  ) : (
                    <div className="w-full h-36 bg-gradient-to-br from-primary-100 to-indigo-200 rounded-t-xl flex items-center justify-center">
                      <BookOpen className="w-10 h-10 text-primary-400" />
                    </div>
                  )}
                  {course.trialAvailable && (
                    <span className="absolute top-2 left-2 bg-green-500 text-white text-xs px-2 py-0.5 rounded-full font-medium">
                      Trial available
                    </span>
                  )}
                  <span className={`absolute top-2 right-2 text-xs px-2 py-0.5 rounded-full font-medium ${
                    course.level === 'Beginner' ? 'bg-blue-100 text-blue-700' :
                    course.level === 'Intermediate' ? 'bg-yellow-100 text-yellow-700' :
                    course.level === 'Advanced' ? 'bg-red-100 text-red-700' :
                    'bg-gray-100 text-gray-600'
                  }`}>
                    {course.level}
                  </span>
                </div>
                <CardContent className="p-4">
                  <h3 className="font-semibold text-gray-900 text-sm leading-tight mb-1 line-clamp-2 group-hover:text-primary-700">
                    {course.title}
                  </h3>
                  {course.tutorName && (
                    <p className="text-xs text-gray-500 mb-2">by {course.tutorName}</p>
                  )}
                  {course.shortDescription && (
                    <p className="text-xs text-gray-500 line-clamp-2 mb-3">{course.shortDescription}</p>
                  )}
                  <div className="flex items-center gap-3 text-xs text-gray-500 mb-3">
                    <span className="flex items-center gap-1">
                      <Clock className="w-3.5 h-3.5" />
                      {course.totalSessions} sessions
                    </span>
                    <span className="flex items-center gap-1">
                      <Users className="w-3.5 h-3.5" />
                      {course.totalEnrollments} enrolled
                    </span>
                    {course.averageRating > 0 && (
                      <span className="flex items-center gap-1">
                        <Star className="w-3.5 h-3.5 text-yellow-400 fill-yellow-400" />
                        {course.averageRating.toFixed(1)}
                      </span>
                    )}
                  </div>
                  <div className="flex items-center justify-between">
                    <div>
                      <span className="font-bold text-primary-700">₹{course.pricePerSession.toLocaleString()}</span>
                      <span className="text-xs text-gray-400">/session</span>
                      {course.bundlePrice && (
                        <div className="text-xs text-green-600 font-medium">
                          Bundle: ₹{course.bundlePrice.toLocaleString()}
                        </div>
                      )}
                    </div>
                    <Button size="sm" variant="outline">View</Button>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>

          {totalPages > 1 && (
            <div className="flex items-center justify-center gap-2 mt-8">
              <Button
                variant="outline" size="sm"
                disabled={page <= 1}
                onClick={() => setPage(p => p - 1)}
              >
                Previous
              </Button>
              <span className="text-sm text-gray-600">Page {page} of {totalPages}</span>
              <Button
                variant="outline" size="sm"
                disabled={page >= totalPages}
                onClick={() => setPage(p => p + 1)}
              >
                Next
              </Button>
            </div>
          )}
        </>
      )}
    </div>
  )
}

export default BrowseCourses
