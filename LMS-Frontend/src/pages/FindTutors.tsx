import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { Search, SlidersHorizontal, Loader2, ChevronDown, Sparkles, GraduationCap, Users, BookOpen, Star } from 'lucide-react'
import { motion, AnimatePresence } from 'framer-motion'
import Button from '../components/ui/Button'
import { TutorCard } from '../components/domain/TutorCard'
import { getTutors, TutorDto } from '../services/tutorsApi'
import { getCurrentUser } from '../utils/auth'

const CATEGORIES = [
  { label: 'All', value: '' },
  { label: '💻 Tech & Coding', value: 'Web Development' },
  { label: '🧮 Mathematics', value: 'Mathematics' },
  { label: '🔬 Science', value: 'Physics' },
  { label: '🤖 AI & ML', value: 'Machine Learning' },
  { label: '📊 Data Science', value: 'Data Science' },
  { label: '☁️ Cloud', value: 'Cloud Computing' },
  { label: '🎨 Design', value: 'Design' },
  { label: '💼 Business', value: 'Business Studies' },
]

const FindTutors = () => {
  const navigate = useNavigate()
  const [searchQuery, setSearchQuery] = useState('')
  const [showMobileFilters, setShowMobileFilters] = useState(false)
  const [tutors, setTutors] = useState<TutorDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [selectedCategory, setSelectedCategory] = useState('')
  const [filters, setFilters] = useState({
    subject: '',
    minPrice: '',
    maxPrice: '',
    minRating: '',
  })
  const [sortBy, setSortBy] = useState('rating')
  const user = getCurrentUser()
  const isAuthenticated = !!user

  useEffect(() => { loadTutors() }, [searchQuery, filters, sortBy, selectedCategory])
  useEffect(() => {
    const interval = setInterval(() => loadTutors(), 30000)
    return () => clearInterval(interval)
  }, [searchQuery, filters, sortBy, selectedCategory])

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

      const activeSubject = selectedCategory || filters.subject
      if (activeSubject) {
        const s = activeSubject.toLowerCase()
        list = list.filter(t =>
          t.subjects?.some(sub => sub.toLowerCase().includes(s)) ||
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
      else if (sortBy === 'sessions') list.sort((a, b) => (b.totalSessions || 0) - (a.totalSessions || 0))

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
    setSelectedCategory('')
  }

  // Stats derived from loaded tutors
  const avgRating = tutors.length
    ? (tutors.reduce((s, t) => s + (t.averageRating || 0), 0) / tutors.length).toFixed(1)
    : '4.8'
  const totalSessions = tutors.reduce((s, t) => s + (t.totalSessions || 0), 0)

  const FilterPanel = () => (
    <div className="space-y-5">
      <div>
        <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wide mb-2">Subject</label>
        <select
          className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg bg-white focus:outline-none focus:ring-2 focus:ring-indigo-300"
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
          <option value="Physics">Physics</option>
          <option value="Business Studies">Business Studies</option>
        </select>
      </div>
      <div>
        <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wide mb-2">Price Range (₹/hr)</label>
        <div className="flex gap-2">
          <input type="number" placeholder="Min" value={filters.minPrice}
            onChange={(e) => setFilters({ ...filters, minPrice: e.target.value })}
            className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg bg-white focus:outline-none focus:ring-2 focus:ring-indigo-300" />
          <input type="number" placeholder="Max" value={filters.maxPrice}
            onChange={(e) => setFilters({ ...filters, maxPrice: e.target.value })}
            className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg bg-white focus:outline-none focus:ring-2 focus:ring-indigo-300" />
        </div>
      </div>
      <div>
        <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wide mb-2">Minimum Rating</label>
        <select
          className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg bg-white focus:outline-none focus:ring-2 focus:ring-indigo-300"
          value={filters.minRating}
          onChange={(e) => setFilters({ ...filters, minRating: e.target.value })}
        >
          <option value="">Any Rating</option>
          <option value="4.5">4.5+ Stars</option>
          <option value="4.0">4.0+ Stars</option>
          <option value="3.5">3.5+ Stars</option>
        </select>
      </div>
      <button onClick={handleClearFilters} className="w-full text-xs text-indigo-500 hover:text-indigo-700 transition-colors py-1 font-semibold">
        Clear all filters
      </button>
    </div>
  )

  return (
    <div className="min-h-screen bg-gray-50">

      {/* ── Hero Section ───────────────────────────────── */}
      <div className="relative overflow-hidden bg-gradient-to-br from-indigo-600 via-indigo-700 to-purple-800">
        {/* Decorative blobs */}
        <div className="absolute top-0 left-0 w-64 h-64 bg-white/5 rounded-full -translate-x-1/2 -translate-y-1/2" />
        <div className="absolute bottom-0 right-0 w-96 h-96 bg-purple-500/20 rounded-full translate-x-1/3 translate-y-1/3" />
        <div className="absolute top-1/2 left-1/2 w-48 h-48 bg-indigo-400/10 rounded-full -translate-x-1/2 -translate-y-1/2" />

        <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-14">
          <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} className="text-center">
            <div className="inline-flex items-center gap-2 bg-white/10 backdrop-blur-sm border border-white/20 rounded-full px-4 py-1.5 text-white text-xs font-semibold mb-5">
              <Sparkles className="w-3.5 h-3.5 text-yellow-300" />
              Expert tutors ready to help you grow
            </div>
            <h1 className="text-4xl sm:text-5xl font-black text-white leading-tight mb-4">
              Find Your Perfect<br />
              <span className="text-yellow-300">Learning Match</span>
            </h1>
            <p className="text-indigo-200 text-base max-w-xl mx-auto mb-8">
              Connect with verified expert tutors for 1-on-1 sessions, group classes, and personalized learning journeys.
            </p>

            {/* Hero Search Bar */}
            <div className="max-w-2xl mx-auto relative">
              <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
              <input
                type="text"
                placeholder="Search by name, subject, or expertise..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="w-full pl-12 pr-32 py-4 text-sm rounded-2xl bg-white shadow-xl focus:outline-none focus:ring-2 focus:ring-white/50 text-gray-800 placeholder-gray-400 border-0"
              />
              <Button
                className="absolute right-2 top-1/2 -translate-y-1/2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white font-bold px-5 py-2"
                onClick={() => loadTutors()}
              >
                Search
              </Button>
            </div>
          </motion.div>

          {/* Stat Pills */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.15 }}
            className="flex flex-wrap justify-center gap-6 mt-10"
          >
            {[
              { icon: <GraduationCap className="w-4 h-4" />, label: 'Expert Tutors', value: tutors.length || '100+' },
              { icon: <BookOpen className="w-4 h-4" />, label: 'Sessions Delivered', value: totalSessions > 0 ? `${totalSessions}+` : '5,000+' },
              { icon: <Star className="w-4 h-4 fill-yellow-300 text-yellow-300" />, label: 'Avg. Rating', value: avgRating },
              { icon: <Users className="w-4 h-4" />, label: 'Happy Students', value: '2,500+' },
            ].map((stat, i) => (
              <div key={i} className="flex items-center gap-2 text-white/90">
                <div className="text-white/60">{stat.icon}</div>
                <span className="text-xl font-black">{stat.value}</span>
                <span className="text-sm text-white/60">{stat.label}</span>
              </div>
            ))}
          </motion.div>
        </div>
      </div>

      {/* ── Auth Banner (unauthenticated) ─────────────── */}
      {!isAuthenticated && (
        <div className="bg-indigo-50 border-b border-indigo-100">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-3 flex items-center justify-between gap-4">
            <p className="text-sm text-indigo-800">
              <span className="font-semibold">Browse freely.</span> Login to book sessions and message tutors.
            </p>
            <div className="flex items-center gap-3 flex-shrink-0">
              <button onClick={() => navigate('/login')} className="text-sm font-semibold text-indigo-700 hover:text-indigo-900 underline">Login</button>
              <button onClick={() => navigate('/register')} className="text-sm font-bold text-white bg-indigo-600 hover:bg-indigo-700 px-4 py-1.5 rounded-lg transition-colors">Register Free</button>
            </div>
          </div>
        </div>
      )}

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">

        {/* ── Category Quick-Filter Pills ───────────────── */}
        <div className="flex flex-wrap gap-2 mb-6">
          {CATEGORIES.map((cat) => (
            <button
              key={cat.value}
              onClick={() => { setSelectedCategory(cat.value); setFilters(f => ({ ...f, subject: '' })) }}
              className={`px-4 py-1.5 rounded-full text-xs font-bold transition-all border ${
                selectedCategory === cat.value
                  ? 'bg-indigo-600 text-white border-indigo-600 shadow-sm'
                  : 'bg-white text-gray-600 border-gray-200 hover:border-indigo-300 hover:text-indigo-600'
              }`}
            >
              {cat.label}
            </button>
          ))}
        </div>

        {/* ── Search + Sort Row ─────────────────────────── */}
        <div className="flex flex-col sm:flex-row gap-3 mb-6">
          <div className="relative flex-1 sm:hidden">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
            <input
              type="text"
              placeholder="Search tutors..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full pl-9 pr-4 py-2.5 text-sm border border-gray-200 rounded-xl bg-white focus:outline-none focus:ring-2 focus:ring-indigo-300"
            />
          </div>
          <div className="relative ml-auto flex items-center gap-2">
            <select
              className="pl-4 pr-8 py-2.5 text-xs font-semibold border border-gray-200 rounded-xl bg-white focus:outline-none focus:ring-2 focus:ring-indigo-300 appearance-none cursor-pointer"
              value={sortBy}
              onChange={(e) => setSortBy(e.target.value)}
            >
              <option value="rating">Top Rated</option>
              <option value="sessions">Most Sessions</option>
              <option value="price-low">Price: Low → High</option>
              <option value="price-high">Price: High → Low</option>
            </select>
            <ChevronDown className="absolute right-2 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-gray-400 pointer-events-none" />
            <button
              className="sm:hidden flex items-center gap-2 px-4 py-2.5 text-xs font-semibold border border-gray-200 rounded-xl bg-white"
              onClick={() => setShowMobileFilters(!showMobileFilters)}
            >
              <SlidersHorizontal className="w-3.5 h-3.5" /> Filters
            </button>
          </div>
        </div>

        {/* Mobile filters */}
        {showMobileFilters && (
          <div className="sm:hidden bg-white rounded-xl border border-gray-200 p-4 mb-4 shadow-sm">
            <FilterPanel />
          </div>
        )}

        {/* Main layout */}
        <div className="flex gap-6">

          {/* Sidebar */}
          <aside className="hidden sm:block w-56 flex-shrink-0">
            <div className="bg-white rounded-2xl border border-gray-100 p-5 sticky top-24 shadow-sm">
              <h3 className="text-sm font-bold text-gray-800 mb-4 flex items-center gap-2">
                <SlidersHorizontal className="w-3.5 h-3.5 text-indigo-500" />
                Filters
              </h3>
              <FilterPanel />
            </div>
          </aside>

          {/* Results */}
          <div className="flex-1 min-w-0">
            <div className="flex items-center justify-between mb-4">
              <p className="text-sm text-gray-500">
                {isLoading ? (
                  <span className="text-gray-400">Loading tutors...</span>
                ) : error ? (
                  <span className="text-red-500">{error}</span>
                ) : (
                  <><span className="font-bold text-gray-900">{tutors.length}</span> tutors found</>
                )}
              </p>
            </div>

            {isLoading ? (
              <div className="flex flex-col items-center justify-center py-24 gap-3">
                <Loader2 className="w-8 h-8 animate-spin text-indigo-500" />
                <p className="text-xs font-semibold text-gray-400 uppercase tracking-widest">Finding expert tutors...</p>
              </div>
            ) : error ? (
              <div className="text-center py-16 bg-white rounded-2xl border border-dashed border-red-100">
                <p className="text-red-500 text-sm mb-3">{error}</p>
                <Button size="sm" onClick={loadTutors}>Retry</Button>
              </div>
            ) : tutors.length === 0 ? (
              <div className="text-center py-20 bg-white rounded-2xl border border-dashed border-gray-200">
                <Search className="w-10 h-10 text-gray-200 mx-auto mb-4" />
                <p className="text-sm font-bold text-gray-700">No tutors found</p>
                <p className="text-xs text-gray-400 mt-1 mb-4">Try adjusting your search or filters</p>
                <Button size="sm" variant="outline" onClick={handleClearFilters}>Clear Filters</Button>
              </div>
            ) : (
              <AnimatePresence mode="popLayout">
                <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
                  {tutors.map((tutor, i) => (
                    <motion.div
                      key={tutor.id}
                      layout
                      initial={{ opacity: 0, y: 20 }}
                      animate={{ opacity: 1, y: 0 }}
                      exit={{ opacity: 0, scale: 0.95 }}
                      transition={{ duration: 0.25, delay: Math.min(i * 0.04, 0.4) }}
                    >
                      <TutorCard
                        name={tutor.name}
                        headline={tutor.headline}
                        rating={tutor.averageRating || 0}
                        reviews={tutor.totalReviews || 0}
                        followers={tutor.followerCount || 0}
                        subjects={tutor.subjects || []}
                        price={tutor.hourlyRate}
                        avatar={tutor.profileImage}
                        available={tutor.available !== false}
                        yearsOfExperience={tutor.yearsOfExperience}
                        totalSessions={tutor.totalSessions}
                        isVerified={tutor.verificationStatus === 'Approved'}
                        hasBackgroundCheck={tutor.hasBackgroundCheck}
                        trialAvailable={tutor.trialAvailable}
                        trialPrice={tutor.trialPrice}
                        onViewProfile={() => handleViewProfile(tutor)}
                      />
                    </motion.div>
                  ))}
                </div>
              </AnimatePresence>
            )}
          </div>
        </div>

      </div>
    </div>
  )
}

export default FindTutors
