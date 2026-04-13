import { useState, useEffect } from 'react'
import { Cookie, X, Settings } from 'lucide-react'
import Button from '../ui/Button'
import { Card } from '../ui/Card'
import { hasCookieConsent, saveCookieConsent, saveCookieConsentToStorage } from '../../services/consentApi'

interface CookieBannerProps {
  onConsentGiven?: () => void
}

const CookieBanner = ({ onConsentGiven }: CookieBannerProps) => {
  const [showBanner, setShowBanner] = useState(false)
  const [showCustomize, setShowCustomize] = useState(false)

  useEffect(() => {
    // Check if consent has been given
    if (!hasCookieConsent()) {
      setShowBanner(true)
    }
  }, [])

  const handleAcceptAll = async () => {
    const consent = {
      necessary: true,
      functional: true,
      analytics: true,
      marketing: true,
      consentGivenAt: new Date().toISOString(),
    }

    // Save to localStorage
    saveCookieConsentToStorage(consent)

    // Save to backend if user is logged in
    try {
      await saveCookieConsent({
        functional: true,
        analytics: true,
        marketing: true,
      })
    } catch (error) {
      // Silent fail for anonymous users
      console.error('Failed to save consent to backend:', error)
    }

    setShowBanner(false)
    onConsentGiven?.()
    
    // Trigger consent event for scripts
    window.dispatchEvent(new CustomEvent('cookieConsentUpdated', { detail: consent }))
  }

  const handleRejectNonEssential = async () => {
    const consent = {
      necessary: true,
      functional: false,
      analytics: false,
      marketing: false,
      consentGivenAt: new Date().toISOString(),
    }

    // Save to localStorage
    saveCookieConsentToStorage(consent)

    // Save to backend if user is logged in
    try {
      await saveCookieConsent({
        functional: false,
        analytics: false,
        marketing: false,
      })
    } catch (error) {
      console.error('Failed to save consent to backend:', error)
    }

    setShowBanner(false)
    onConsentGiven?.()
    
    // Trigger consent event
    window.dispatchEvent(new CustomEvent('cookieConsentUpdated', { detail: consent }))
  }

  if (!showBanner) return null

  return (
    <div className="fixed bottom-0 left-0 right-0 z-50 p-4 bg-white border-t border-gray-200 shadow-lg">
      <Card className="max-w-7xl mx-auto">
        <div className="p-6">
          <div className="flex items-start gap-4">
            <div className="flex-shrink-0">
              <div className="w-12 h-12 rounded-full bg-primary-100 flex items-center justify-center">
                <Cookie className="w-6 h-6 text-primary-600" />
              </div>
            </div>
            <div className="flex-1">
              <h3 className="text-lg font-semibold text-gray-900 mb-2">
                We Value Your Privacy
              </h3>
              <p className="text-sm text-gray-600 mb-4">
                We use cookies to enhance your browsing experience, serve personalized content, and analyze our traffic.
                By clicking "Accept All", you consent to our use of cookies. You can customize your preferences or
                reject non-essential cookies.
              </p>
              <div className="flex flex-wrap gap-3">
                <Button onClick={handleAcceptAll} size="sm">
                  Accept All
                </Button>
                <Button onClick={handleRejectNonEssential} variant="outline" size="sm">
                  Reject Non-Essential
                </Button>
                <Button
                  onClick={() => setShowCustomize(true)}
                  variant="ghost"
                  size="sm"
                  className="flex items-center gap-2"
                >
                  <Settings className="w-4 h-4" />
                  Customize Preferences
                </Button>
              </div>
            </div>
          </div>
        </div>
      </Card>

      {showCustomize && (
        <CookiePreferencesModal
          onClose={() => setShowCustomize(false)}
          onSave={async (consent) => {
            // Save to localStorage
            saveCookieConsentToStorage({
              ...consent,
              consentGivenAt: new Date().toISOString(),
            })

            // Save to backend
            try {
              await saveCookieConsent({
                functional: consent.functional,
                analytics: consent.analytics,
                marketing: consent.marketing,
              })
            } catch (error) {
              console.error('Failed to save consent to backend:', error)
            }

            setShowBanner(false)
            setShowCustomize(false)
            onConsentGiven?.()
            
            // Trigger consent event
            window.dispatchEvent(new CustomEvent('cookieConsentUpdated', { detail: consent }))
          }}
        />
      )}
    </div>
  )
}

