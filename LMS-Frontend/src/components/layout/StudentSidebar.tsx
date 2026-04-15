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

const SECTIONS = [
  {
    label: 'Main',
    items: [
      { label: 'Dashboard', path: '/student/dashboard', icon: LayoutDashboard },
      { label: 'Find Tutors', path: '/student/find-tutors', icon: Search },
      { label: 'My Sessions', path: '/student/my-sessions', icon: Calendar },
    ],
  },
  {
    label: 'Learning',
    items: [
      { label: 'Browse Courses', path: '/student/courses', icon: BookOpen },
      { label: 'My Courses', path: '/student/my-enrollments', icon: BookOpen },
      { label: 'Daily Games', path: '/student/daily-games', icon: Gamepad2 },
    ],
  },
  {
    label: 'AI Tools',
    items: [
      { label: 'AI Tools', path: '/student/ai-tools', icon: Sparkles },
      { label: 'AI Assistant', path: '/student/ai-assistant', icon: Bot },
      { label: 'Resume Builder', path: '/student/resume-builder', icon: FileText },
      { label: 'Mock Interview', path: '/student/mock-interview', icon: Mic },
    ],
  },
  {
    label: 'Account',
    items: [
      { label: 'Bonus Points', path: '/student/wallet', icon: Wallet },
      { label: 'Referral Program', path: '/student/referrals', icon: Gift },
      { label: 'Subscription', path: '/student/subscription', icon: CreditCard },
      { label: 'Billing', path: '/student/billing', icon: Receipt },
      { label: 'Inbox', path: '/student/inbox', icon: MessageSquare },
      { label: 'Notifications', path: '/student/notifications', icon: Bell },
      { label: 'Profile', path: '/student/profile', icon: User },
      { label: 'Settings', path: '/student/profile-settings', icon: Settings },
    ],
  },
]

const StudentSidebar = ({ currentPath }: StudentSidebarProps) => {
  return (
    <nav className="px-2 pb-2">
      {SECTIONS.map((section) => (
        <div key={section.label} className="mb-3">
          <p className="px-3 mb-1 text-[10px] font-bold uppercase tracking-widest text-gray-400">
            {section.label}
          </p>
          {section.items.map((item) => {
            const Icon = item.icon
            const isActive = currentPath === item.path || currentPath.startsWith(item.path + '/')
            return (
              <Link
                key={item.path}
                to={item.path}
                className={clsx(
                  'flex items-center gap-2.5 px-3 py-1.5 rounded-lg text-[13px] font-medium transition-colors',
                  isActive
                    ? 'bg-indigo-50 text-indigo-700'
                    : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900'
                )}
              >
                <Icon className={clsx('w-4 h-4 flex-shrink-0', isActive ? 'text-indigo-600' : 'text-gray-400')} />
                <span className="truncate">{item.label}</span>
              </Link>
            )
          })}
        </div>
      ))}
    </nav>
  )
}

export default StudentSidebar
