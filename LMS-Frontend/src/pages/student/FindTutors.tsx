import { useState, useEffect, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { Search, Users, Loader2, ArrowUpDown, ChevronDown, Sparkles, TrendingUp } from 'lucide-react'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'
import { TutorCard } from '../../components/domain/TutorCard'
import { getTutors, TutorDto } from '../../services/tutorsApi'
import { motion, AnimatePresence } from 'framer-motion'
import { Badge } from '../../components/ui/Badge'

const CATEGORIES = [
  { label: 'All', value: 'All Subjects' },
  { label: '💻 Coding', value: 'Web Development' },
  { label: '🧮 Math', value: 'Mathematics' },
  { label: '🔬 Science', value: 'Physics' },
  { label: '🤖 AI/ML', value: 'Machine Learning' },
  { label: '📊 Data', value: 'Data Science' },
  { label: '☁️ Cloud', value: 'Cloud Computing' },
  { label: '🎨 Design', value: 'Design' },
  { label: '💼 Business', value: 'Business Studies' },
]

const FindTutors = () => {
  const navigate = useNavigate()

  const [tutors, setTutors] = useState<TutorDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [searchQuery, setSearchQuery] = useState('')
  const [subject, setSubject] = useState('All Subjects')
  const [maxPrice, setMaxPrice] = useState('')
  const [sortBy, setSortBy] = useState('Rating')

  const fetchTutors = useCallback(async () => {
    setIsLoading(true)
    setError(null)
    try {
      const response = await getTutors({ page: 1, pageSize: 50 })
      let items = [...response.items]

      if (searchQuery) {
        const query = searchQuery.toLowerCase()
        items = items.filter(tutor =>
          (tutor.name && tutor.name.toLowerCase().includes(query)) ||
          (tutor.headline && tutor.headline.toLowerCase().includes(query)) ||
          (tutor.bio && tutor.bio.toLowerCase().includes(query))
        )
      }

      if (subject !== 'All Subjects') {
        const lowerSubject = subject.toLowerCase()
        items = items.filter(tutor =>
          (tutor.subjects && tutor.subjects.some(s => s.toLowerCase().includes(lowerSubject))) ||
          (tutor.headline && tutor.headline.toLowerCase().includes(lowerSubject)) ||
          (tutor.bio && tutor.bio.toLowerCase().includes(lowerSubject))
        )
      }

      if (maxPrice) {
        const pLimit = parseFloat(maxPrice)
        if (!isNaN(pLimit)) {
          items = items.filter(tutor => tutor.hourlyRate <= pLimit)
        }
      }

      if (sortBy === 'Price Low-High') items.sort((a, b) => a.hourlyRate - b.hourlyRate)
      else if (sortBy === 'Price High-Low') items.sort((a, b) => b.hourlyRate - a.hourlyRate)
      else if (sortBy === 'Most Sessions') items.sort((a, b) => (b.totalSessions || 0) - (a.totalSessions || 0))
      else items.sort((a, b) => (b.averageRating || 0) - (a.averageRating || 0))

      setTutors(items)
    } catch (err: any) {
      setError(err.message || 'Error occurred')
    } finally {
      setIsLoading(false)
    }
  }, [searchQuery, subject, maxPrice, sortBy])

  useEffect(() => {
    const timer = setTimeout(() => fetchTutors(), 400)
    return () => clearTimeout(timer)
  }, [fetchTutors])

  // Top tutors for featured strip (top 4 by rating)
  const featuredTutors = [...tutors]
    .filter(t => (t.averageRating || 0) >= 4)
    .sort((a, b) => (b.averageRating || 0) - (a.averageRating || 0))
    .slice(0, 4)

  return (
    <div className="min-h-full bg-gray-50/30 py-8 px-4 sm:px-6 lg:px-8">
      <div className="max-w-7xl mx-auto space-y-8">

        {/* Header */}
        <div>
          <motion.h1
            initial={{ opacity: 0, x: -20 }}
            animate={{ opacity: 1, x: 0 }}
            className="text-3xl font-black text-gray-900 tracking-tight"
          >
            Find a Tutor
          </motion.h1>
          <motion.p
            initial={{ opacity: 0, x: -20 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ delay: 0.1 }}
            className="text-sm text-gray-500 mt-1.5 font-medium"
          >
            Choose from our network of verified expert tutors.
          </motion.p>
        </div>

        {/* Featured Tutors strip */}
        {!isLoading && featuredTutors.length > 0 && (
          <motion.div
            initial={{ opacity: 0, y: 16 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.15 }}
          >
            <div className="flex items-center gap-2 mb-3">
              <TrendingUp className="w-4 h-4 text-indigo-500" />
              <h2 className="text-sm font-bold text-gray-800 uppercase tracking-wider">Top Rated This Week</h2>
              <Sparkles className="w-3.5 h-3.5 text-amber-400" />
            </div>
            <div className="flex gap-3 overflow-x-auto pb-2 scrollbar-hide">
              {featuredTutors.map((tutor, i) => (
                <motion.div
                  key={tutor.id}
                  initial={{ opacity: 0, x: 20 }}
                  animate={{ opacity: 1, x: 0 }}
                  transition={{ delay: 0.2 + i * 0.06 }}
                  className="flex-shrink-0 w-56 cursor-pointer group"
                  onClick={() => navigate(`/student/tutors/${tutor.id}`)}
                >
                  <div className="bg-gradient-to-br from-indigo-50 to-purple-50 border border-indigo-100 rounded-2xl p-3.5 hover:shadow-md hover:border-indigo-200 transition-all">
                    <div className="flex items-center gap-2.5 mb-2">
                      <div className="w-10 h-10 rounded-xl bg-indigo-100 flex items-center justify-center font-bold text-indigo-700 text-sm overflow-hidden flex-shrink-0">
                        {tutor.profileImage
                          ? <img src={tutor.profileImage} alt={tutor.name} className="w-full h-full object-cover" />
                          : tutor.name.charAt(0).toUpperCase()
                        }
                      </div>
                      <div className="flex-1 min-w-0">
                        <p className="text-xs font-bold text-gray-900 truncate">{tutor.name}</p>
                        <div className="flex items-center gap-1">
                          <span className="text-[10px] text-amber-500 font-bold">★ {tutor.averageRating?.toFixed(1) || '—'}</span>
                          <span className="text-[10px] text-gray-400">({tutor.totalReviews || 0})</span>
                        </div>
                      </div>
                    </div>
                    <p className="text-[10px] text-gray-500 truncate mb-2">{tutor.headline || tutor.subjects?.[0] || 'Expert Tutor'}</p>
                    <div className="flex items-center justify-between">
                      <span className="text-xs font-black text-gray-900">₹{tutor.hourlyRate}<span className="text-[9px] font-medium text-gray-400">/hr</span></span>
                      <span className="text-[9px] font-bold text-indigo-600 bg-indigo-50 px-2 py-0.5 rounded-full">View →</span>
                    </div>
                  </div>
                </motion.div>
              ))}
            </div>
          </motion.div>
        )}

        {/* Category Pills */}
        <div className="flex flex-wrap gap-2">
          {CATEGORIES.map((cat) => (
            <button
              key={cat.value}
              onClick={() => setSubject(cat.value)}
              className={`px-3.5 py-1.5 rounded-full text-[11px] font-bold transition-all border ${
                subject === cat.value
                  ? 'bg-indigo-600 text-white border-indigo-600 shadow-sm'
                  : 'bg-white text-gray-600 border-gray-200 hover:border-indigo-300 hover:text-indigo-600'
              }`}
            >
              {cat.label}
            </button>
          ))}
        </div>

        {/* Filters Row */}
        <div className="flex flex-col md:flex-row items-center gap-3">
          <div className="flex-1 w-full relative">
            <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
            <Input
              placeholder="Search by name, expertise, or title..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-11 h-11 bg-white border-gray-200 rounded-xl text-sm w-full shadow-sm"
            />
          </div>

          <div className="flex flex-wrap items-center gap-2.5 w-full md:w-auto">
            {/* Price Filter */}
            <div className="flex items-center gap-2 bg-white border border-gray-200 rounded-xl px-3.5 h-11 shadow-sm">
              <span className="text-[10px] font-bold text-gray-400 uppercase tracking-widest">Max ₹</span>
              <input
                type="number"
                placeholder="Any"
                value={maxPrice}
                onChange={(e) => setMaxPrice(e.target.value)}
                className="w-14 bg-transparent outline-none text-sm font-bold text-gray-900 border-none p-0 focus:ring-0"
              />
            </div>

            {/* Sort Dropdown */}
            <div className="relative">
              <select
                value={sortBy}
                onChange={(e) => setSortBy(e.target.value)}
                className="h-11 pl-4 pr-10 bg-white border border-gray-200 rounded-xl outline-none text-xs font-bold text-gray-700 min-w-[140px] appearance-none cursor-pointer shadow-sm"
              >
                <option>Rating</option>
                <option>Most Sessions</option>
                <option>Price Low-High</option>
                <option>Price High-Low</option>
              </select>
              <ArrowUpDown className="absolute right-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-gray-400 pointer-events-none" />
            </div>
          </div>
        </div>

        {/* Results Count */}
        <div className="flex items-center justify-between px-1">
          <div className="flex items-center gap-3">
            <div className="w-9 h-9 rounded-xl bg-indigo-50 flex items-center justify-center text-indigo-500 border border-indigo-100">
              <Users className="w-4 h-4" />
            </div>
            <div>
              <h2 className="text-base font-bold text-gray-900 leading-tight">
                {isLoading ? 'Searching...' : `${tutors.length} Tutors`}
              </h2>
              <p className="text-[10px] text-gray-400 font-bold uppercase tracking-widest leading-tight mt-0.5">Profiles available</p>
            </div>
          </div>
          <div className="flex gap-2">
            {subject !== 'All Subjects' && (
              <Badge variant="success" className="px-3 py-1 font-bold text-[10px] uppercase border-none bg-indigo-50 text-indigo-700 cursor-default">
                {subject}
                <button className="ml-2 hover:opacity-60 transition-opacity" onClick={() => setSubject('All Subjects')}>×</button>
              </Badge>
            )}
          </div>
        </div>

        {/* Grid */}
        {isLoading ? (
          <div className="flex flex-col items-center justify-center py-24 gap-4">
            <Loader2 className="w-10 h-10 animate-spin text-indigo-500" />
            <p className="text-xs font-bold text-gray-400 uppercase tracking-widest animate-pulse">Finding matches...</p>
          </div>
        ) : error ? (
          <div className="text-center py-20 bg-white rounded-2xl border border-gray-100 shadow-sm flex flex-col items-center gap-4">
            <p className="text-red-500 text-sm font-bold">{error}</p>
            <Button variant="outline" onClick={fetchTutors}>Retry</Button>
          </div>
        ) : tutors.length === 0 ? (
          <div className="text-center py-32 bg-white rounded-3xl border-2 border-dashed border-gray-200">
            <Search className="w-12 h-12 text-gray-200 mx-auto mb-4" />
            <p className="text-sm font-black text-gray-900 uppercase tracking-[0.15em]">No Tutors Found</p>
            <p className="text-gray-400 mt-2 text-sm max-w-xs mx-auto font-medium">
              Try broadening your search or selecting a different category.
            </p>
            <Button variant="outline" className="mt-6 font-bold rounded-xl" onClick={() => { setSearchQuery(''); setSubject('All Subjects'); setMaxPrice('') }}>
              Reset Filters
            </Button>
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
            <AnimatePresence mode="popLayout">
              {tutors.map((tutor, i) => (
                <motion.div
                  key={tutor.id}
                  layout
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0, y: 20 }}
                  transition={{ duration: 0.25, delay: Math.min(i * 0.04, 0.4) }}
                  className="flex flex-col h-full"
                >
                  <TutorCard
                    name={tutor.name}
                    headline={tutor.headline}
                    rating={tutor.averageRating ?? 0}
                    reviews={tutor.totalReviews ?? 0}
                    followers={tutor.followerCount ?? 0}
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
                    onViewProfile={() => navigate(`/student/tutors/${tutor.id}`)}
                  />
                </motion.div>
              ))}
            </AnimatePresence>
          </div>
        )}

      </div>
    </div>
  )
}

export default FindTutors