interface CookiePreferencesModalProps {
  onClose: () => void
  onSave: (consent: { necessary: boolean; functional: boolean; analytics: boolean; marketing: boolean }) => void
}

const CookiePreferencesModal = ({ onClose, onSave }: CookiePreferencesModalProps) => {
  const [preferences, setPreferences] = useState({
    necessary: true, // Always true, locked
    functional: true,
    analytics: false,
    marketing: false,
  })

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <Card className="max-w-2xl w-full max-h-[90vh] overflow-y-auto">
        <div className="p-6">
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-2xl font-bold text-gray-900">Cookie Preferences</h2>
            <button
              onClick={onClose}
              className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
            >
              <X className="w-5 h-5 text-gray-500" />
            </button>
          </div>

          <div className="space-y-6 mb-6">
            {/* Necessary Cookies */}
            <div className="p-4 border border-gray-200 rounded-lg">
              <div className="flex items-center justify-between mb-2">
                <div>
                  <h3 className="font-semibold text-gray-900">Necessary Cookies</h3>
                  <p className="text-sm text-gray-600 mt-1">
                    Required for the website to function. Cannot be disabled.
                  </p>
                </div>
                <div className="flex items-center">
                  <input
                    type="checkbox"
                    checked={preferences.necessary}
                    disabled
                    className="w-5 h-5 text-primary-600 rounded border-gray-300"
                  />
                </div>
              </div>
              <div className="mt-2 text-xs text-gray-500">
                <p>• Authentication</p>
                <p>• Session management</p>
                <p>• Security</p>
              </div>
            </div>

            {/* Functional Cookies */}
            <div className="p-4 border border-gray-200 rounded-lg">
              <div className="flex items-center justify-between mb-2">
                <div>
                  <h3 className="font-semibold text-gray-900">Functional Cookies</h3>
                  <p className="text-sm text-gray-600 mt-1">
                    Enhance functionality and personalization.
                  </p>
                </div>
                <label className="relative inline-flex items-center cursor-pointer">
                  <input
                    type="checkbox"
                    checked={preferences.functional}
                    onChange={(e) =>
                      setPreferences({ ...preferences, functional: e.target.checked })
                    }
                    className="sr-only peer"
                  />
                  <div className="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-primary-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-primary-600"></div>
                </label>
              </div>
              <div className="mt-2 text-xs text-gray-500">
                <p>• Language preferences</p>
                <p>• User settings</p>
                <p>• Calendar settings</p>
              </div>
            </div>

            {/* Analytics Cookies */}
            <div className="p-4 border border-gray-200 rounded-lg">
              <div className="flex items-center justify-between mb-2">
                <div>
                  <h3 className="font-semibold text-gray-900">Analytics Cookies</h3>
                  <p className="text-sm text-gray-600 mt-1">
                    Help us understand how visitors interact with our website.
                  </p>
                </div>
                <label className="relative inline-flex items-center cursor-pointer">
                  <input
                    type="checkbox"
                    checked={preferences.analytics}
                    onChange={(e) =>
                      setPreferences({ ...preferences, analytics: e.target.checked })
                    }
                    className="sr-only peer"
                  />
                  <div className="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-primary-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-primary-600"></div>
                </label>
              </div>
              <div className="mt-2 text-xs text-gray-500">
                <p>• Usage tracking</p>
                <p>• Performance metrics</p>
              </div>
            </div>

            {/* Marketing Cookies */}
            <div className="p-4 border border-gray-200 rounded-lg">
              <div className="flex items-center justify-between mb-2">
                <div>
                  <h3 className="font-semibold text-gray-900">Marketing Cookies</h3>
                  <p className="text-sm text-gray-600 mt-1">
                    Used to deliver personalized advertisements.
                  </p>
                </div>
                <label className="relative inline-flex items-center cursor-pointer">
                  <input
                    type="checkbox"
                    checked={preferences.marketing}
                    onChange={(e) =>
                      setPreferences({ ...preferences, marketing: e.target.checked })
                    }
                    className="sr-only peer"
                  />
                  <div className="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-primary-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-primary-600"></div>
                </label>
              </div>
              <div className="mt-2 text-xs text-gray-500">
                <p>• Campaign tracking</p>
              </div>
            </div>
          </div>

          <div className="flex gap-3">
            <Button onClick={() => onSave(preferences)} fullWidth>
              Save Preferences
            </Button>
            <Button onClick={onClose} variant="outline" fullWidth>
              Cancel
            </Button>
          </div>
        </div>
      </Card>
    </div>
  )
}

export default CookieBanner
