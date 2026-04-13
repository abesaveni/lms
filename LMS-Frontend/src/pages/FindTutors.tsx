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
  const [showFilters, setShowFilters] = useState(false)
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

  // Load tutors on mount and when filters change
  useEffect(() => {
    loadTutors()
  }, [searchQuery, filters, sortBy])

  // Real-time updates: Poll every 30 seconds
  useEffect(() => {
    const interval = setInterval(() => {
      loadTutors()
    }, 30000) // Refresh every 30 seconds

    return () => clearInterval(interval)
  }, [searchQuery, filters, sortBy])

  const loadTutors = async () => {
    setIsLoading(true)
    setError(null)
    try {
      const params: any = {
        page: 1,
        pageSize: 50,
      }
      
      if (searchQuery.trim()) {
        params.search = searchQuery.trim()
      }
      
      if (filters.subject) {
        params.subject = filters.subject
      }
      
      if (filters.minPrice) {
        params.minPrice = parseFloat(filters.minPrice)
      }
      
      if (filters.maxPrice) {
        params.maxPrice = parseFloat(filters.maxPrice)
      }
      
      if (filters.minRating) {
        params.minRating = parseFloat(filters.minRating)
      }

      const response = await getTutors(params)
      let tutorsList = response.items || []

      // Filter by verification status (only show approved tutors)
      tutorsList = tutorsList.filter(t => t.verificationStatus === 'Approved')

      // Sort tutors
      if (sortBy === 'rating') {
        tutorsList.sort((a, b) => (b.averageRating || 0) - (a.averageRating || 0))
      } else if (sortBy === 'price-low') {
        tutorsList.sort((a, b) => a.hourlyRate - b.hourlyRate)
      } else if (sortBy === 'price-high') {
        tutorsList.sort((a, b) => b.hourlyRate - a.hourlyRate)
      } else if (sortBy === 'availability') {
        tutorsList.sort((a, b) => {
          const aAvailable = a.available ? 1 : 0
          const bAvailable = b.available ? 1 : 0
          return bAvailable - aAvailable
        })
      }

      setTutors(tutorsList)
    } catch (err: any) {
      console.error('Failed to load tutors:', err)
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

  const handleApplyFilters = () => {
    loadTutors()
  }

  const handleClearFilters = () => {
    setFilters({
      subject: '',
      minPrice: '',
      maxPrice: '',
      minRating: '',
    })
    setSearchQuery('')
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Find Your Perfect Tutor</h1>
        <p className="text-gray-600">Browse through our verified expert tutors</p>
        {!isAuthenticated && (
          <div className="mt-4 p-4 bg-primary-50 border border-primary-200 rounded-lg">
            <p className="text-sm text-primary-800">
              <strong>Note:</strong> You can browse tutors without logging in. To book sessions, please{' '}
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
              placeholder="Search by name, subject, or expertise..."
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

        {/* Filter Sidebar (Mobile) */}
        {showFilters && (
          <Card className="mb-4 md:mb-0 md:absolute md:left-4 md:w-64">
            <div className="p-4">
              <h3 className="font-semibold mb-4">Filters</h3>
              <div className="space-y-4">
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
                    <option value="Programming">Programming</option>
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Price Range
                  </label>
                  <div className="flex gap-2">
                    <Input
                      type="number"
                      placeholder="Min"
                      className="flex-1"
                      value={filters.minPrice}
                      onChange={(e) => setFilters({ ...filters, minPrice: e.target.value })}
                    />
                    <Input
                      type="number"
                      placeholder="Max"
                      className="flex-1"
                      value={filters.maxPrice}
                      onChange={(e) => setFilters({ ...filters, maxPrice: e.target.value })}
                    />
                  </div>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Rating
                  </label>
                  <select
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg"
                    value={filters.minRating}
                    onChange={(e) => setFilters({ ...filters, minRating: e.target.value })}
                  >
                    <option value="">Any Rating</option>
                    <option value="4.5">4.5+ Stars</option>
                    <option value="4.0">4.0+ Stars</option>
                    <option value="3.5">3.5+ Stars</option>
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
                      <option value="Programming">Programming</option>
                    </select>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Price Range
                    </label>
                    <div className="flex gap-2">
                      <Input
                        type="number"
                        placeholder="Min"
                        className="flex-1"
                        value={filters.minPrice}
                        onChange={(e) => setFilters({ ...filters, minPrice: e.target.value })}
                      />
                      <Input
                        type="number"
                        placeholder="Max"
                        className="flex-1"
                        value={filters.maxPrice}
                        onChange={(e) => setFilters({ ...filters, maxPrice: e.target.value })}
                      />
                    </div>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Rating
                    </label>
                    <select
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg"
                      value={filters.minRating}
                      onChange={(e) => setFilters({ ...filters, minRating: e.target.value })}
                    >
                      <option value="">Any Rating</option>
                      <option value="4.5">4.5+ Stars</option>
                      <option value="4.0">4.0+ Stars</option>
                      <option value="3.5">3.5+ Stars</option>
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

      {/* Tutors Grid */}
      <div className="md:ml-72">
        <div className="flex items-center justify-between mb-6">
          <p className="text-gray-600">
            {isLoading ? (
              'Loading tutors...'
            ) : error ? (
              <span className="text-red-600">{error}</span>
            ) : (
              <>
                Found <span className="font-semibold">{tutors.length}</span> tutors
              </>
            )}
          </p>
          <select
            className="px-3 py-2 border border-gray-300 rounded-lg"
            value={sortBy}
            onChange={(e) => setSortBy(e.target.value)}
          >
            <option value="rating">Sort by: Rating</option>
            <option value="price-low">Sort by: Price (Low to High)</option>
            <option value="price-high">Sort by: Price (High to Low)</option>
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
            <Button onClick={loadTutors}>Try Again</Button>
          </div>
        ) : tutors.length === 0 ? (
          <div className="text-center py-12">
            <p className="text-gray-600">No tutors found. Try adjusting your filters.</p>
          </div>
        ) : (
          <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
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
  )
}

export default FindTutors
