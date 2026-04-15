import { Star, Users, Clock, CheckCircle2, BookOpen, Zap } from 'lucide-react'
import Button from '../ui/Button'
import { Badge } from '../ui/Badge'

interface TutorCardProps {
  name: string
  headline?: string
  rating: number
  reviews: number
  followers?: number
  subjects: string[]
  price: number
  location?: string
  avatar?: string
  available?: boolean
  yearsOfExperience?: number
  totalSessions?: number
  isVerified?: boolean
  hasBackgroundCheck?: boolean
  trialAvailable?: boolean
  trialPrice?: number
  onViewProfile?: () => void
}

// Generate a consistent gradient from tutor name
const getGradient = (name: string) => {
  const gradients = [
    'from-violet-500 to-indigo-600',
    'from-indigo-500 to-blue-600',
    'from-blue-500 to-cyan-600',
    'from-emerald-500 to-teal-600',
    'from-orange-500 to-amber-600',
    'from-rose-500 to-pink-600',
    'from-purple-500 to-violet-600',
    'from-teal-500 to-emerald-600',
  ]
  const idx = name.charCodeAt(0) % gradients.length
  return gradients[idx]
}

const getInitials = (name: string) =>
  name
    .split(' ')
    .map((n) => n[0])
    .slice(0, 2)
    .join('')
    .toUpperCase()

const renderStars = (rating: number) => {
  const filled = Math.round(rating)
  return Array.from({ length: 5 }).map((_, i) => (
    <svg
      key={i}
      className={`w-3 h-3 ${i < filled ? 'text-amber-400 fill-amber-400' : 'text-gray-200 fill-gray-200'}`}
      viewBox="0 0 20 20"
    >
      <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
    </svg>
  ))
}

