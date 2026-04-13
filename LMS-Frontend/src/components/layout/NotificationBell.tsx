import { useEffect, useState } from 'react'
import { Bell, Check } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import { getNotifications, getUnreadCount, markAllNotificationsRead, markNotificationRead, NotificationDto } from '../../services/notificationsApi'

const NotificationBell = () => {
  const [isOpen, setIsOpen] = useState(false)
  const [notifications, setNotifications] = useState<NotificationDto[]>([])
  const [unreadCount, setUnreadCount] = useState(0)
  const navigate = useNavigate()

  const loadNotifications = async () => {
    try {
      const [list, count] = await Promise.all([
        getNotifications({ page: 1, pageSize: 10 }),
        getUnreadCount(),
      ])
      setNotifications(list.items)
      setUnreadCount(count)
    } catch (err) {
      console.error('Failed to load notifications', err)
    }
  }

  useEffect(() => {
    loadNotifications()
  }, [])

  const handleNotificationClick = async (notification: NotificationDto) => {
    if (!notification.isRead) {
      await markNotificationRead(notification.id)
      setUnreadCount((prev) => Math.max(0, prev - 1))
    }
    if (notification.actionUrl) {
      navigate(notification.actionUrl)
    }
  }

  const handleMarkAllRead = async () => {
    await markAllNotificationsRead()
    setUnreadCount(0)
    setNotifications((prev) => prev.map((n) => ({ ...n, isRead: true })))
  }

  return (
    <div className="relative">
      <button
        onClick={() => {
          setIsOpen(!isOpen)
          if (!isOpen) {
            loadNotifications()
          }
        }}
        className="p-2 rounded-lg hover:bg-gray-100 transition-colors relative"
        aria-label="Notifications"
      >
        <Bell className="w-5 h-5 text-gray-700" />
        {unreadCount > 0 && (
          <span className="absolute -top-1 -right-1 bg-red-500 text-white text-[10px] px-1.5 py-0.5 rounded-full">
            {unreadCount}
          </span>
        )}
      </button>
      {isOpen && (
        <div className="absolute right-0 mt-2 w-80 bg-white rounded-lg shadow-lg border border-gray-200">
          <div className="flex items-center justify-between px-4 py-3 border-b border-gray-200">
            <span className="font-semibold text-gray-900">Notifications</span>
            <button
              type="button"
              onClick={handleMarkAllRead}
              className="text-xs text-primary-600 hover:text-primary-700 flex items-center gap-1"
            >
              <Check className="w-3 h-3" />
              Mark all read
            </button>
          </div>
          <div className="max-h-80 overflow-y-auto">
            {notifications.length === 0 ? (
              <div className="p-4 text-sm text-gray-500">No notifications</div>
            ) : (
              notifications.map((notification) => (
                <button
                  key={notification.id}
                  type="button"
                  onClick={() => handleNotificationClick(notification)}
                  className={`w-full text-left px-4 py-3 border-b border-gray-100 hover:bg-gray-50 ${
                    notification.isRead ? 'bg-white' : 'bg-blue-50'
                  }`}
                >
                  <p className="text-sm font-medium text-gray-900">{notification.title}</p>
                  <p className="text-xs text-gray-600 mt-1">{notification.message}</p>
                </button>
              ))
            )}
          </div>
        </div>
      )}
    </div>
  )
}

export default NotificationBell
