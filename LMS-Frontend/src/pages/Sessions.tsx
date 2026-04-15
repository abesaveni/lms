import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { Search, Calendar, Users, Clock, Loader2, SlidersHorizontal } from 'lucide-react'
import Button from '../components/ui/Button'
import { Card } from '../components/ui/Card'
import { Badge } from '../components/ui/Badge'
import { Avatar } from '../components/ui/Avatar'
import { getSessions, SessionDto } from '../services/sessionsApi'
import { getCurrentUser } from '../utils/auth'

const Sessions = () => {
  const navigate = useNavigate()
  const [searchQuery, setSearchQuery] = useState('')
  const [sessions, setSessions] = useState<SessionDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [filters, setFilters] = useState({ sessionType: '', subject: '' })
  const [sortBy, setSortBy] = useState('date')
  const [showMobileFilters, setShowMobileFilters] = useState(false)
  const user = getCurrentUser()
  const isAuthenticated = !!user

  useEffect(() => { loadSessions() }, [searchQuery, filters, sortBy])
  useEffect(() => {
    const interval = setInterval(loadSessions, 30000)
    return () => clearInterval(interval)
  }, [searchQuery, filters, sortBy])

  const loadSessions = async () => {
    setIsLoading(true)
    setError(null)
    try {
      const response = await getSessions({ page: 1, pageSize: 50, upcoming: true })
      let list = response.items || []
      if (filters.sessionType === '1-on-1') list = list.filter(s => s.maxStudents === 1)
      else if (filters.sessionType === 'Group') list = list.filter(s => s.maxStudents > 1)
      if (filters.subject) list = list.filter(s => (s.subject || s.subjectName || '').toLowerCase() === filters.subject.toLowerCase())
      if (searchQuery.trim()) {
        const q = searchQuery.toLowerCase()
        list = list.filter(s =>
          (s.subject || s.subjectName || '').toLowerCase().includes(q) ||
          (s.title || '').toLowerCase().includes(q) ||
          s.tutorName?.toLowerCase().includes(q) ||
          (s.description || '').toLowerCase().includes(q)
        )
      }
      if (sortBy === 'date') list.sort((a, b) => new Date((a as any).startTime || a.scheduledAt || '').getTime() - new Date((b as any).startTime || b.scheduledAt || '').getTime())
      else if (sortBy === 'price-low') list.sort((a, b) => (a.basePrice || 0) - (b.basePrice || 0))
      setSessions(list)
    } catch (err: any) {
      setError(err.message || 'Failed to load sessions')
      setSessions([])
    } finally {
      setIsLoading(false)
    }
  }

  const handleBookSession = (sessionId: string) => {
    if (!isAuthenticated) { if (confirm('Please login to book sessions. Login now?')) navigate('/login'); return }
    navigate(`/sessions/${sessionId}/book`)
  }

  const formatDate = (d?: string) => d ? new Date(d).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' }) : 'N/A'
  const formatTime = (d?: string) => d ? new Date(d).toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit' }) : 'N/A'
  const getSessionType = (max: number) => max === 1 ? '1-on-1' : 'Group'

  const FilterPanel = () => (
    <div className="space-y-5">
      <div>
        <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wide mb-2">Session Type</label>
        <select
          className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg bg-white focus:outline-none focus:ring-2 focus:ring-primary-300"
          value={filters.sessionType}
          onChange={(e) => setFilters({ ...filters, sessionType: e.target.value })}
        >
          <option value="">All Types</option>
          <option value="1-on-1">1-on-1</option>
          <option value="Group">Group</option>
        </select>
      </div>
      <div>
        <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wide mb-2">Subject</label>
        <select
          className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg bg-white focus:outline-none focus:ring-2 focus:ring-primary-300"
          value={filters.subject}
          onChange={(e) => setFilters({ ...filters, subject: e.target.value })}
        >
          <option value="">All Subjects</option>
          <option value="Web Development">Web Development</option>
          <option value="Data Science">Data Science</option>
          <option value="Design">Design</option>
          <option value="Mathematics">Mathematics</option>
          <option value="Python Programming">Python Programming</option>
        </select>
      </div>
      <button
        onClick={() => setFilters({ sessionType: '', subject: '' })}
        className="w-full text-xs text-gray-400 hover:text-gray-600 transition-colors py-1"
      >
        Clear filters
      </button>
    </div>
  )

  return (
    <div className="bg-gray-50 min-h-screen">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">

        {/* Page Header */}
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-gray-900">Browse Sessions</h1>
          <p className="text-sm text-gray-500 mt-1">Find group and private sessions created by tutors</p>
        </div>

        {/* Search bar + sort row */}
        <div className="flex flex-col sm:flex-row gap-3 mb-6">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
            <input
              type="text"
              placeholder="Search by title, tutor, or subject..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full pl-9 pr-4 py-2.5 text-sm border border-gray-200 rounded-lg bg-white focus:outline-none focus:ring-2 focus:ring-primary-300"
            />
          </div>
          <select
            className="px-3 py-2.5 text-sm border border-gray-200 rounded-lg bg-white focus:outline-none focus:ring-2 focus:ring-primary-300"
            value={sortBy}
            onChange={(e) => setSortBy(e.target.value)}
          >
            <option value="date">Sort: Date</option>
            <option value="price-low">Sort: Price (Low–High)</option>
            <option value="availability">Sort: Availability</option>
          </select>
          <button
            className="sm:hidden flex items-center gap-2 px-4 py-2.5 text-sm font-medium border border-gray-200 rounded-lg bg-white"
            onClick={() => setShowMobileFilters(!showMobileFilters)}
          >
            <SlidersHorizontal className="w-4 h-4" /> Filters
          </button>
        </div>

        {/* Mobile filters */}
        {showMobileFilters && (
          <div className="sm:hidden bg-white rounded-lg border border-gray-200 p-4 mb-4">
            <FilterPanel />
          </div>
        )}

        {/* Main layout: sidebar + grid */}
        <div className="flex gap-6">

          {/* Sidebar — desktop only */}
          <aside className="hidden sm:block w-56 flex-shrink-0">
            <div className="bg-white rounded-xl border border-gray-200 p-5 sticky top-24">
              <h3 className="text-sm font-semibold text-gray-800 mb-4">Filters</h3>
              <FilterPanel />
            </div>
          </aside>

          {/* Content */}
          <div className="flex-1 min-w-0">
            {/* Result count */}
            <p className="text-sm text-gray-500 mb-4">
              {isLoading ? 'Loading...' : error ? <span className="text-red-500">{error}</span> : <><span className="font-semibold text-gray-800">{sessions.length}</span> sessions found</>}
            </p>

            {isLoading ? (
              <div className="flex items-center justify-center py-20">
                <Loader2 className="w-7 h-7 animate-spin text-primary-500" />
              </div>
            ) : error ? (
              <div className="text-center py-16">
                <p className="text-red-500 text-sm mb-3">{error}</p>
                <Button size="sm" onClick={loadSessions}>Retry</Button>
              </div>
            ) : sessions.length === 0 ? (
              <div className="text-center py-20 bg-white rounded-xl border border-dashed border-gray-200">
                <Search className="w-8 h-8 text-gray-300 mx-auto mb-3" />
                <p className="text-sm font-medium text-gray-700">No sessions found</p>
                <p className="text-xs text-gray-400 mt-1">Try adjusting your filters or search query</p>
              </div>
            ) : (
              <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
                {sessions.map((session) => {
                  const type = getSessionType(session.maxStudents)
                  const enrolled = (session as any).enrolledStudents || session.currentStudents || 0
                  const spots = session.maxStudents - enrolled
                  const price = session.basePrice || 0
                  const subject = session.subject || session.subjectName || 'General'
                  const title = session.title || subject
                  const timeStr = (session as any).startTime || session.scheduledAt || ''

                  return (
                    <Card key={session.id} hover>
                      <div className="p-5">
                        <div className="flex items-center justify-between mb-3">
                          <div className="flex items-center gap-2.5">
                            <Avatar name={session.tutorName} size="sm" />
                            <div>
                              <p className="text-sm font-semibold text-gray-900 leading-tight">{session.tutorName}</p>
                              <p className="text-xs text-gray-500">{subject}</p>
                            </div>
                          </div>
                          <Badge variant={type === '1-on-1' ? 'info' : 'success'} className="text-xs">{type}</Badge>
                        </div>

                        <h4 className="text-sm font-semibold text-gray-900 mb-3 line-clamp-1">{title}</h4>

                        {session.description && (
                          <p className="text-xs text-gray-500 mb-3 line-clamp-2">{session.description}</p>
                        )}

                        <div className="space-y-1.5 mb-4">
                          {timeStr && (
                            <div className="flex items-center gap-2 text-xs text-gray-500">
                              <Calendar className="w-3.5 h-3.5 flex-shrink-0" />
                              <span>{formatDate(timeStr)} at {formatTime(timeStr)}</span>
                            </div>
                          )}
                          <div className="flex items-center gap-2 text-xs text-gray-500">
                            <Clock className="w-3.5 h-3.5 flex-shrink-0" />
                            <span>{session.duration} min</span>
                          </div>
                          {type === 'Group' && (
                            <div className="flex items-center gap-2 text-xs text-gray-500">
                              <Users className="w-3.5 h-3.5 flex-shrink-0" />
                              <span>{enrolled}/{session.maxStudents} enrolled</span>
                            </div>
                          )}
                        </div>

                        <div className="flex items-center justify-between pt-3 border-t border-gray-100">
                          <span className="text-base font-bold text-gray-900">₹{price}{session.pricingType === 'Hourly' ? '/hr' : ''}</span>
                          <Button
                            size="sm"
                            onClick={() => handleBookSession(session.id)}
                            disabled={type === 'Group' && spots === 0}
                          >
                            {type === 'Group' && spots === 0 ? 'Full' : 'Book Now'}
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
      </div>
    </div>
  )
}

export default Sessions