export const TutorCard = ({
  name,
  headline,
  rating,
  reviews,
  followers = 0,
  subjects,
  price,
  avatar,
  available = true,
  yearsOfExperience,
  totalSessions,
  isVerified = true,
  hasBackgroundCheck = false,
  trialAvailable = false,
  trialPrice,
  onViewProfile,
}: TutorCardProps) => {
  const gradient = getGradient(name)
  const initials = getInitials(name)

  return (
    <div className="group relative bg-white rounded-2xl border border-gray-100 overflow-hidden shadow-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300 flex flex-col h-full">
      {/* Gradient Banner */}
      <div className={`h-20 bg-gradient-to-br ${gradient} relative overflow-hidden`}>
        {/* Decorative circles */}
        <div className="absolute -top-4 -right-4 w-24 h-24 bg-white/10 rounded-full" />
        <div className="absolute -bottom-6 -left-2 w-16 h-16 bg-white/10 rounded-full" />
        {available && (
          <div className="absolute top-3 right-3 flex items-center gap-1 bg-white/20 backdrop-blur-sm text-white text-[9px] font-bold px-2 py-0.5 rounded-full border border-white/30">
            <Zap className="w-2.5 h-2.5 fill-white" />
            Available
          </div>
        )}
      </div>

      {/* Avatar overlapping banner */}
      <div className="relative px-4">
        <div className="absolute -top-7 left-4">
          <div className={`w-14 h-14 rounded-xl border-3 border-white shadow-lg overflow-hidden ring-2 ring-white`}>
            {avatar ? (
              <img src={avatar} alt={name} className="w-full h-full object-cover" />
            ) : (
              <div className={`w-full h-full bg-gradient-to-br ${gradient} flex items-center justify-center`}>
                <span className="text-white font-bold text-base">{initials}</span>
              </div>
            )}
          </div>
          {isVerified && (
            <div className="absolute -bottom-1 -right-1 w-5 h-5 bg-indigo-500 rounded-full flex items-center justify-center border-2 border-white">
              <CheckCircle2 className="w-3 h-3 text-white fill-white" />
            </div>
          )}
        </div>
      </div>

      {/* Content */}
      <div className="pt-9 px-4 pb-4 flex-1 flex flex-col">
        {/* Name + Rating */}
        <div className="mb-2">
          <h3 className="text-sm font-bold text-gray-900 truncate leading-tight">{name}</h3>
          {headline && (
            <p className="text-[11px] text-gray-500 mt-0.5 line-clamp-1 font-medium">{headline}</p>
          )}
          <div className="flex items-center gap-1.5 mt-1.5">
            <div className="flex items-center gap-0.5">{renderStars(rating)}</div>
            <span className="text-[11px] font-bold text-amber-600">{rating > 0 ? rating.toFixed(1) : '—'}</span>
            <span className="text-[10px] text-gray-400">({reviews})</span>
          </div>
        </div>

        {/* Stats Row */}
        <div className="flex items-center gap-3 mb-3 text-[10px] text-gray-400">
          {typeof totalSessions === 'number' && totalSessions > 0 && (
            <div className="flex items-center gap-1">
              <BookOpen className="w-3 h-3" />
              <span className="font-bold">{totalSessions} sessions</span>
            </div>
          )}
          {followers > 0 && (
            <div className="flex items-center gap-1">
              <Users className="w-3 h-3" />
              <span className="font-bold">{followers}</span>
            </div>
          )}
          {typeof yearsOfExperience === 'number' && yearsOfExperience > 0 && (
            <div className="flex items-center gap-1">
              <Clock className="w-3 h-3" />
              <span className="font-bold">{yearsOfExperience}yr</span>
            </div>
          )}
        </div>

        {/* Subject Tags */}
        <div className="flex flex-wrap gap-1 mb-3 min-h-[28px]">
          {(subjects && subjects.length > 0 ? subjects : ['Expert']).slice(0, 3).map((sub, i) => (
            <span
              key={i}
              className="text-[9px] font-bold px-2 py-0.5 rounded-full bg-indigo-50 text-indigo-600 uppercase tracking-wide border border-indigo-100"
            >
              {sub}
            </span>
          ))}
          {subjects && subjects.length > 3 && (
            <span className="text-[9px] font-bold px-2 py-0.5 rounded-full bg-gray-50 text-gray-400 border border-gray-100">
              +{subjects.length - 3}
            </span>
          )}
        </div>

        {/* Trust Badges */}
        {(hasBackgroundCheck || trialAvailable) && (
          <div className="flex flex-wrap gap-1 mb-3">
            {hasBackgroundCheck && (
              <span className="text-[9px] font-bold px-2 py-0.5 rounded-full bg-emerald-50 text-emerald-700 border border-emerald-100 flex items-center gap-1">
                <svg className="w-2.5 h-2.5" fill="currentColor" viewBox="0 0 20 20"><path fillRule="evenodd" d="M10 1l2.39 4.84L18 6.91l-4 3.89.94 5.51L10 13.77l-4.94 2.54.94-5.51L2 6.91l5.61-.07z" clipRule="evenodd"/></svg>
                Background Check
              </span>
            )}
            {trialAvailable && (
              <span className="text-[9px] font-bold px-2 py-0.5 rounded-full bg-orange-50 text-orange-600 border border-orange-100">
                🎯 Trial {trialPrice === 0 ? 'Free' : trialPrice ? `₹${trialPrice}` : 'Available'}
              </span>
            )}
          </div>
        )}

        {/* Price + CTA */}
        <div className="mt-auto pt-3 border-t border-gray-50 flex items-center justify-between">
          <div>
            <div className="flex items-baseline gap-0.5">
              <span className="text-lg font-black text-gray-900">₹{price}</span>
              <span className="text-[10px] font-bold text-gray-400 uppercase tracking-widest">/hr</span>
            </div>
          </div>
          <Button
            size="sm"
            className="rounded-xl font-bold text-xs px-4 py-1.5 bg-indigo-600 hover:bg-indigo-700 text-white shadow-sm group-hover:shadow-indigo-200 group-hover:shadow-md transition-all"
            onClick={onViewProfile}
          >
            View Profile
          </Button>
        </div>
      </div>
    </div>
  )
}

export default TutorCard
