import { useState, useEffect } from 'react'
import { Shield, Cookie, Calendar, LogOut, CheckCircle, XCircle, AlertCircle } from 'lucide-react'
import { Card, CardHeader, CardTitle, CardContent } from '../ui/Card'
import Button from '../ui/Button'
import { Badge } from '../ui/Badge'
import {
  getCookieConsent,
  updateCookieConsent,
  getUserConsents,
  revokeUserConsent,
  getCookieConsentFromStorage,
  saveCookieConsentToStorage,
} from '../../services/consentApi'
import { checkCalendarConnection } from '../../services/calendarApi'
import { useNavigate } from 'react-router-dom'

const ConsentManagement = () => {
  const navigate = useNavigate()
  const [cookieConsent, setCookieConsent] = useState<any>(null)
  const [userConsents, setUserConsents] = useState<any[]>([])
  const [isCalendarConnected, setIsCalendarConnected] = useState<boolean | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    loadConsents()
    checkCalendarStatus()
  }, [])

  const loadConsents = async () => {
    try {
      // Load cookie consent from localStorage first
      const localConsent = getCookieConsentFromStorage()
      if (localConsent) {
        setCookieConsent(localConsent)
      }

      // Try to load from backend (for logged-in users)
      try {
        const backendConsent = await getCookieConsent()
        if (backendConsent) {
          setCookieConsent(backendConsent)
        }
      } catch (error) {
        // User might not be logged in
      }

      // Load user consents (Google OAuth)
      try {
        const consents = await getUserConsents()
        setUserConsents(consents)
      } catch (error) {
        // User might not be logged in
      }
    } catch (error: any) {
      console.error('Failed to load consents:', error)
    }
  }

  const checkCalendarStatus = async () => {
    try {
      const connected = await checkCalendarConnection()
      setIsCalendarConnected(connected)
    } catch (error) {
      setIsCalendarConnected(false)
    }
  }

  const handleUpdateCookieConsent = async (updates: {
    functional?: boolean
    analytics?: boolean
    marketing?: boolean
  }) => {
    setIsLoading(true)
    setError(null)

    try {
      const updated = {
        ...cookieConsent,
        ...updates,
        consentUpdatedAt: new Date().toISOString(),
      }

      // Update localStorage
      saveCookieConsentToStorage(updated)
      setCookieConsent(updated)

      // Update backend
      try {
        await updateCookieConsent({
          functional: updated.functional,
          analytics: updated.analytics,
          marketing: updated.marketing,
        })
      } catch (error) {
        console.error('Failed to update consent in backend:', error)
      }

      // Trigger consent event
      window.dispatchEvent(new CustomEvent('cookieConsentUpdated', { detail: updated }))
    } catch (error: any) {
      setError(error.message || 'Failed to update cookie preferences')
    } finally {
      setIsLoading(false)
    }
  }

  const handleRevokeCalendarConsent = async () => {
    if (!confirm('Are you sure you want to disconnect Google Calendar? This will prevent you from creating or joining sessions.')) {
      return
    }

    setIsLoading(true)
    setError(null)

    try {
      await revokeUserConsent('GoogleCalendar')
      await loadConsents()
      await checkCalendarStatus()
      alert('Google Calendar disconnected. You will need to reconnect to use session features.')
    } catch (error: any) {
      setError(error.message || 'Failed to revoke calendar consent')
    } finally {
      setIsLoading(false)
    }
  }

  const calendarConsent = userConsents.find((c) => c.consentType === 'GoogleCalendar')
  const loginConsent = userConsents.find((c) => c.consentType === 'GoogleLogin')

  return (
    <div className="space-y-6">
      {error && (
        <div className="p-4 bg-red-50 border border-red-200 rounded-lg text-sm text-red-600">
          {error}
        </div>
      )}

      {/* Cookie Preferences */}
      <Card>
        <CardHeader>
          <div className="flex items-center gap-3">
            <Cookie className="w-6 h-6 text-primary-600" />
            <CardTitle>Cookie Preferences</CardTitle>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          {cookieConsent ? (
            <>
              <div className="space-y-3">
                <div className="flex items-center justify-between p-3 border border-gray-200 rounded-lg">
                  <div>
                    <p className="font-medium text-gray-900">Necessary Cookies</p>
                    <p className="text-sm text-gray-600">Required for website functionality</p>
                  </div>
                  <Badge variant="success">Always Enabled</Badge>
                </div>

                <div className="flex items-center justify-between p-3 border border-gray-200 rounded-lg">
                  <div>
                    <p className="font-medium text-gray-900">Functional Cookies</p>
                    <p className="text-sm text-gray-600">Language, preferences, calendar settings</p>
                  </div>
                  <label className="relative inline-flex items-center cursor-pointer">
                    <input
                      type="checkbox"
                      checked={cookieConsent.functional}
                      onChange={(e) =>
                        handleUpdateCookieConsent({ functional: e.target.checked })
                      }
                      disabled={isLoading}
                      className="sr-only peer"
                    />
                    <div className="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-primary-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-primary-600"></div>
                  </label>
                </div>

                <div className="flex items-center justify-between p-3 border border-gray-200 rounded-lg">
                  <div>
                    <p className="font-medium text-gray-900">Analytics Cookies</p>
                    <p className="text-sm text-gray-600">Usage tracking and performance metrics</p>
                  </div>
                  <label className="relative inline-flex items-center cursor-pointer">
                    <input
                      type="checkbox"
                      checked={cookieConsent.analytics}
                      onChange={(e) =>
                        handleUpdateCookieConsent({ analytics: e.target.checked })
                      }
                      disabled={isLoading}
                      className="sr-only peer"
                    />
                    <div className="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-primary-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-primary-600"></div>
                  </label>
                </div>

                <div className="flex items-center justify-between p-3 border border-gray-200 rounded-lg">
                  <div>
                    <p className="font-medium text-gray-900">Marketing Cookies</p>
                    <p className="text-sm text-gray-600">Campaign tracking</p>
                  </div>
                  <label className="relative inline-flex items-center cursor-pointer">
                    <input
                      type="checkbox"
                      checked={cookieConsent.marketing}
                      onChange={(e) =>
                        handleUpdateCookieConsent({ marketing: e.target.checked })
                      }
                      disabled={isLoading}
                      className="sr-only peer"
                    />
                    <div className="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-primary-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-primary-600"></div>
                  </label>
                </div>
              </div>

              <div className="pt-4 border-t border-gray-200 text-sm text-gray-600">
                <p>
                  Consent given: {new Date(cookieConsent.consentGivenAt).toLocaleString()}
                </p>
                {cookieConsent.consentUpdatedAt && (
                  <p>
                    Last updated: {new Date(cookieConsent.consentUpdatedAt).toLocaleString()}
                  </p>
                )}
              </div>
            </>
          ) : (
            <div className="text-center py-8 text-gray-500">
              <Cookie className="w-12 h-12 mx-auto mb-2 opacity-50" />
              <p>No cookie preferences set</p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Google Consents */}
      <Card>
        <CardHeader>
          <div className="flex items-center gap-3">
            <Shield className="w-6 h-6 text-primary-600" />
            <CardTitle>Google Account Access</CardTitle>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          {/* Google Login Consent */}
          <div className="p-4 border border-gray-200 rounded-lg">
            <div className="flex items-start justify-between mb-3">
              <div className="flex-1">
                <div className="flex items-center gap-2 mb-1">
                  <h3 className="font-semibold text-gray-900">Google Login</h3>
                  {loginConsent?.granted ? (
                    <Badge variant="success">
                      <CheckCircle className="w-3 h-3 mr-1" />
                      Connected
                    </Badge>
                  ) : (
                    <Badge variant="error">
                      <XCircle className="w-3 h-3 mr-1" />
                      Not Connected
                    </Badge>
                  )}
                </div>
                <p className="text-sm text-gray-600 mb-2">
                  Access to your basic profile information (email, name, profile picture) for
                  authentication purposes only.
                </p>
                {loginConsent?.grantedAt && (
                  <p className="text-xs text-gray-500">
                    Connected: {new Date(loginConsent.grantedAt).toLocaleString()}
                  </p>
                )}
              </div>
            </div>
            <div className="text-xs text-gray-500 bg-gray-50 p-2 rounded">
              <p className="font-medium mb-1">Scopes:</p>
              <ul className="list-disc list-inside space-y-1">
                <li>email</li>
                <li>profile</li>
              </ul>
            </div>
          </div>

          {/* Google Calendar Consent */}
          <div className="p-4 border border-gray-200 rounded-lg">
            <div className="flex items-start justify-between mb-3">
              <div className="flex-1">
                <div className="flex items-center gap-2 mb-1">
                  <Calendar className="w-4 h-4 text-primary-600" />
                  <h3 className="font-semibold text-gray-900">Google Calendar</h3>
                  {calendarConsent?.granted && isCalendarConnected ? (
                    <Badge variant="success">
                      <CheckCircle className="w-3 h-3 mr-1" />
                      Connected
                    </Badge>
                  ) : (
                    <Badge variant="error">
                      <XCircle className="w-3 h-3 mr-1" />
                      Not Connected
                    </Badge>
                  )}
                </div>
                <p className="text-sm text-gray-600 mb-2">
                  Required for scheduling sessions, creating Google Meet links, and managing your
                  calendar. This is mandatory to use session features.
                </p>
                {calendarConsent?.grantedAt && (
                  <p className="text-xs text-gray-500 mb-2">
                    Connected: {new Date(calendarConsent.grantedAt).toLocaleString()}
                  </p>
                )}
                {calendarConsent?.revokedAt && (
                  <p className="text-xs text-red-600 mb-2">
                    Revoked: {new Date(calendarConsent.revokedAt).toLocaleString()}
                  </p>
                )}
              </div>
            </div>
            <div className="text-xs text-gray-500 bg-gray-50 p-2 rounded mb-3">
              <p className="font-medium mb-1">Scopes:</p>
              <ul className="list-disc list-inside space-y-1">
                <li>https://www.googleapis.com/auth/calendar</li>
              </ul>
            </div>
            <div className="flex gap-3">
              {calendarConsent?.granted && isCalendarConnected ? (
                <Button
                  variant="outline"
                  size="sm"
                  onClick={handleRevokeCalendarConsent}
                  disabled={isLoading}
                >
                  <LogOut className="w-4 h-4 mr-2" />
                  Disconnect Calendar
                </Button>
              ) : (
                <Button
                  size="sm"
                  onClick={() => navigate('/calendar/connect')}
                >
                  <Calendar className="w-4 h-4 mr-2" />
                  Connect Google Calendar
                </Button>
              )}
            </div>
            {!calendarConsent?.granted && (
              <div className="mt-3 p-3 bg-yellow-50 border border-yellow-200 rounded-lg">
                <div className="flex items-start gap-2">
                  <AlertCircle className="w-4 h-4 text-yellow-600 flex-shrink-0 mt-0.5" />
                  <p className="text-sm text-yellow-800">
                    Google Calendar connection is required to create or join sessions. Please connect
                    your calendar to continue.
                  </p>
                </div>
              </div>
            )}
          </div>
        </CardContent>
      </Card>
    </div>
  )
}

export default ConsentManagement
