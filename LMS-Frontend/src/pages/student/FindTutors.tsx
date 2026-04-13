import { useState, useEffect, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { Search, Users, Loader2, ArrowUpDown, ChevronDown } from 'lucide-react'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'
import { TutorCard } from '../../components/domain/TutorCard'
import { getTutors, TutorDto } from '../../services/tutorsApi'
import { motion, AnimatePresence } from 'framer-motion'
import { Badge } from '../../components/ui/Badge'

const FindTutors = () => {
  const navigate = useNavigate()
  
  // State
  const [tutors, setTutors] = useState<TutorDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  
  // Filters
  const [searchQuery, setSearchQuery] = useState('')
  const [subject, setSubject] = useState('All Subjects')
  const [maxPrice, setMaxPrice] = useState('')
  const [sortBy, setSortBy] = useState('Rating')

  const fetchTutors = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await getTutors({
        page: 1,
        pageSize: 50,
      });
      
      let items = [...response.items];
      
      // Local Filter Logic
      if (searchQuery) {
        const query = searchQuery.toLowerCase();
        items = items.filter(tutor => 
          (tutor.name && tutor.name.toLowerCase().includes(query)) ||
          (tutor.headline && tutor.headline.toLowerCase().includes(query)) ||
          (tutor.bio && tutor.bio.toLowerCase().includes(query))
        );
      }
      
      if (subject !== 'All Subjects') {
        const lowerSubject = subject.toLowerCase();
        items = items.filter(tutor => 
          (tutor.subjects && tutor.subjects.includes(subject)) ||
          (tutor.headline && tutor.headline.toLowerCase().includes(lowerSubject)) ||
          (tutor.bio && tutor.bio.toLowerCase().includes(lowerSubject))
        );
      }
      
      if (maxPrice) {
        const pLimit = parseFloat(maxPrice);
        if (!isNaN(pLimit)) {
          items = items.filter(tutor => tutor.hourlyRate <= pLimit);
        }
      }

      // Sort Logic
      if (sortBy === 'Price Low-High') items.sort((a,b) => a.hourlyRate - b.hourlyRate);
      else if (sortBy === 'Price High-Low') items.sort((a,b) => b.hourlyRate - a.hourlyRate);
      else if (sortBy === 'Rating') items.sort((a,b) => (b.averageRating || 0) - (a.averageRating || 0));

      setTutors(items);
    } catch (err: any) {
      setError(err.message || 'Error occurred');
    } finally {
      setIsLoading(false);
    }
  }, [searchQuery, subject, maxPrice, sortBy]);

  useEffect(() => {
    const timer = setTimeout(() => fetchTutors(), 400);
    return () => clearTimeout(timer);
  }, [fetchTutors]);

  return (
    <div className="min-h-full bg-gray-50/10 py-8 px-4 sm:px-6 lg:px-8">
      <div className="max-w-7xl mx-auto space-y-8">
        
        <div>
          <motion.h1 
            initial={{ opacity: 0, x: -20 }}
            animate={{ opacity: 1, x: 0 }}
            className="text-3xl font-bold text-gray-900 tracking-tight"
          >
            Find Tutor
          </motion.h1>
          <motion.p 
            initial={{ opacity: 0, x: -20 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ delay: 0.1 }}
            className="text-sm text-gray-500 mt-2 font-medium"
          >
            Choose from a professional network of expert knowledge masters.
          </motion.p>
        </div>

        {/* Dynamic Horizontal Filters - Premium Row */}
        <div className="flex flex-col md:flex-row items-center gap-4">
          <div className="flex-1 w-full relative group">
            <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400 focus-within:text-primary-500 transition-colors" />
            <Input
              placeholder="Search by name, expertise, or title..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-12 h-12 bg-white border-gray-200 focus:border-primary-300 focus:bg-white rounded-xl text-sm w-full shadow-sm hover:border-gray-300 transition-all border-none ring-1 ring-gray-200"
            />
          </div>

          <div className="flex flex-wrap items-center gap-3 w-full md:w-auto">
            {/* Category Dropdown */}
            <div className="relative">
              <select 
                value={subject}
                onChange={(e) => setSubject(e.target.value)}
                className="h-12 px-5 py-2 bg-white border border-gray-200 rounded-xl outline-none text-xs font-bold text-gray-700 min-w-[150px] appearance-none cursor-pointer shadow-sm hover:border-gray-300 transition-all"
              >
                <option>All Subjects</option>
                <option disabled className="font-bold text-gray-400 bg-gray-50">── Technology & Coding ──</option>
                <option>Web Development</option>
                <option>Frontend Development</option>
                <option>Backend Development</option>
                <option>Mobile App Development</option>
                <option>Python Programming</option>
                <option>Java Programming</option>
                <option>C++ / C# Programming</option>
                <option>Data Science</option>
                <option>Machine Learning</option>
                <option>Artificial Intelligence</option>
                <option>Cloud Computing</option>
                <option>Design</option>
                <option disabled className="font-bold text-gray-400 bg-gray-50">── Sciences & Math ──</option>
                <option>Mathematics</option>
                <option>Statistics</option>
                <option>Physics</option>
                <option>Chemistry</option>
                <option>Biology</option>
                <option disabled className="font-bold text-gray-400 bg-gray-50">── Humanities & Business ──</option>
                <option>English Literature</option>
                <option>History</option>
                <option>Geography</option>
                <option>Economics</option>
                <option>Accounting</option>
                <option>Business Studies</option>
              </select>
              <ChevronDown className="absolute right-4 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
            </div>

            {/* Price Filter Group */}
            <div className="flex items-center gap-2 bg-white border border-gray-200 rounded-xl px-4 h-12 shadow-sm hover:border-gray-300 transition-all">
              <span className="text-[10px] font-bold text-gray-400 uppercase tracking-widest pl-1">Max $</span>
              <input 
                type="number"
                placeholder="0"
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
                className="h-12 px-5 py-2 bg-white border border-gray-200 rounded-xl outline-none text-xs font-bold text-gray-700 min-w-[150px] appearance-none cursor-pointer shadow-sm hover:border-gray-300 transition-all"
              >
                <option>Rating</option>
                <option>Price Low-High</option>
                <option>Price High-Low</option>
              </select>
              <ArrowUpDown className="absolute right-4 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
            </div>
          </div>
        </div>

        {/* Found Status Banner */}
        <div className="flex items-center justify-between px-2">
          <div className="flex items-center gap-3">
             <div className="w-10 h-10 rounded-xl bg-primary-50 flex items-center justify-center text-primary-500 shadow-sm border border-primary-100">
               <Users className="w-5 h-5 font-bold" />
             </div>
             <div>
               <h2 className="text-lg font-bold text-gray-900 tracking-tight leading-tight">
                 {isLoading ? 'Searching...' : `${tutors.length} Total Tutors`}
               </h2>
               <p className="text-[10px] text-gray-400 font-bold uppercase tracking-widest leading-tight mt-0.5">Profiles currently live</p>
             </div>
          </div>
          <div className="flex gap-2">
            {subject !== 'All Subjects' && (
              <Badge variant="success" className="px-3 py-1.5 font-bold text-[10px] uppercase border-none bg-green-50 text-green-700 transition-all cursor-default">
                 {subject} 
                 <button className="ml-2 hover:bg-green-100 rounded px-1 transition-colors" onClick={() => setSubject('All Subjects')}>×</button>
              </Badge>
            )}
          </div>
        </div>

        {/* Dynamic Grid Results */}
        {isLoading ? (
          <div className="flex flex-col items-center justify-center py-24 gap-4">
            <Loader2 className="w-10 h-10 animate-spin text-primary-500" />
            <p className="text-sm font-bold text-gray-400 uppercase tracking-widest animate-pulse">Scanning Matches...</p>
          </div>
        ) : error ? (
          <div className="text-center py-20 bg-white rounded-2xl border border-gray-100 shadow-sm flex flex-col items-center gap-4">
            <div className="p-4 bg-red-50 rounded-full">
               <span className="text-red-500 text-2xl font-black">!</span>
            </div>
            <p className="text-red-500 text-sm font-bold">{error}</p>
            <Button variant="outline" className="font-bold border-red-100 text-red-600 hover:bg-red-50" onClick={fetchTutors}>Retry Connection</Button>
          </div>
        ) : tutors.length === 0 ? (
          <div className="text-center py-32 bg-white rounded-3xl border-2 border-dashed border-gray-200">
            <Search className="w-12 h-12 text-gray-200 mx-auto mb-4" />
            <p className="text-sm font-black text-gray-900 uppercase tracking-[0.2em]">No Tutors Found</p>
            <p className="text-gray-400 mt-2 text-sm max-w-xs mx-auto px-4 font-medium">We couldn't find any experts matching those filters. Try broading your search.</p>
            <Button variant="outline" className="mt-8 font-bold border-gray-100 hover:bg-gray-50 rounded-xl" onClick={() => { setSearchQuery(''); setSubject('All Subjects'); setMaxPrice(''); }}>Reset All Filters</Button>
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
            <AnimatePresence mode="popLayout">
              {tutors.map((tutor) => (
                <motion.div
                  key={tutor.id}
                  layout
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0, y: 20 }}
                  transition={{ duration: 0.3, ease: "easeOut" }}
                  className="flex flex-col h-full"
                >
                  <TutorCard
                    name={tutor.name}
                    rating={tutor.averageRating ?? 0}
                    reviews={tutor.totalReviews ?? 0}
                    followers={tutor.followerCount ?? 0}
                    subjects={tutor.subjects || []}
                    price={tutor.hourlyRate}
                    avatar={tutor.profileImage}
                    available={true}
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
