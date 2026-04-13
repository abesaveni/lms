import { Link } from 'react-router-dom'
import {
  LayoutDashboard,
  Calendar,
  User,
  MessageSquare,
  DollarSign,
  Settings,
  Video,
  Bell,
  BookOpen,
  TrendingUp,
} from 'lucide-react'
import { clsx } from 'clsx'

interface TutorSidebarProps {
  isOpen: boolean
  currentPath: string
}

const TutorSidebar = ({ currentPath }: TutorSidebarProps) => {
  const menuItems = [
    {
      label: 'Dashboard',
      path: '/tutor/dashboard',
      icon: LayoutDashboard,
    },
    {
      label: 'Sessions',
      path: '/tutor/sessions',
      icon: Calendar,
    },
    {
      label: 'Create Session',
      path: '/tutor/sessions/create',
      icon: Video,
    },
    {
      label: 'My Courses',
      path: '/tutor/courses',
      icon: BookOpen,
    },
    {
      label: 'Subject Rates',
      path: '/tutor/subject-rates',
      icon: TrendingUp,
    },
    {
      label: 'Earnings',
      path: '/tutor/earnings',
      icon: DollarSign,
    },
    {
      label: 'Profile',
      path: '/tutor/profile',
      icon: User,
    },
    {
      label: 'Settings',
      path: '/tutor/profile-settings',
      icon: Settings,
    },
    {
      label: 'Notifications',
      path: '/tutor/notifications',
      icon: Bell,
    },
    {
      label: 'Inbox',
      path: '/tutor/inbox',
      icon: MessageSquare,
    },
  ]

  return (
    <nav className="px-2 space-y-1">
      {menuItems.map((item) => {
        const Icon = item.icon
        const isActive = currentPath === item.path || currentPath.startsWith(item.path + '/')
        
        return (
          <Link
            key={item.path}
            to={item.path}
            className={clsx(
              'flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors',
              isActive
                ? 'bg-primary-50 text-primary-700'
                : 'text-gray-700 hover:bg-gray-100'
            )}
          >
            <Icon className="w-5 h-5" />
            <span>{item.label}</span>
          </Link>
        )
      })}
    </nav>
  )
}

export default TutorSidebar
