import { useEffect, useState } from 'react'
import Button from '../ui/Button'
import { getNotificationPreferences, updateNotificationPreferences, NotificationPreferenceDto } from '../../services/notificationPreferencesApi'

const categoryLabels: Record<string, string> = {
  SessionBooking: 'Session & booking updates',
  ChatRequests: 'Chat requests',
  EarningsPayouts: 'Earnings & payouts',
  PointsBonuses: 'Points & bonuses',
  EngagementReminders: 'Engagement & reminders',
  MarketingAnnouncements: 'Marketing & announcements',
}

const NotificationPreferences = () => {
  const [preferences, setPreferences] = useState<NotificationPreferenceDto[]>([])
  const [isSaving, setIsSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const loadPreferences = async () => {
      try {
        const data = await getNotificationPreferences()
        setPreferences(data)
      } catch (err: any) {
        setError(err.message || 'Failed to load preferences')
      }
    }
    loadPreferences()
  }, [])

  const toggle = (category: string, key: keyof NotificationPreferenceDto) => {
    setPreferences((prev) =>
      prev.map((pref) =>
        pref.category === category ? { ...pref, [key]: !pref[key] } : pref
      )
    )
  }

  const handleSave = async () => {
    setIsSaving(true)
    setError(null)
    try {
      await updateNotificationPreferences(preferences)
    } catch (err: any) {
      setError(err.message || 'Failed to save preferences')
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <div className="space-y-4">
      {error && <div className="text-sm text-red-600">{error}</div>}
      {preferences.map((pref) => {
        const label = categoryLabels[pref.category] || pref.category
        const isTransactional = pref.category === 'SessionBooking' || pref.category === 'ChatRequests'
        return (
          <div key={pref.category} className="border border-gray-200 rounded-lg p-4">
            <div className="font-medium text-gray-900">{label}</div>
            <p className="text-xs text-gray-600 mt-1">
              {isTransactional ? 'Transactional notifications are always ON.' : 'Customize your channels.'}
            </p>
            <div className="grid grid-cols-3 gap-3 mt-3 text-sm">
              <label className="flex items-center gap-2">
                <input
                  type="checkbox"
                  checked={pref.emailEnabled}
                  onChange={() => toggle(pref.category, 'emailEnabled')}
                  disabled={isTransactional}
                />
                Email
              </label>
              <label className="flex items-center gap-2">
                <input
                  type="checkbox"
                  checked={pref.whatsAppEnabled}
                  onChange={() => toggle(pref.category, 'whatsAppEnabled')}
                  disabled={isTransactional}
                />
                WhatsApp
              </label>
              <label className="flex items-center gap-2">
                <input
                  type="checkbox"
                  checked={pref.inAppEnabled}
                  onChange={() => toggle(pref.category, 'inAppEnabled')}
                  disabled={isTransactional}
                />
                In-App
              </label>
            </div>
          </div>
        )
      })}
      <div className="pt-2">
        <Button onClick={handleSave} isLoading={isSaving}>
          Save Notification Preferences
        </Button>
      </div>
    </div>
  )
}

export default NotificationPreferences
