import { useEffect, useState, useMemo } from 'react'
import { 
  Bell, 
  Trash2, 
  CheckCircle, 
  Clock, 
  Search, 
  X, 
  CreditCard, 
  Video, 
  UserPlus, 
  Gift, 
  AlertCircle 
} from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import { motion, AnimatePresence } from 'framer-motion'
import { 
  getNotifications, 
  markNotificationRead, 
  markAllNotificationsRead, 
  deleteNotification,
  NotificationDto 
} from '../services/notificationsApi'
import { getCurrentUserRole } from '../utils/auth'
import { Card, CardContent } from '../components/ui/Card'
import Button from '../components/ui/Button'
import { EmptyState } from '../components/ui/EmptyState'
import { Tabs, TabsList, TabsTrigger } from '../components/ui/Tabs'
import { clsx } from 'clsx'

const NotificationsPage = () => {
  const [notifications, setNotifications] = useState<NotificationDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [searchQuery, setSearchQuery] = useState('')
  const [activeFilter, setActiveFilter] = useState('all')
  const navigate = useNavigate()

  const loadNotifications = async () => {
    setIsLoading(true)
    setError(null)
    try {
      const data = await getNotifications({ pageSize: 150 })
      setNotifications(data.items)
    } catch (err: any) {
      setError(err.message || 'Failed to load notifications')
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    loadNotifications()
  }, [])

  const filteredNotifications = useMemo(() => {
    let result = notifications

    // Filter by tab
    if (activeFilter === 'unread') {
      result = result.filter(n => !n.isRead)
    }

    // Filter by search
    if (searchQuery.trim() !== '') {
      result = result.filter(n => 
        n.title.toLowerCase().includes(searchQuery.toLowerCase()) || 
        n.message.toLowerCase().includes(searchQuery.toLowerCase())
      )
    }

    return result
  }, [notifications, searchQuery, activeFilter])

  // Group notifications by date
  const groupedNotifications = useMemo(() => {
    const groups: { [key: string]: NotificationDto[] } = {
      'Today': [],
      'Yesterday': [],
      'Earlier': []
    }

    const today = new Date()
    today.setHours(0, 0, 0, 0)
    
    const yesterday = new Date(today)
    yesterday.setDate(yesterday.getDate() - 1)

    filteredNotifications.forEach(n => {
      const nDate = new Date(n.createdAt)
      nDate.setHours(0, 0, 0, 0)

      if (nDate.getTime() === today.getTime()) {
        groups['Today'].push(n)
      } else if (nDate.getTime() === yesterday.getTime()) {
        groups['Yesterday'].push(n)
      } else {
        groups['Earlier'].push(n)
      }
    })

    return groups
  }, [filteredNotifications])

  const getNotificationIcon = (type: string) => {
    const iconClass = "w-6 h-6"
    switch (type.toLowerCase()) {
      case 'payment':
      case 'withdrawal':
      case 'earning':
        return <CreditCard className={iconClass} />
      case 'sessionbooking':
      case 'session':
        return <Video className={iconClass} />
      case 'referral':
      case 'registration':
        return <UserPlus className={iconClass} />
      case 'bonus':
      case 'point':
        return <Gift className={iconClass} />
      case 'alert':
      case 'warning':
        return <AlertCircle className={iconClass} />
      default:
        return <Bell className={iconClass} />
    }
  }

  const handleMarkAsRead = async (e: React.MouseEvent | null, id: string) => {
    if (e) e.stopPropagation()
    try {
      await markNotificationRead(id)
      setNotifications((prev: NotificationDto[]) => 
        prev.map((n: NotificationDto) => n.id === id ? { ...n, isRead: true } : n)
      )
    } catch (err: any) {
      console.error('Failed to mark as read:', err)
    }
  }

  const handleMarkAllAsRead = async () => {
    try {
      await markAllNotificationsRead()
      setNotifications((prev: NotificationDto[]) => prev.map((n: NotificationDto) => ({ ...n, isRead: true })))
    } catch (err: any) {
      console.error('Failed to mark all as read:', err)
    }
  }

  const handleDelete = async (e: React.MouseEvent, id: string) => {
    e.stopPropagation()
    try {
      await deleteNotification(id)
      setNotifications((prev: NotificationDto[]) => prev.filter((n: NotificationDto) => n.id !== id))
    } catch (err: any) {
      console.error('Failed to delete notification:', err)
    }
  }

  const handleNotificationClick = async (notification: NotificationDto) => {
    if (!notification.isRead) {
      await handleMarkAsRead(null, notification.id)
    }

    const role = getCurrentUserRole()?.toLowerCase()
    let dashboardPath = '/student/dashboard'
    if (role === 'admin') dashboardPath = '/admin/dashboard'
    else if (role === 'tutor') dashboardPath = '/tutor/dashboard'

    if (notification.actionUrl && notification.actionUrl.trim() !== '') {
      try {
        navigate(notification.actionUrl)
      } catch (err) {
        navigate(dashboardPath)
      }
    } else {
      navigate(dashboardPath)
    }
  }

  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      {/* Header Section */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between mb-8 gap-4">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 mb-1">Notifications</h1>
          <p className="text-gray-600 font-medium">Your personal hub for updates and alerts</p>
        </div>
        <div className="flex gap-2">
          {notifications.some(n => !n.isRead) && (
            <Button 
              variant="outline" 
              size="sm" 
              onClick={handleMarkAllAsRead}
              className="font-semibold text-primary-600 border-primary-100 hover:bg-primary-50"
            >
              <CheckCircle className="w-4 h-4 mr-2" />
              Mark all as read
            </Button>
          )}
        </div>
      </div>

      {error && (
        <div className="mb-6 p-4 bg-red-50 text-red-700 text-sm rounded-xl border border-red-100 font-semibold flex items-center gap-2">
          <AlertCircle className="w-4 h-4" />
          {error}
        </div>
      )}

      <div className="flex flex-col md:flex-row gap-6 mb-6">
        {/* Filtering Tabs */}
        <div className="flex-1">
          <Tabs defaultValue="all" value={activeFilter} onValueChange={setActiveFilter}>
            <TabsList className="border-none mb-0 bg-gray-100/50 p-1 rounded-xl w-fit">
              <TabsTrigger value="all">
                <span className={clsx(
                  "px-4 py-1.5 rounded-lg transition-all",
                  activeFilter === 'all' ? "bg-white shadow-sm text-primary-600" : "text-gray-500"
                )}>
                  All 
                  <span className="ml-2 bg-gray-200 text-gray-700 px-2 py-0.5 rounded-full text-[10px]">
                    {notifications.length}
                  </span>
                </span>
              </TabsTrigger>
              <TabsTrigger value="unread">
                <span className={clsx(
                  "px-4 py-1.5 rounded-lg transition-all",
                  activeFilter === 'unread' ? "bg-white shadow-sm text-primary-600" : "text-gray-500"
                )}>
                  Unread
                  <span className="ml-2 bg-primary-100 text-primary-600 px-2 py-0.5 rounded-full text-[10px]">
                    {notifications.filter(n => !n.isRead).length}
                  </span>
                </span>
              </TabsTrigger>
            </TabsList>
          </Tabs>
        </div>

        {/* Search */}
        <div className="relative w-full md:w-80 group">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400 group-focus-within:text-primary-500 transition-colors pointer-events-none" />
          <input
            id="notif-search"
            type="text"
            placeholder="Search notifications..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="w-full pl-10 pr-9 py-2 bg-white border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-400 transition-all text-sm h-11 shadow-sm"
          />
          {searchQuery && (
            <button
              onClick={() => setSearchQuery('')}
              className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 transition-colors focus:outline-none"
            >
              <X className="h-4 w-4" />
            </button>
          )}
        </div>
      </div>

      <div className="space-y-8">
        {isLoading ? (
          <Card className="border-none">
            <CardContent className="p-12 flex flex-col items-center justify-center">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mb-4"></div>
              <p className="text-gray-500 font-medium font-inter">Loading your updates...</p>
            </CardContent>
          </Card>
        ) : filteredNotifications.length > 0 ? (
          Object.entries(groupedNotifications).map(([groupName, items]) => (
            items.length > 0 && (
              <div key={groupName} className="space-y-4">
                <h3 className="text-xs font-bold text-gray-400 uppercase tracking-widest px-1">{groupName}</h3>
                <Card className="rounded-2xl overflow-hidden border-gray-100 shadow-sm">
                  <CardContent className="p-0 divide-y divide-gray-100">
                    <AnimatePresence mode="popLayout">
                      {items.map((notification) => (
                        <motion.div
                          key={notification.id}
                          layout
                          initial={{ opacity: 0 }}
                          animate={{ opacity: 1 }}
                          exit={{ opacity: 0 }}
                          onClick={() => handleNotificationClick(notification)}
                          className={clsx(
                            "group relative flex gap-4 p-5 transition-all cursor-pointer",
                            !notification.isRead 
                              ? "bg-primary-50/30 hover:bg-primary-50/50" 
                              : "hover:bg-gray-50/50 bg-white"
                          )}
                        >
                          <div className={clsx(
                            "flex-shrink-0 w-12 h-12 rounded-xl flex items-center justify-center transition-all shadow-sm",
                            !notification.isRead 
                              ? "bg-primary-600 text-white group-hover:scale-110" 
                              : "bg-gray-100 text-gray-400"
                          )}>
                            {getNotificationIcon(notification.notificationType)}
                          </div>

                          <div className="flex-1 min-w-0">
                            <div className="flex items-start justify-between gap-3 mb-1">
                              <h4 className={clsx(
                                "text-sm font-bold truncate leading-tight",
                                !notification.isRead ? "text-gray-900" : "text-gray-600"
                              )}>
                                {notification.title}
                              </h4>
                              {!notification.isRead && (
                                <span className="flex-shrink-0 w-2 h-2 rounded-full bg-primary-600 ring-4 ring-primary-100 animate-pulse mt-1.5" />
                              )}
                            </div>
                            <p className={clsx(
                              "text-sm leading-snug line-clamp-2",
                              !notification.isRead ? "text-gray-700 font-medium" : "text-gray-500"
                            )}>
                              {notification.message}
                            </p>
                            <div className="mt-2 flex items-center gap-1.5 text-[11px] font-bold text-gray-400 uppercase tracking-wider">
                              <Clock className="w-3 h-3" />
                              {new Date(notification.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                              {groupName === 'Earlier' && ` • ${new Date(notification.createdAt).toLocaleDateString([], { day: '2-digit', month: 'short' })}`}
                            </div>
                          </div>

                          <div className="flex items-center gap-1.5 opacity-0 group-hover:opacity-100 transition-all pointer-events-auto">
                            {!notification.isRead && (
                              <button
                                onClick={(e) => handleMarkAsRead(e, notification.id)}
                                className="p-2 text-primary-600 hover:bg-white hover:shadow-sm rounded-lg transition-all"
                                title="Mark as read"
                              >
                                <CheckCircle className="w-5 h-5" />
                              </button>
                            )}
                            <button
                              onClick={(e) => handleDelete(e, notification.id)}
                              className="p-2 text-gray-400 hover:text-red-500 hover:bg-white hover:shadow-sm rounded-lg transition-all"
                              title="Delete notification"
                            >
                              <Trash2 className="w-5 h-5" />
                            </button>
                          </div>
                        </motion.div>
                      ))}
                    </AnimatePresence>
                  </CardContent>
                </Card>
              </div>
            )
          ))
        ) : (
          <EmptyState
            icon={<Bell className="w-16 h-16 text-gray-200" />}
            title={searchQuery ? "No matches found" : activeFilter === 'unread' ? "Zero unread alerts" : "No notifications"}
            description={searchQuery ? "Try searching for something else." : "You're all caught up for now!"}
          />
        )}
      </div>
    </div>
  )
}

export default NotificationsPage
