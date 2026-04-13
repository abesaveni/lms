import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { Search, SlidersHorizontal, Calendar, Users, Clock, Loader2 } from 'lucide-react'
import Button from '../components/ui/Button'
import Input from '../components/ui/Input'
import { Card } from '../components/ui/Card'
import { Badge } from '../components/ui/Badge'
import { Avatar } from '../components/ui/Avatar'
import { getSessions, SessionDto } from '../services/sessionsApi'
import { getCurrentUser } from '../utils/auth'

const Sessions = () => {
  const navigate = useNavigate()
  const [searchQuery, setSearchQuery] = useState('')
  const [showFilters, setShowFilters] = useState(false)
  const [sessions, setSessions] = useState<SessionDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [filters, setFilters] = useState({
    sessionType: '',
    subject: '',
  })
  const [sortBy, setSortBy] = useState('date')
  const user = getCurrentUser()
  const isAuthenticated = !!user

  // Load sessions on mount and when filters change
  useEffect(() => {
    loadSessions()
  }, [searchQuery, filters, sortBy])

  // Real-time updates: Poll every 30 seconds
  useEffect(() => {
    const interval = setInterval(() => {
      loadSessions()
    }, 30000) // Refresh every 30 seconds

    return () => clearInterval(interval)
  }, [searchQuery, filters, sortBy])

  const loadSessions = async () => {
    setIsLoading(true)
    setError(null)
    try {
      const params: any = {
        page: 1,
        pageSize: 50,
        upcoming: true, // Only get upcoming sessions
      }

      if (filters.subject) {
        params.subject = filters.subject
      }

      const response = await getSessions(params)
      let sessionsList = response.items || []

      // Filter by session type (1-on-1 vs Group)
      if (filters.sessionType) {
        if (filters.sessionType === '1-on-1') {
          sessionsList = sessionsList.filter(s => s.maxStudents === 1)
        } else if (filters.sessionType === 'Group') {
          sessionsList = sessionsList.filter(s => s.maxStudents > 1)
        }
      }

      // Filter by search query
      if (searchQuery.trim()) {
        const query = searchQuery.toLowerCase()
        sessionsList = sessionsList.filter(s =>
          (s.subject || s.subjectName || '')?.toLowerCase().includes(query) ||
          (s.title || '')?.toLowerCase().includes(query) ||
          s.tutorName?.toLowerCase().includes(query) ||
          (s.description || '')?.toLowerCase().includes(query)
        )
      }

      // Sort sessions
      if (sortBy === 'date') {
        sessionsList.sort((a, b) => {
          const aTime = (a as any).startTime || a.scheduledAt || ''
          const bTime = (b as any).startTime || b.scheduledAt || ''
          return new Date(aTime).getTime() - new Date(bTime).getTime()
        })
      } else if (sortBy === 'price-low') {
        sessionsList.sort((a, b) => (a.basePrice || 0) - (b.basePrice || 0))
      } else if (sortBy === 'availability') {
        // Sort by available spots (more spots first)
        sessionsList.sort((a, b) => {
          const aEnrolled = (a as any).enrolledStudents || a.currentStudents || 0
          const bEnrolled = (b as any).enrolledStudents || b.currentStudents || 0
          const aSpots = (a.maxStudents || 0) - aEnrolled
          const bSpots = (b.maxStudents || 0) - bEnrolled
          return bSpots - aSpots
        })
      }

      setSessions(sessionsList)
    } catch (err: any) {
      console.error('Failed to load sessions:', err)
      setError(err.message || 'Failed to load sessions')
      setSessions([])
    } finally {
      setIsLoading(false)
    }
  }

  const handleBookSession = (sessionId: string) => {
    if (!isAuthenticated) {
      if (confirm('Please login to book sessions. Would you like to login now?')) {
        navigate('/login')
      }
      return
    }
    navigate(`/sessions/${sessionId}/book`)
  }

  const handleApplyFilters = () => {
    loadSessions()
  }

  const handleClearFilters = () => {
    setFilters({
      sessionType: '',
      subject: '',
    })
    setSearchQuery('')
  }

  const formatDate = (dateString?: string) => {
    if (!dateString) return 'N/A'
    const date = new Date(dateString)
    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })
  }

  const formatTime = (dateString?: string) => {
    if (!dateString) return 'N/A'
    const date = new Date(dateString)
    return date.toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit' })
  }

  const getSessionType = (maxStudents: number) => {
    return maxStudents === 1 ? '1-on-1' : 'Group'
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Browse Sessions</h1>
        <p className="text-gray-600">Find group and private sessions created by tutors</p>
        {!isAuthenticated && (
          <div className="mt-4 p-4 bg-primary-50 border border-primary-200 rounded-lg">
            <p className="text-sm text-primary-800">
              <strong>Note:</strong> You can browse sessions without logging in. To book sessions, please{' '}
              <button
                onClick={() => navigate('/login')}
                className="underline font-semibold hover:text-primary-900"
              >
                login
              </button>
              {' '}or{' '}
              <button
                onClick={() => navigate('/register')}
                className="underline font-semibold hover:text-primary-900"
              >
                register
              </button>
              .
            </p>
          </div>
        )}
      </div>

      {/* Search and Filters */}
      <div className="mb-8">
        <div className="flex flex-col md:flex-row gap-4 mb-4">
          <div className="flex-1 relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
            <Input
              placeholder="Search sessions by title, tutor, or subject..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-10"
            />
          </div>
          <Button
            variant="outline"
            onClick={() => setShowFilters(!showFilters)}
            className="md:hidden"
          >
            <SlidersHorizontal className="w-5 h-5 mr-2" />
            Filters
          </Button>
        </div>

        {/* Filter Sidebar */}
        {showFilters && (
          <Card className="mb-4 md:mb-0 md:absolute md:left-4 md:w-64">
            <div className="p-4">
              <h3 className="font-semibold mb-4">Filters</h3>
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Session Type
                  </label>
                  <select
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg"
                    value={filters.sessionType}
                    onChange={(e) => setFilters({ ...filters, sessionType: e.target.value })}
                  >
                    <option value="">All Types</option>
                    <option value="1-on-1">1-on-1</option>
                    <option value="Group">Group</option>
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Subject
                  </label>
                  <select
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg"
                    value={filters.subject}
                    onChange={(e) => setFilters({ ...filters, subject: e.target.value })}
                  >
                    <option value="">All Subjects</option>
                    <option value="Web Development">Web Development</option>
                    <option value="Data Science">Data Science</option>
                    <option value="Design">Design</option>
                  </select>
                </div>
                <Button fullWidth onClick={handleApplyFilters}>Apply Filters</Button>
                <Button variant="ghost" fullWidth onClick={handleClearFilters}>
                  Clear
                </Button>
              </div>
            </div>
          </Card>
        )}

        {/* Desktop Filter Sidebar */}
        <div className="hidden md:block md:relative">
          <div className="md:absolute md:left-0 md:top-0 md:w-64">
            <Card>
              <div className="p-4">
                <h3 className="font-semibold mb-4">Filters</h3>
                <div className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Session Type
                    </label>
                    <select
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg"
                      value={filters.sessionType}
                      onChange={(e) => setFilters({ ...filters, sessionType: e.target.value })}
                    >
                      <option value="">All Types</option>
                      <option value="1-on-1">1-on-1</option>
                      <option value="Group">Group</option>
                    </select>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Subject
                    </label>
                    <select
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg"
                      value={filters.subject}
                      onChange={(e) => setFilters({ ...filters, subject: e.target.value })}
                    >
                      <option value="">All Subjects</option>
                      <option value="Web Development">Web Development</option>
                      <option value="Data Science">Data Science</option>
                      <option value="Design">Design</option>
                    </select>
                  </div>
                  <Button fullWidth onClick={handleApplyFilters}>Apply Filters</Button>
                  <Button variant="ghost" fullWidth onClick={handleClearFilters}>
                    Clear
                  </Button>
                </div>
              </div>
            </Card>
          </div>
        </div>
      </div>

      {/* Sessions Grid */}
      <div className="md:ml-72">
        <div className="flex items-center justify-between mb-6">
          <p className="text-gray-600">
            {isLoading ? (
              'Loading sessions...'
            ) : error ? (
              <span className="text-red-600">{error}</span>
            ) : (
              <>
                Found <span className="font-semibold">{sessions.length}</span> sessions
              </>
            )}
          </p>
          <select
            className="px-3 py-2 border border-gray-300 rounded-lg"
            value={sortBy}
            onChange={(e) => setSortBy(e.target.value)}
          >
            <option value="date">Sort by: Date</option>
            <option value="price-low">Sort by: Price (Low to High)</option>
            <option value="availability">Sort by: Availability</option>
          </select>
        </div>

        {isLoading ? (
          <div className="flex items-center justify-center py-12">
            <Loader2 className="w-8 h-8 animate-spin text-primary-600" />
          </div>
        ) : error ? (
          <div className="text-center py-12">
            <p className="text-red-600 mb-4">{error}</p>
            <Button onClick={loadSessions}>Try Again</Button>
          </div>
        ) : sessions.length === 0 ? (
          <div className="text-center py-12">
            <p className="text-gray-600">No sessions found. Try adjusting your filters.</p>
          </div>
        ) : (
          <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
            {sessions.map((session) => {
              const sessionType = getSessionType(session.maxStudents)
              const enrolledStudents = (session as any).enrolledStudents || session.currentStudents || 0
              const availableSpots = session.maxStudents - enrolledStudents
              const price = session.basePrice || 0
              const pricingLabel = session.pricingType === 'Hourly' ? '/hr' : ''
              const subject = session.subject || session.subjectName || 'General'
              const title = session.title || subject
              const timeString = (session as any).startTime || session.scheduledAt || ''

              return (
                <Card key={session.id} hover>
                  <div className="p-6">
                    <div className="flex items-start justify-between mb-4">
                      <div className="flex items-center gap-3">
                        <Avatar name={session.tutorName} size="md" />
                        <div>
                          <h3 className="font-semibold text-gray-900">{session.tutorName}</h3>
                          <p className="text-sm text-gray-600">{subject}</p>
                        </div>
                      </div>
                      <Badge variant={sessionType === '1-on-1' ? 'info' : 'success'}>
                        {sessionType}
                      </Badge>
                    </div>
                    <h4 className="text-lg font-semibold text-gray-900 mb-3">{title}</h4>
                    {session.description && (
                      <p className="text-sm text-gray-600 mb-3 line-clamp-2">{session.description}</p>
                    )}
                    <div className="space-y-2 mb-4">
                      {timeString && (
                        <div className="flex items-center gap-2 text-sm text-gray-600">
                          <Calendar className="w-4 h-4" />
                          <span>{formatDate(timeString)} at {formatTime(timeString)}</span>
                        </div>
                      )}
                      <div className="flex items-center gap-2 text-sm text-gray-600">
                        <Clock className="w-4 h-4" />
                        <span>Duration: {session.duration} min</span>
                      </div>
                      {sessionType === 'Group' && (
                        <div className="flex items-center gap-2 text-sm text-gray-600">
                          <Users className="w-4 h-4" />
                          <span>{enrolledStudents}/{session.maxStudents} students enrolled</span>
                        </div>
                      )}
                    </div>
                    <div className="flex items-center justify-between pt-4 border-t border-gray-200">
                      <div>
                        <span className="text-2xl font-bold text-gray-900">₹{price}{pricingLabel}</span>
                      </div>
                      <Button
                        size="sm"
                        onClick={() => handleBookSession(session.id)}
                        disabled={sessionType === 'Group' && availableSpots === 0}
                      >
                        {sessionType === 'Group' && availableSpots === 0 ? 'Full' : 'Book Now'}
                      </Button>
                    </div>
                  </div>
                </Card>
              )
            })}
          </div>
        )}
      </div>
    </div>
  )
}

export default Sessions
