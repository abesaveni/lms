import { Card } from '../ui/Card'
import { Avatar } from '../ui/Avatar'
import { Badge } from '../ui/Badge'
import Button from '../ui/Button'
import { Star, Users } from 'lucide-react'

interface TutorCardProps {
  name: string
  rating: number
  reviews: number
  followers?: number
  subjects: string[]
  price: number
  location?: string
  avatar?: string
  available?: boolean
  onViewProfile?: () => void
}

export const TutorCard = ({
  name,
  rating,
  reviews,
  followers = 0,
  subjects,
  price,
  avatar,
  available = true,
  onViewProfile,
}: TutorCardProps) => {
  return (
    <Card hover className="flex flex-col h-full bg-white border border-gray-100 rounded-2xl overflow-hidden shadow-sm hover:shadow-lg transition-all transform hover:-translate-y-1">
      <div className="p-3.5 flex-1 flex flex-col">
        {/* Profile Info */}
        <div className="flex items-center gap-3 mb-3">
          <div className="relative shrink-0">
            <Avatar src={avatar} name={name} size="lg" className="rounded-xl border border-gray-100 bg-gray-50" />
            {available && (
              <span className="absolute -bottom-1 -right-1 w-3.5 h-3.5 bg-green-500 border-2 border-white rounded-full"></span>
            )}
          </div>
          <div className="flex-1 min-w-0">
            <h3 className="text-sm font-bold text-gray-900 truncate leading-tight tracking-tight">{name}</h3>
            <div className="flex items-center gap-1.5 mt-0.5">
              <div className="flex items-center gap-0.5 bg-yellow-50 px-1.5 py-0.5 rounded-md">
                <Star className="w-2.5 h-2.5 fill-yellow-400 text-yellow-400" />
                <span className="text-[10px] font-bold text-yellow-700">{rating}</span>
              </div>
              <span className="text-[10px] font-bold text-gray-400 uppercase tracking-widest">• {reviews}</span>
            </div>
          </div>
        </div>

        {/* Followers Status - Compact line */}
        <div className="flex items-center gap-1.5 mb-3 text-[10px] text-gray-400 font-bold uppercase tracking-wider">
           <Users className="w-3 h-3" />
           <span>{followers} Followers</span>
        </div>

        {/* Subjects - Standardized Height (Tightened) */}
        <div className="mb-3 flex-1">
          <div className="flex flex-wrap gap-1 min-h-[44px] content-start">
            {(subjects && subjects.length > 0 ? subjects : ['Expert']).slice(0, 3).map((sub, i) => (
              <Badge 
                key={i} 
                variant="info" 
                className="rounded-lg px-2 py-0.5 border-none bg-primary-50 text-primary-600 font-bold text-[9px] uppercase tracking-wide"
              >
                {sub}
              </Badge>
            ))}
            {subjects && subjects.length > 3 && (
              <Badge variant="default" className="rounded-lg px-2 py-0.5 text-[9px] font-bold text-gray-300 border-none bg-gray-50">+ {subjects.length - 3}</Badge>
            )}
          </div>
        </div>

        {/* Action and Pricing */}
        <div className="pt-3 border-t border-gray-50 flex items-center justify-between">
           <div className="flex flex-col">
              <div className="flex items-baseline gap-0.5">
                 <span className="text-xl font-bold text-gray-900">${price}</span>
                 <span className="text-[10px] font-bold text-gray-400 uppercase tracking-widest pl-0.5">/hr</span>
              </div>
           </div>
           <Button 
            size="sm"
            className="rounded-xl font-bold text-[10px] px-3 py-1 shadow-sm"
            onClick={onViewProfile}
           >
             Profile
           </Button>
        </div>
      </div>
    </Card>
  )
}

export default TutorCard
