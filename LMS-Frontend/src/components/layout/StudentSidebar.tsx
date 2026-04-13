import { Link } from 'react-router-dom'
import {
  LayoutDashboard,
  Search,
  Calendar,
  User,
  MessageSquare,
  Settings,
  Wallet,
  Gift,
  Bell,
  FileText,
  Bot,
  Sparkles,
  Mic,
  CreditCard,
  BookOpen,
  Receipt,
  Gamepad2,
} from 'lucide-react'
import { clsx } from 'clsx'

interface StudentSidebarProps {
  isOpen: boolean
  currentPath: string
}

const StudentSidebar = ({ currentPath }: StudentSidebarProps) => {
  const menuItems = [
    {
      label: 'Dashboard',
      path: '/student/dashboard',
      icon: LayoutDashboard,
    },
    {
      label: 'Find Tutors',
      path: '/student/find-tutors',
      icon: Search,
    },
    {
      label: 'My Sessions',
      path: '/student/my-sessions',
      icon: Calendar,
    },
    {
      label: 'Browse Courses',
      path: '/student/courses',
      icon: Search,
    },
    {
      label: 'My Courses',
      path: '/student/my-enrollments',
      icon: BookOpen,
    },
    {
      label: 'Billing',
      path: '/student/billing',
      icon: Receipt,
    },
    {
      label: 'Daily Games',
      path: '/student/daily-games',
      icon: Gamepad2,
    },
    {
      label: 'Bonus Points',
      path: '/student/wallet',
      icon: Wallet,
    },
    {
      label: 'Referral Program',
      path: '/student/referrals',
      icon: Gift,
    },
    {
      label: 'Profile',
      path: '/student/profile',
      icon: User,
    },
    {
      label: 'Settings',
      path: '/student/profile-settings',
      icon: Settings,
    },
    {
      label: 'Notifications',
      path: '/student/notifications',
      icon: Bell,
    },
    {
      label: 'Inbox',
      path: '/student/inbox',
      icon: MessageSquare,
    },
    {
      label: 'AI Tools',
      path: '/student/ai-tools',
      icon: Sparkles,
    },
    {
      label: 'Resume Builder',
      path: '/student/resume-builder',
      icon: FileText,
    },
    {
      label: 'AI Assistant',
      path: '/student/ai-assistant',
      icon: Bot,
    },
    {
      label: 'Mock Interview',
      path: '/student/mock-interview',
      icon: Mic,
    },
    {
      label: 'Subscription',
      path: '/student/subscription',
      icon: CreditCard,
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

export default StudentSidebar
