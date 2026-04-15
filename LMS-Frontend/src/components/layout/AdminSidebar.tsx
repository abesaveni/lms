import { Link } from 'react-router-dom'
import {
  LayoutDashboard,
  Users,
  DollarSign,
  Settings,
  FileText,
  Calendar,
  MessageSquare,
  Shield,
  BarChart3,
  Wallet,
  BookOpen,
  Bell,
  Bot,
  Tag,
} from 'lucide-react'
import { clsx } from 'clsx'

interface AdminSidebarProps {
  isOpen: boolean
  currentPath: string
}

const AdminSidebar = ({ currentPath }: AdminSidebarProps) => {
  const menuItems = [
    {
      label: 'Dashboard',
      path: '/admin/dashboard',
      icon: LayoutDashboard,
    },
    {
      label: 'User Management',
      path: '/admin/users',
      icon: Users,
    },
    {
      label: 'Tutor Verification',
      path: '/admin/tutor-verification',
      icon: Shield,
    },
    {
      label: 'Financial Dashboard',
      path: '/admin/financial-dashboard',
      icon: BarChart3,
    },
    {
      label: 'Financials',
      path: '/admin/financials',
      icon: DollarSign,
    },
    {
      label: 'Payout Management',
      path: '/admin/payouts',
      icon: Wallet,
    },
    {
      label: 'Admin Management',
      path: '/admin/admin-management',
      icon: Users,
    },
    {
      label: 'Blog Management',
      path: '/admin/blogs',
      icon: FileText,
    },
    {
      label: 'Subject Management',
      path: '/admin/subjects',
      icon: BookOpen,
    },
    {
      label: 'Coupons',
      path: '/admin/coupons',
      icon: Tag,
    },
    {
      label: 'Consent Management',
      path: '/admin/consents',
      icon: Calendar,
    },
    {
      label: 'WhatsApp Campaigns',
      path: '/admin/whatsapp-campaigns',
      icon: MessageSquare,
    },
    {
      label: 'API Settings',
      path: '/admin/api-settings',
      icon: Settings,
    },
    {
      label: 'Settings',
      path: '/admin/settings',
      icon: Settings,
    },
    {
      label: 'AI Tools',
      path: '/admin/ai-tools',
      icon: Bot,
    },
    {
      label: 'Notifications',
      path: '/admin/notifications',
      icon: Bell,
    },
    {
      label: 'Inbox',
      path: '/admin/inbox',
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

export default AdminSidebar
