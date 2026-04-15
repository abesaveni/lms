import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { Search, SlidersHorizontal, Loader2 } from 'lucide-react'
import Button from '../components/ui/Button'
import Input from '../components/ui/Input'
import { TutorCard } from '../components/domain/TutorCard'
import { Card } from '../components/ui/Card'
import { getTutors, TutorDto } from '../services/tutorsApi'
import { getCurrentUser } from '../utils/auth'

const FindTutors = () => {
  const navigate = useNavigate()
  const [searchQuery, setSearchQuery] = useState('')
  const [showMobileFilters, setShowMobileFilters] = useState(false)
  const [tutors, setTutors] = useState<TutorDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [filters, setFilters] = useState({
    subject: '',
    minPrice: '',
    maxPrice: '',
    minRating: '',
  })
  const [sortBy, setSortBy] = useState('rating')
  const user = getCurrentUser()
  const isAuthenticated = !!user

  useEffect(() => { loadTutors() }, [searchQuery, filters, sortBy])
  useEffect(() => {
    const interval = setInterval(() => loadTutors(), 30000)
    return () => clearInterval(interval)
  }, [searchQuery, filters, sortBy])

  const loadTutors = async () => {
    setIsLoading(true)
    setError(null)
    try {
      const response = await getTutors({ page: 1, pageSize: 50 })
      let list = response.items || []

      list = list.filter(t => t.verificationStatus === 'Approved')

      if (searchQuery.trim()) {
        const q = searchQuery.toLowerCase()
        list = list.filter(t =>
          t.name?.toLowerCase().includes(q) ||
          t.headline?.toLowerCase().includes(q) ||
          t.bio?.toLowerCase().includes(q)
        )
      }

      if (filters.subject) {
        const s = filters.subject.toLowerCase()
        list = list.filter(t =>
          t.subjects?.some(sub => sub.toLowerCase() === s) ||
          t.headline?.toLowerCase().includes(s)
        )
      }

      if (filters.minPrice) {
        const min = parseFloat(filters.minPrice)
        if (!isNaN(min)) list = list.filter(t => t.hourlyRate >= min)
      }

      if (filters.maxPrice) {
        const max = parseFloat(filters.maxPrice)
        if (!isNaN(max)) list = list.filter(t => t.hourlyRate <= max)
      }

      if (filters.minRating) {
        const minR = parseFloat(filters.minRating)
        if (!isNaN(minR)) list = list.filter(t => (t.averageRating || 0) >= minR)
      }

      if (sortBy === 'rating') list.sort((a, b) => (b.averageRating || 0) - (a.averageRating || 0))
      else if (sortBy === 'price-low') list.sort((a, b) => a.hourlyRate - b.hourlyRate)
      else if (sortBy === 'price-high') list.sort((a, b) => b.hourlyRate - a.hourlyRate)
      else if (sortBy === 'availability') list.sort((a, b) => (b.available ? 1 : 0) - (a.available ? 1 : 0))

      setTutors(list)
    } catch (err: any) {
      setError(err.message || 'Failed to load tutors')
      setTutors([])
    } finally {
      setIsLoading(false)
    }
  }

  const handleViewProfile = (tutor: TutorDto) => {
    if (isAuthenticated) {
      navigate(`/student/tutors/${tutor.userId}`)
    } else {
      if (confirm('Please login to view tutor profiles and book sessions. Would you like to login now?')) {
        navigate('/login')
      }
    }
  }

  const handleClearFilters = () => {
    setFilters({ subject: '', minPrice: '', maxPrice: '', minRating: '' })
    setSearchQuery('')
  }

  const FilterPanel = () => (
    <div className="space-y-5">
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
          <option value="Machine Learning">Machine Learning</option>
          <option value="Cloud Computing">Cloud Computing</option>
        </select>
      </div>
      <div>
        <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wide mb-2">Price Range</label>
        <div className="flex gap-2">
          <input
            type="number"
            placeholder="Min"
            value={filters.minPrice}
            onChange={(e) => setFilters({ ...filters, minPrice: e.target.value })}
            className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg bg-white focus:outline-none focus:ring-2 focus:ring-primary-300"
          />
          <input
            type="number"
            placeholder="Max"
            value={filters.maxPrice}
            onChange={(e) => setFilters({ ...filters, maxPrice: e.target.value })}
            className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg bg-white focus:outline-none focus:ring-2 focus:ring-primary-300"
          />
        </div>
      </div>
      <div>
        <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wide mb-2">Rating</label>
        <select
          className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg bg-white focus:outline-none focus:ring-2 focus:ring-primary-300"
          value={filters.minRating}
          onChange={(e) => setFilters({ ...filters, minRating: e.target.value })}
        >
          <option value="">Any Rating</option>
          <option value="4.5">4.5+ Stars</option>
          <option value="4.0">4.0+ Stars</option>
          <option value="3.5">3.5+ Stars</option>
        </select>
      </div>
      <button
        onClick={handleClearFilters}
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
          <h1 className="text-2xl font-bold text-gray-900">Find Your Perfect Tutor</h1>
          <p className="text-sm text-gray-500 mt-1">Browse through our verified expert tutors</p>
          {!isAuthenticated && (
            <div className="mt-3 p-3 bg-primary-50 border border-primary-200 rounded-lg">
              <p className="text-sm text-primary-800">
                Browse tutors without logging in. To book sessions,{' '}
                <button onClick={() => navigate('/login')} className="underline font-semibold hover:text-primary-900">login</button>
                {' '}or{' '}
                <button onClick={() => navigate('/register')} className="underline font-semibold hover:text-primary-900">register</button>.
              </p>
            </div>
          )}
        </div>

        {/* Search bar + sort row */}
        <div className="flex flex-col sm:flex-row gap-3 mb-6">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
            <input
              type="text"
              placeholder="Search by name, subject, or expertise..."
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
            <option value="rating">Sort: Rating</option>
            <option value="price-low">Sort: Price (Low–High)</option>
            <option value="price-high">Sort: Price (High–Low)</option>
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
            <div className="flex items-center justify-between mb-4">
              <p className="text-sm text-gray-500">
                {isLoading ? 'Loading...' : error
                  ? <span className="text-red-500">{error}</span>
                  : <><span className="font-semibold text-gray-800">{tutors.length}</span> tutors found</>
                }
              </p>
            </div>

            {isLoading ? (
              <div className="flex items-center justify-center py-20">
                <Loader2 className="w-7 h-7 animate-spin text-primary-500" />
              </div>
            ) : error ? (
              <div className="text-center py-16">
                <p className="text-red-500 text-sm mb-3">{error}</p>
                <Button size="sm" onClick={loadTutors}>Retry</Button>
              </div>
            ) : tutors.length === 0 ? (
              <div className="text-center py-20 bg-white rounded-xl border border-dashed border-gray-200">
                <Search className="w-8 h-8 text-gray-300 mx-auto mb-3" />
                <p className="text-sm font-medium text-gray-700">No tutors found</p>
                <p className="text-xs text-gray-400 mt-1">Try adjusting your filters or search query</p>
              </div>
            ) : (
              <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
                {tutors.map((tutor) => (
                  <TutorCard
                    key={tutor.id}
                    name={tutor.name}
                    rating={tutor.averageRating || 0}
                    reviews={tutor.totalReviews || 0}
                    followers={tutor.followerCount || 0}
                    subjects={tutor.subjects || []}
                    price={tutor.hourlyRate}
                    location={tutor.location}
                    available={tutor.available !== false}
                    onViewProfile={() => handleViewProfile(tutor)}
                  />
                ))}
              </div>
            )}
          </div>
        </div>

      </div>
    </div>
  )
}

export default FindTutors
